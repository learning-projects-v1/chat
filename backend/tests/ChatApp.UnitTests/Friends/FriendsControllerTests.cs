using System.Security.Claims;
using ChatApp.API.Controllers;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace ChatApp.UnitTests.Friends;

public class FriendsControllerTests
{
    private readonly IFriendshipRepository _friendshipRepository = Substitute.For<IFriendshipRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRealTimeNotifier _notifier = Substitute.For<IRealTimeNotifier>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly FriendsController _sut;

    public FriendsControllerTests()
    {
        _sut = new FriendsController(_friendshipRepository, _unitOfWork, _notifier, _userRepository);
    }

    [Fact]
    public async Task SendFriendRequest_ToSelf_ReturnsBadRequest()
    {
        var me = Guid.NewGuid();
        SignInAs(me);

        var result = await _sut.SendFriendRequestAsync(me.ToString());

        Assert.IsType<BadRequestObjectResult>(result);
        await _friendshipRepository.DidNotReceive().AddFriendRequestAsync(Arg.Any<Friendship>());
    }

    [Fact]
    public async Task SendFriendRequest_AlreadyPending_ReturnsBadRequest_AndDoesNotSave()
    {
        var me = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SignInAs(me);
        _friendshipRepository.GetFriendshipAsync(me, receiver)
            .Returns(new Friendship { Status = FriendshipStatus.Pending });

        var result = await _sut.SendFriendRequestAsync(receiver.ToString());

        Assert.IsType<BadRequestObjectResult>(result);
        await _friendshipRepository.DidNotReceive().AddFriendRequestAsync(Arg.Any<Friendship>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task AcceptFriendRequest_NotPending_ReturnsBadRequest()
    {
        var me = Guid.NewGuid();
        var sender = Guid.NewGuid();
        SignInAs(me);
        _friendshipRepository.GetFriendshipAsync(sender, me)
            .Returns(new Friendship { Status = FriendshipStatus.Accepted });

        var result = await _sut.AcceptFriendRequestAsync(sender.ToString());

        Assert.IsType<BadRequestObjectResult>(result);
        _friendshipRepository.DidNotReceive().ApproveFriendRequest(Arg.Any<Friendship>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task AcceptFriendRequest_Pending_SetsAccepted_AndSaves()
    {
        var me = Guid.NewGuid();
        var sender = Guid.NewGuid();
        SignInAs(me);
        _friendshipRepository.GetFriendshipAsync(sender, me)
            .Returns(new Friendship { Status = FriendshipStatus.Pending });

        var result = await _sut.AcceptFriendRequestAsync(sender.ToString());

        Assert.IsType<OkObjectResult>(result);
        _friendshipRepository.Received(1).ApproveFriendRequest(Arg.Is<Friendship>(f => f.Status == FriendshipStatus.Accepted));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    private void SignInAs(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userId.ToString()) }, "Test");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext{User = new ClaimsPrincipal(identity)}
        };
    }
}
