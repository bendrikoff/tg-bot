using Microsoft.VisualBasic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    // Создаем экземпляр TelegramBotClient, передавая токен бота
    static ITelegramBotClient botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("BOT_TOKEN"));
    static async Task Main()
    {
        Console.WriteLine("Запуск бота...");

        using var cts = new CancellationTokenSource();

        // Настройка приемника обновлений
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Получаем все типы обновлений
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Бот {me.Username} запущен и готов к приему сообщений.");

        Console.ReadLine();
        await Task.Delay(Timeout.Infinite);
    }

    // Обработка входящих сообщений
    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Проверяем, что обновление содержит сообщение
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        Console.WriteLine($"Получено сообщение от пользователя: {messageText}");

        // Простой ответ на сообщение
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Вы отправили: {messageText}",
            cancellationToken: cancellationToken
        );
    }

    // Обработка ошибок
    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Ошибка Telegram API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
