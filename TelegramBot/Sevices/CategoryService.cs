using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using TelegramBot.Shared;
using Telegram.Bot.Types;
using TelegramBot.Model;

namespace TelegramBot.Sevices
{
    public class CategoryService
    {
        Context _context { get; set; }
        public CategoryService(Context context)
        {
            _context = context;
        }
        public async Task<ExpensesCategory> GetCategory(CategoryType currentCategory) {
            string currentCategoryName = currentCategory.ToString();

            var category = _context.ExpensesCategory.FirstOrDefault(c => c.Key == currentCategoryName);
            if (category == null)
            {
                category = new ExpensesCategory
                {
                    Key = currentCategoryName
                };
                _context.ExpensesCategory.Add(category);
                await _context.SaveChangesAsync();
            }
            return category;
        }
    }
}
