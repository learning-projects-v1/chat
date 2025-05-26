using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; }
    public string Paswword { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
}
