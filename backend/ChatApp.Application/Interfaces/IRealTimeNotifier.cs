using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IRealTimeNotifier
{
    Task NotifyFriendRequest(string ReceiverId, object payload);
    Task NotifyMessage(string ReceiverId, object payload);

}
