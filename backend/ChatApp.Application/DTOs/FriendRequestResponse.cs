using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class FriendRequestResponse
{
    public UserResponse Sender { get; set; }
    public string Message { get; set; }
}
