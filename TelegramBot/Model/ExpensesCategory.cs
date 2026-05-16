using System.ComponentModel.DataAnnotations;

namespace TelegramBot.Model;

public class ExpensesCategory
{
    [Key]
    public int Id { get; set; }
    public string Key { get; set; } = String.Empty;
}
