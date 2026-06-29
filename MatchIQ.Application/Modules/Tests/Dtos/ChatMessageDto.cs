using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Tests.Dtos;

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SendChatMessageDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
}
