using Serilog;
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
    IAiProvider _AiProvider { get; set; }
    ITelegramBotClient _botClient { get; set; }

    public Handler(ITelegramBotClient botClient, UserService userService, ExpensesService expensesService, CategoryService categoryService, IAiProvider aiProvider)
    {
        _botClient = botClient;
        _userService = userService;
        _expensesService = expensesService;
        _categoryService = categoryService;
        _AiProvider = aiProvider;
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
        Language userLang = await _userService.GetUserLang(update);
        var data = update.CallbackQuery.Data;
        var user = await _userService.GetOrCreateUser(update);

        if (Enum.TryParse<CategoryType>(data, out var category))
        {
            await _userService.SetSelectedCategory(update, category);
            Log.Information("User {user.Id} selected cathegory have have been updated", user.Id);
            await botClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: _localizationService.Get("EnterExpenseAmount", userLang));
        }

        if (Enum.TryParse<Language>(data, out var language))
        {
            Log.Information("User {user.Id} language have been updated", user.Id);
            await _userService.SetUserLang(update, (Language)language);
            await botClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: _localizationService.Get("LanguageChangeMessage", language)
                );
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
        Log.Information("Stats message sent to user UserId={user.Id}", user.Id);

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
            var user = await _userService.GetOrCreateUser(update);
            var userLang = await _userService.GetUserLang(update);
            var selectedCategory = user.SelectedCategory;
            var amount = update.Message.Text.ParseAmount();

            if (amount != null && selectedCategory is not null)
            {
                var category = await _categoryService.GetCategory((CategoryType)selectedCategory);
                await _expensesService.AddExpense(user, category, amount.Value);
                await _userService.SetSelectedCategory(update, null);
                string[] par = new[] { _localizationService.Get(category.Key, userLang), amount.ToString() };
                await _botClient.SendMessage(
                   chatId: update.Message.Chat.Id,
                   text: _localizationService.Get("ExpensesUpdateMessage", userLang, par)
                   );
                Log.Information("Expense added: UserId={UserId}, Category={Category}, Amount={Amount}", user.Id, category.Key, amount.Value);
            }
            else
            {
                await _botClient.SendChatAction(
                    chatId: update.Message.Chat.Id,
                    action: ChatAction.Typing);

                var response = await _AiProvider.GetExpensesResponse(update.Message.Text);
                if (response is null)
                {
                    Log.Information("AI response is empty for UserId={UserId}", user.Id);
                    await _botClient.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: _localizationService.Get("WarnMessage", userLang));

                }
                else
                {
                    var category = await _categoryService.GetCategory((CategoryType)response.ExpensesCategory);
                    await _expensesService.AddExpense(user, category, response.Amount);
                    Log.Information("Expense added: UserId={UserId}, Category={Category}, Amount={Amount}", user.Id, category.Key, response.Amount);
                    string[] par = new[] { _localizationService.Get(category.Key, userLang), response.Amount.ToString() };
                    await _botClient.SendMessage(
                       chatId: update.Message.Chat.Id,
                       text: _localizationService.Get("ExpensesUpdateMessage", userLang, par)
                       );
                }
            }
        }
    }



}
