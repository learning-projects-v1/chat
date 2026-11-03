using System.Security.Claims;
using ChatApp.API.Controllers;
using ChatApp.Application.DTOs;
using ChatApp.Application.Events;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace ChatApp.UnitTests.Messages;

public class MessagesControllerTests
{
    private readonly IMessageRepository _messageRepository = Substitute.For<IMessageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRealTimeNotifier _notifier = Substitute.For<IRealTimeNotifier>();
    private readonly IChatThreadRepository _chatThreadRepository = Substitute.For<IChatThreadRepository>();
    private readonly IChatThreadMemberRepository _chatThreadMemberRepository = Substitute.For<IChatThreadMemberRepository>();
    private readonly IMessageSeenStatusRepository _messageSeenStatusRepository = Substitute.For<IMessageSeenStatusRepository>();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private readonly MessagesController _sut;

    public MessagesControllerTests()
    {
        _sut = new MessagesController(_messageRepository, _unitOfWork, _userRepository, _notifier,
            _chatThreadRepository, _chatThreadMemberRepository, _messageSeenStatusRepository, _eventPublisher);
    }

    [Fact]
    public async Task SendMessage_ThreadNotFound_ReturnsBadRequest()
    {
        // Arrange: sign in, the sender exists (it's looked up before the thread), the thread returns null
        var me = Guid.NewGuid();
        SignInAs(me);
        GivenSenderExists(me);
        // no GivenThreadExists: an unconfigured substitute returns null for GetItemByIdAsync
        // Act
        var result = await _sut.SendMessage(NewChat(Guid.NewGuid(), me));
        // Assert: BadRequestObjectResult · _messageRepository never got AddAsync
        Assert.IsType<BadRequestObjectResult>(result);
        await _messageRepository.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _messageSeenStatusRepository.DidNotReceive()
            .AddAsync(Arg.Any<MessageSeenStatus>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive()
            .PublishAsync(Arg.Any<MessageSent>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessage_Valid_SavesAndPublishesMessageSentForAllThreadMembers()
    {
        // Arrange: sign in, sender + thread exist, GetThreadMembersAsync returns 2 users
        var me = Guid.NewGuid();
        var friend = Guid.NewGuid();
        var threadId = Guid.NewGuid();
        SignInAs(me);
        GivenSenderExists(me);
        GivenThreadExists(threadId, me, friend);

        // Act
        var result = await _sut.SendMessage(NewChat(threadId, me));

        // Assert: OkObjectResult · SaveChangesAsync once · one MessageSent with both member ids,
        // published with the chat message id as its MessageId · no inline SignalR push any more
        var ok = Assert.IsType<OkObjectResult>(result);
        var sent = Assert.IsType<ChatDto>(ok.Value);
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<MessageSent>(e => e.MessageId == sent.Id
                                     && e.SenderId == me
                                     && e.ThreadId == threadId
                                     && e.ThreadMemberIds.Count == 2
                                     && e.ThreadMemberIds.Contains(me)
                                     && e.ThreadMemberIds.Contains(friend)),
            sent.Id.ToString(),
            Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().NotifyMessageToAll(Arg.Any<List<string>>(), Arg.Any<ChatPreviewDto>());
    }

    [Fact]
    public async Task SendMessage_UsesSenderFromToken_NotFromBody()
    {
        // Arrange: sign in as `me`, chat.SenderId = someone else, sender + thread exist
        var me = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var threadId = Guid.NewGuid();
        SignInAs(me);
        GivenSenderExists(me);
        GivenSenderExists(someoneElse);
        GivenThreadExists(threadId, me);
        // capture the saved message: Arg.Do must be wired up BEFORE the Act
        Message? saved = null;
        await _messageRepository.AddAsync(Arg.Do<Message>(m => saved = m), Arg.Any<CancellationToken>());

        // Act
        var result = await _sut.SendMessage(NewChat(threadId, someoneElse));

        // Assert: AddAsync got a Message with SenderId == me (fails until the controller is fixed)
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(saved);
        Assert.Equal(me, saved.SenderId);
    }

    [Fact]
    public async Task SendMessage_SenderNotInThread_ReturnsForbid()
    {
        var me = Guid.NewGuid();
        var threadId = Guid.NewGuid();
        SignInAs(me);
        GivenSenderExists(me);
        GivenThreadExists(threadId, me);
        SenderNotInThread(threadId, me);
        var result = await _sut.SendMessage(NewChat(threadId, me));
        Assert.IsType<ForbidResult>(result);
        await _messageRepository.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    private void SignInAs(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userId.ToString()) }, "Test");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private void GivenSenderExists(Guid userId) =>
        _userRepository.GetItemByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new User { Id = userId, UserName = "sender" });

    private void GivenThreadExists(Guid threadId, params Guid[] memberIds)
    {
        _chatThreadRepository.GetItemByIdAsync(threadId, Arg.Any<CancellationToken>())
            .Returns(new ChatThread { Id = threadId, ThreadName = "thread" });
        _chatThreadMemberRepository.GetThreadMembersAsync(threadId)
            .Returns(memberIds.Select(id => new User { Id = id, UserName = $"user-{id}" }).ToList());
        foreach(var member in memberIds)
        {
            _chatThreadMemberRepository.IsMember(threadId, member).Returns(true);
        }
    }

    private void SenderNotInThread(Guid threadId, Guid senderId)
    {
        _chatThreadMemberRepository.IsMember(threadId, senderId).Returns(false);
    }

    private static ChatDto NewChat(Guid threadId, Guid senderId) => new()
    {
        Content = "hello",
        ChatThreadId = threadId,
        SenderId = senderId,
        SentAt = DateTime.UtcNow
    };
}
