using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Extention;
using TelegramBot.Sevices;
using TelegramBot.Shared;

namespace TelegramBot;

public class Handler
{
    private static LocalizationService _localizationService = new LocalizationService();
    UserService _userService { get; set; }
    ExpensesService _expensesService { get; set; }
    CategoryService _categoryService { get; set; }
    ITelegramBotClient _botClient { get; set; }

    public Handler(ITelegramBotClient botClient, UserService userService, ExpensesService expensesService, CategoryService categoryService)
    {
        _botClient = botClient;
        _userService = userService;
        _expensesService = expensesService;
        _categoryService = categoryService;
    }

    public async Task Handle(Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackQueryType(_botClient, update, cancellationToken);
            return;
        }
        if (update.Type == UpdateType.Message)

        {
            await HandleMessageType(update, cancellationToken);
            return;
        }
    }
    public async Task HandleCallbackQueryType(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var userLang = await _userService.GetUserLang(update);
        var data = update.CallbackQuery.Data;
        if (Enum.TryParse<CategoryType>(data, out var category))
        {
            await _userService.SetSelectedCategory(update, category);
            await botClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: _localizationService.Get("EnterExpenseAmount", userLang));
        }
        if (Enum.TryParse<Language>(data, out var language))
        {
            await _userService.SetUserLang(update, (Language)language);
           
        }
    }


    public async Task HandleMessageType(Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
        {
            var lang = await _userService.GetUserLang(update);
            switch (update.Message.Text)
            {
                case Command.Start:
                    await _botClient.SendMessage(
                    chatId: update.Message.Chat.Id,
                    text: _localizationService.Get("StartMessage", lang),
                    cancellationToken: cancellationToken);
                    break;
                case Command.Add:
                    await HandleAddCommand(update);
                    break;
                case Command.Stats:
                    await HandleStatsCommand(update);
                    break;
                case Command.Lang:
                    await HandleLangCommand(update);
                    break;
                default:
                    await HandleText(update);
                    break;
            }
        }
    }

    public async Task HandleStatsCommand(Update update)
    {
        long? userId = update.Message.From?.Id;
        var user = await _userService.GetOrCreateUser(update);
        var lang = await _userService.GetUserLang(update);

        var fromDate = DateTime.Now.AddMonths(-1);

        var expenses = await _expensesService.getExpanses(user.Id, fromDate);
        var message = VisualisationService.GetExpanseReportMessage(expenses, lang);

        await _botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: message,
                parseMode: ParseMode.Html
                );

    }

    public async Task HandleAddCommand(Update update)
    {
        var categories = Enum.GetNames<CategoryType>();
        var lang = await _userService.GetUserLang(update);

        var keyboard = new InlineKeyboardMarkup
            (
            categories
                .Select((cat, index) => new
                {
                    Button = InlineKeyboardButton.
            WithCallbackData(
                        _localizationService.Get(cat, lang), cat
                    ),
                    Index = index
                })
                .GroupBy(x => x.Index / 3)
                .Select(g => g.Select(x => x.Button).ToArray())
                .ToArray()
                    );

        await _botClient.SendMessage(
            chatId: update.Message.Chat.Id,
            text: _localizationService.Get("ChooseCategory", lang),
            replyMarkup: keyboard
        );
    }

    public async Task HandleLangCommand(Update update)
    {
        var lang = await _userService.GetUserLang(update);
        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard = new[]
         {
             new[] { InlineKeyboardButton.WithCallbackData("En", "EN") },
             new[] { InlineKeyboardButton.WithCallbackData("Укр", "UA") }
         }
        };

        await _botClient.SendMessage(chatId: update.Message.Chat.Id,
            text: _localizationService.Get("ChooseLanguage", lang),
            replyMarkup: keyboard);
    }



    public async Task HandleText(Update update)
    {
        if (update.Message is not null)
        {
            var userId = update.Message.From?.Id;
            var amount = update.Message.Text.ParseAmount();
            if (amount != null && userId is not null)
            {
                var user = await _userService.GetOrCreateUser(update);
                var selectedCategory = user.SelectedCategory;
                var category = await _categoryService.GetCategory((CategoryType)selectedCategory);
                await _expensesService.AddExpense(user, category, amount.Value);
            }
        }
    }



}
