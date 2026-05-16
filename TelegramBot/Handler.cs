using System.Text;
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
    private static CategoryType? currentCategory;
    private static LocalizationServise _localizationService = new LocalizationServise();
    UserService _userService { get; set; }
    ExpensesService _expensesService { get; set; }
    CategoryService _categoryService { get; set; }
    ITelegramBotClient _botClient { get; set; }
    CancellationToken _cancellationToken { get; set; }

    public Handler(ITelegramBotClient botClient, CancellationToken cancellationToken, UserService userService, ExpensesService expensesService, CategoryService categoryService)
    {
        _botClient = botClient;
        _cancellationToken = cancellationToken;
        _userService = userService;
        _expensesService = expensesService;
        _categoryService = categoryService;
    }

    public async Task Handle(Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackQueryType(_botClient, update, _cancellationToken);
            return;
        }
        if (update.Type == UpdateType.Message)

        {
            await HandleMessageType(update, _cancellationToken);
            return;
        }
    }
    public async Task HandleCallbackQueryType(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var lang = await _userService.GetUserLang(update);
        var data = update.CallbackQuery.Data;
        if (Enum.TryParse<CategoryType>(data, out var category))
        {
            currentCategory = category;
            await botClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: _localizationService.Get("EnterExpenseAmount", lang));
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
    public async Task HandleText(Update update)
    {
        if (update.Message is not null)
        {
            var userId = update.Message.From?.Id;
            var amount = update.Message.Text.ParseAmount();
            if (currentCategory != null && amount != null && userId is not null)
            {
                var user = await _userService.GetOrCreateUser(update);
                var category = await _categoryService.GetCategory((CategoryType)currentCategory);
                await _expensesService.AddExpense(user, category, amount.Value);
            }
        }
    }



}
