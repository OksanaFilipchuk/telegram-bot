using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;
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
        new BotCommand{ Command="lang", Description= "Змінити мову" },

    };
    private static readonly Context _dbContext = new Context();

    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.File("logs/botapp.txt", rollingInterval: RollingInterval.Day)
           .CreateLogger();


        var config = new ConfigurationBuilder().
            SetBasePath(Directory.GetCurrentDirectory()).
            AddJsonFile("appsettings.json");

        IConfigurationRoot configuration = config.Build();
        try
        {
            string token = configuration["Bot:Token"];

            botClient = new TelegramBotClient(token);
            var me = botClient.GetMe().Result;
            Console.WriteLine($"Bot id: {me.Id}, Bot Name: {me.FirstName}");
            botClient.SetMyCommands(commands);
            Log.Information($"Bot id: {me.Id}, Bot Name: {me.FirstName} is started");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the bot");
        }

        _dbContext.Database.EnsureCreated();

        UserService userService = new(_dbContext);
        ExpensesService expensesService = new(_dbContext);
        CategoryService categoryService = new(_dbContext);
        IAiProvider aiProvider = new AiOllamaProvider();
        if (botClient is not null)
        {
            var handler = new Handler(botClient, userService, expensesService, categoryService, aiProvider);

            botClient.StartReceiving(async (botClient, update, cancellationToken) =>
            {
                await handler.Handle(update, cancellationToken);

            }, ErrorHandler);
        }
        else
        {
            Console.WriteLine("Bot initialization failed");
            Log.Error("Bot initialization failed");
        }
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    public static async Task ErrorHandler(ITelegramBotClient botClient, Exception ex, HandleErrorSource handle, CancellationToken cancellationToken)
    {
        Console.WriteLine("Error happend");
    }
}