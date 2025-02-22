using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Misha_s_bot.DeepSeek;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Misha_s_bot
{
    class Program
    {
        // Создаем экземпляр TelegramBotClient, передавая токен бота
        static ITelegramBotClient botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("BOT_TOKEN"));

        private static DeepSeekHandler _deepSeekHandler;
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

            var deepSeek = new DeepSeekHandler();
            _deepSeekHandler = deepSeek;

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
            if (chatId != -160830561 && chatId != 324234993) return;

            Console.WriteLine($"Чат: {chatId}.Получено сообщение от пользователя: {messageText}");

            await HandleVoice(update.Message);
            await HandleText(update.Message);

            var user = update.Message.From;

            long userId = user.Id;
            string userName = user.Username;

            // Формируем строку для записи
            string record = $"{userId}-{userName}";

            AddUserRecordToFile(record);
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

        // Метод для добавления записи в файл, если запись уникальна
        private static void AddUserRecordToFile(string record)
        {
            string filePath = "user_records.txt"; // Путь к файлу

            // Если файл существует, считываем все записи
            HashSet<string> existingRecords = new HashSet<string>();
            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    existingRecords.Add(line);
                }
            }

            // Если записи уникальны, добавляем в файл
            if (!existingRecords.Contains(record))
            {
                System.IO.File.AppendAllLines(filePath, new[] { record });
                Console.WriteLine($"Запись добавлена: {record}");
            }
        }

        static async Task HandleVoice(Message message)
        {

            if (message.Text.ToLower().Contains("воронежс"))
            {
                await SendVoiceFile(Voices.OhYes, message.Chat.Id);
            }


            if (message.Text.Contains("бублик"))
            {
                await SendVoiceFile(Voices.Bublik, message.Chat.Id);
            }
        }

        static async Task HandleText(Message message)
        {
            var reply = new ReplyParameters();
            reply.MessageId = message.MessageId;

            if (message.Text == "/info@kto_takoy_misha_bot")
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"{message.From.FirstName} иди нахуй \uD83D\uDD95\uD83C\uDF69\uD83D\uDCA5",
                    replyParameters: reply
                );
            }

            if (message.Text.ToLower().Contains("миш"))
            {
                await botClient.SendTextMessageAsync(
                 chatId: message.Chat.Id,
                 text: await _deepSeekHandler.DeserializeAsync(_deepSeekHandler.HandleMessage(message.Text, Users.Names[message.From.Id])),
                 replyParameters: reply
                 );
            }

            if (message.ReplyToMessage != null && message.ReplyToMessage.From.Id == (await botClient.GetMeAsync()).Id)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: await _deepSeekHandler.DeserializeAsync(_deepSeekHandler.HandleMessage(message.Text, Users.Names[message.From.Id], message.ReplyToMessage.Text)),
                    replyParameters: reply
                );
            }
        }

        static async Task SendVoiceFile(Voices voice, long chatId)
        {
            try
            {
                var filePath = VoiceLoader.VoicesPath[voice];

                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var file = new InputFileStream(stream, "voice.ogg");
                    await botClient.SendVoiceAsync(chatId, file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}