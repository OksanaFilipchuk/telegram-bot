using System.ComponentModel.DataAnnotations;


namespace TelegramBot.Model;

public class Expenses
{
    [Key]
    public int Id { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public ExpensesCategory Category { get; set; } = null!;
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
