using ChatApp.Domain.Models;
namespace ChatApp.Application.DTOs;

public class ChatPreviewDto
{
    public ChatDto Chat { get; set; }
    public UserInfoDto SenderInfo { get; set; }
}
