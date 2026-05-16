using Microsoft.EntityFrameworkCore;
using TelegramBot.Model;
using TelegramBot.Shared;

namespace TelegramBot.Sevices;

public class ExpensesService
{
    Context _context { get; set; }
    public ExpensesService(Context context)
    {
        _context = context;
    }

    public async Task AddExpense(User user, ExpensesCategory category, decimal amount)
    {

        _context.Expenses.Add(new Expenses
        {
            User = user,
            UserId = user.Id,
            Category = category,
            CategoryId = category.Id,
            Amount = amount
        });


        await _context.SaveChangesAsync();
    }

    public async Task<List<ExpenseReport>> getExpanses(int userId, DateTime fromDate)
    {
        var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.CreatedAt >= fromDate)
                .ToListAsync();
        var grouped =  expenses
            .GroupBy(e => e.Category.Key)
            .Select(g => new ExpenseReport
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount),
                Percentage = Decimal.Round(g.Sum(x => x.Amount) / expenses.Sum(e => e.Amount) * 100, 2)
            })
            .ToList();
        return grouped;
    }
}

