using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Authorization;

public class ThreadMemberHandler : AuthorizationHandler<ThreadMemberRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ChatAppDbContext _context;
    private readonly IChatThreadRepository _threadRepository;
    private readonly IChatThreadMemberRepository _memberRepository;
    public ThreadMemberHandler(IHttpContextAccessor httpContextAccessor, ChatAppDbContext context, IChatThreadMemberRepository threadMemberRepository
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _memberRepository = threadMemberRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ThreadMemberRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
        var threadId = _httpContextAccessor.HttpContext.GetRouteValue("threadId")?.ToString();
        bool isMember =  await _memberRepository.IsMember(Guid.Parse(threadId), Guid.Parse(userId));
        if (isMember)
        {
            context.Succeed(requirement);
        }
    }
}
