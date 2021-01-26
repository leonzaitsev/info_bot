using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TG_bot.Core;

namespace TG_bot
{
    public static class Program
    {
        private static TelegramBotClient Bot;

        public static async Task Main()
        {

            commandProcessors.Add("usd", new UsdCommandProcessor());
            commandProcessors.Add("eur", new EurCommandProcessor());
            commandProcessors.Add("rur", new RurCommandProcessor());
            commandProcessors.Add("grn", new GrnCommandProcessor());

#if USE_PROXY
            var Proxy = new WebProxy(Configuration.Proxy.Host, Configuration.Proxy.Port) { UseDefaultCredentials = true };
            Bot = new TelegramBotClient(Configuration.BotToken, webProxy: Proxy);
#else
            Bot = new TelegramBotClient("1461641901:AAH7PHCU2erlPU8mZr8V_ca2ga5U7Cx9CpI");
#endif

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token
            );

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start - запуск бота
/weather - прогноз погоды
/currency - курсы валют";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                case "/weather":
                    var InlineKeyboard1 = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Погода в Минске", "/minsk"),
                        InlineKeyboardButton.WithCallbackData("Погода в Могилёве","/mogilev")
                        },
                         new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Погода в Гомеле" ,"/gomel"),
                        InlineKeyboardButton.WithCallbackData("Погода в Гродно" , "/grodno")
                        },
                         new[]{
                        InlineKeyboardButton.WithCallbackData("Погода в Бресте","/brest"),
                        InlineKeyboardButton.WithCallbackData("Погода в Витебске", "/vitebsk")
                        }
            });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню", replyMarkup: InlineKeyboard1);
                    break;

                case "/currency":
                    var InlineKeyboard2 = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Курс доллара", "/usd"),
                        InlineKeyboardButton.WithCallbackData("Курс евро", "/eur")
                        },
                         new[]
                        {
                        InlineKeyboardButton.WithCallbackData("100 РФ рублей", "/rur"),
                        InlineKeyboardButton.WithCallbackData(" 100 гривень", "/grn")
                        }

            });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню", replyMarkup: InlineKeyboard2);
                    break;

                default:
                    break;

            }
        }

        private static IDictionary<string, ICommandProcessor> commandProcessors = new Dictionary<string, ICommandProcessor>();

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var result = "Not Found";
            if (commandProcessors.ContainsKey(callbackQuery.Data))
            {
                var commandProcessor = commandProcessors[callbackQuery.Data];
                result = await commandProcessor.ProcessCommand();
            }
            await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    result
                );

        }

       private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }


        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
     
    }
}