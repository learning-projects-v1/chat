using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Constants;

public static class GlobalConstants
{
    public static string MessageReceived = "MessageReceived";
    public static string FriendRequestReceived = "FriendRequestReceived";
    public static string MessageAllNotification = "MessageAllNotification";
    public static string ReactionNotification = "ReactionNotification";
    public static string MessageSeenNotification = "MessageSeenNotification";

    public static class AuthorizationPolicy
    {
        public static string ThreadMember = "ThreadMember";
    }
}
