using System.ComponentModel.DataAnnotations;
using TelegramBot.Shared;

namespace TelegramBot.Model;

public class User
{
    [Key]
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string? Name { get; set; }
    public Language? Lang { get; set; }
}
