using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ChatPreviewDto
{
    public SenderInfo SenderInfo { get; set; }
    public ChatDto Chat { get; set; }
    public ChatThread Thread { get; set; }
}
