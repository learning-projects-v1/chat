using System.Security.Claims;
using ChatApp.API.Controllers;
using ChatApp.Application.DTOs;
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
    private readonly MessagesController _sut;

    public MessagesControllerTests()
    {
        _sut = new MessagesController(_messageRepository, _unitOfWork, _userRepository, _notifier,
            _chatThreadRepository, _chatThreadMemberRepository, _messageSeenStatusRepository);
    }

    [Fact]
    public async Task SendMessage_ThreadNotFound_ReturnsBadRequest()
    {
        // Arrange: sign in, the sender exists (it's looked up before the thread), the thread returns null
        SignInAs(Guid.NewGuid());
        GivenSenderExists(Guid.NewGuid());
        GivenThreadExists(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // Act: await _sut.SendMessage(chat)
        var result = await _sut.SendMessage(Arg.Any<ChatDto>());
        // Assert: BadRequestObjectResult · _messageRepository never got AddAsync
        Assert.IsType<BadRequestObjectResult>(result);
        await _messageRepository.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _messageSeenStatusRepository.DidNotReceive()
            .AddAsync(Arg.Any<MessageSeenStatus>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessage_Valid_SavesAndNotifiesAllThreadMembers()
    {
        // Arrange: sign in, sender + thread exist, GetThreadMembersAsync returns 2 users
        SignInAs(Guid.NewGuid());
        GivenSenderExists(Guid.NewGuid());
        GivenThreadExists(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // _userRepository.GetItemByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)new(){});
        // _chatThreadRepository.GetItemByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
        //     .Returns((ChatThread?)new(){});
        //
        // Act
        var result = await _sut.SendMessage(Arg.Any<ChatDto>());

        // Assert: OkObjectResult · SaveChangesAsync once · NotifyMessageToAll got both member ids
        Assert.IsType<OkObjectResult>(result);
    
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _notifier.Received(1).NotifyMessageToAll(Arg.Any<List<string>>(), Arg.Any<ChatPreviewDto>());
    }

    [Fact]
    public async Task SendMessage_UsesSenderFromToken_NotFromBody()
    {
        // Arrange: sign in as `me`, chat.SenderId = someone else, sender + thread exist
        var me = Guid.NewGuid();
        SignInAs(me);
        GivenThreadExists(Guid.NewGuid());
        // Act  
        var result = await _sut.SendMessage(new() { SenderId = Guid.NewGuid() });
        // Assert: AddAsync got a Message with SenderId == me (fails until the controller is fixed)

        Guid senderId = Guid.Empty;
        await _messageRepository.AddAsync(Arg.Do<Message>(x => senderId = x.SenderId));
        Assert.False(senderId == me);
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
    }

    private static ChatDto NewChat(Guid threadId, Guid senderId) => new()
    {
        Content = "hello",
        ChatThreadId = threadId,
        SenderId = senderId,
        SentAt = DateTime.UtcNow
    };
}
