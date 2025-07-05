using ChatApp.Domain.Models;
namespace ChatApp.Application.DTOs;

public class ChatPreviewDto
{
    public ChatDto Chat { get; set; }
    public ChatThread Thread { get; set; }
}
