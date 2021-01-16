using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
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

namespace TG_bot
{
    public static class Program
    {
        private static TelegramBotClient Bot;

        public static async Task Main()
        {
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

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync("https://finance.tut.by/kurs/minsk/");
            var minskWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/minsk");
            var mogilevWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/mogilev/");
            var gomelWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/gomel/");
            var grodnoWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/grodno/");
            var brestWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/brest/");
            var vitebskWeatherHtml = await httpClient.GetStringAsync("https://pogoda.tut.by/city/vitebsk/");
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);
            var minskWeather = await parser.ParseDocumentAsync(minskWeatherHtml);
            var mogilevWeather = await parser.ParseDocumentAsync(mogilevWeatherHtml);
            var gomelWeather = await parser.ParseDocumentAsync(gomelWeatherHtml);
            var grodnoWeather = await parser.ParseDocumentAsync(grodnoWeatherHtml);
            var brestWeather = await parser.ParseDocumentAsync(brestWeatherHtml);
            var vitebskWeather = await parser.ParseDocumentAsync(vitebskWeatherHtml);
            var str = "#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp"; 
            var result = "Not Found";
            switch (callbackQuery.Data)
            {
                case "/usd":
                    result = document.QuerySelector("#content-band > div.col-c > div > div > div.b-cnt > table > tbody > tr:nth-child(1) > td:nth-child(2) > div > p").TextContent;
                    break;


                case "/eur":
                    result = document.QuerySelector("#content-band > div.col-c > div > div > div.b-cnt > table > tbody > tr:nth-child(3) > td:nth-child(2) > div > p").TextContent;
                    break;
            

               case "/rur":
                    result = document.QuerySelector("#content-band > div.col-c > div > div > div.b-cnt > table > tbody > tr:nth-child(4) > td.inctances > div > p:nth-child(2)").TextContent;
                    break;

                case "/grn":
                    result = document.QuerySelector("#content-band > div.col-c > div > div > div.b-cnt > table > tbody > tr:nth-child(7) > td.inctances > div > p:nth-child(2)").TextContent;
                    break;

                case "/minsk":
                    result = minskWeather.QuerySelector("#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp").TextContent;
                    break;

                case "/mogilev":
                    result = mogilevWeather.QuerySelector("#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp").TextContent;
                    break;

                case "/gomel":
                    result = gomelWeather.QuerySelector(str).TextContent;
                    break;

                case "/grodno":
                    result = grodnoWeather.QuerySelector("#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp").TextContent;
                    break;

                case "/brest":
                    result = brestWeather.QuerySelector("#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp").TextContent;
                    break;

                case "/vitebsk":
                    result = vitebskWeather.QuerySelector("#simple_mode > div > div.b-forecast-top__item.b-forecast-top--gen > div.b-forecast-top__wrap > div.b-forecast-top__temp").TextContent;
                    break;

            }
            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                result
            );
        }

        #region Inline Mode

        private static async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }
        #endregion

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