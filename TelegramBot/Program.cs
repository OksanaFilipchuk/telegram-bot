using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBot;
using TelegramBot.Sevices;
using TelegramBot.Shared;


class Program
{
    private static TelegramBotClient botClient;
    private static BotCommand[] commands = new[]{

        new BotCommand { Command="start", Description="Запустити бота"},
        new BotCommand{ Command="add", Description= "Додати витрати" },
        new BotCommand{ Command="stats", Description= "Статистика за місяць" },

    };
    private static readonly Context _dbContext = new Context();

    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().
            SetBasePath(Directory.GetCurrentDirectory()).
            AddJsonFile("appsettings.json");

        IConfigurationRoot configuration = config.Build();

        string token = configuration["Bot:Token"];

        botClient = new TelegramBotClient(token);

        var me = botClient.GetMe().Result;
        Console.WriteLine($"Bot id: {me.Id}, Bot Name: {me.FirstName}");
        botClient.SetMyCommands(commands);
        _dbContext.Database.EnsureCreated();

        botClient.StartReceiving(static async (botClient, update, cancellationToken) =>
        {
            UserService userService = new(_dbContext);
            ExpensesService expensesService = new(_dbContext);
            CategoryService categoryService = new(_dbContext);
            var handler = new Handler(botClient, cancellationToken, userService, expensesService, categoryService);
            await handler.Handle(update);
        }, ErrorHandler);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }




    public static async Task ErrorHandler(ITelegramBotClient botClient, Exception ex, HandleErrorSource handle, CancellationToken cancellationToken)
    {
        Console.WriteLine("Error happend");
    }
}