using Telegram.Bot.Types;
using TelegramBot.Shared;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Sevices;

public class UserService
{
    Context _context { get; set; }
    public UserService(Context context)
    {
        _context = context;
    }

    public async Task<TelegramBot.Model.User> GetOrCreateUser(Update update)
    {
        var userTelegramId = update.Type == UpdateType.Message ? update.Message.From?.Id : update.CallbackQuery.From?.Id;
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == userTelegramId.Value);
        if (user == null)
        {
            user = new TelegramBot.Model.User
            {
                TelegramId = userTelegramId.Value,
                Name = update.Message?.From?.Username,
                Lang = Language.UA
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

        }
        return user;
    }

    public async Task<Language> GetUserLang(Update update) { 
        var user = await GetOrCreateUser(update);
        return user?.Lang ?? Language.UA;
    }


}
