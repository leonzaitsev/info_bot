using System;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;


namespace useful_infoBot
{
    class Program
    {
        static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("1461641901:AAH7PHCU2erlPU8mZr8V_ca2ga5U7Cx9CpI");

            Bot.OnMessage += Bot_OnMessage;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void BotOnMessageRecivied(object sender,Telegram.Bot.Args.CallbackQueryEventArgs e)
        //обработка  InlineKeyboardButton.WithCallbackData
        {
            string buttonText = e.CallbackQuery.Data;

            await Bot.AnswerCallbackQueryAsync()

        }
        

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            // string name = $"{message.From.FirstName}";

            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start - запуск бота
/getWeather - прогноз погоды
/getCurrency - курсы валют";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                case "/getWeather":
                    var InlineKeyboard1 = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Погода в Минске"),
                        InlineKeyboardButton.WithCallbackData("Погода в Могилёве")
                        },
                         new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Погода в Гомеле"),
                        InlineKeyboardButton.WithCallbackData("Погода в Гродно")
                        },
                         new[]{
                        InlineKeyboardButton.WithCallbackData("Погода в Бресте"),
                        InlineKeyboardButton.WithCallbackData("Погода в Витебске")
                        }
            });
                    await Bot.SendTextMessageAsync(message.From.Id,"Выберите пункт меню", replyMarkup: InlineKeyboard1);
                    break;

                case "/getCurrency":
                    var InlineKeyboard2 = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Курс доллара"),
                        InlineKeyboardButton.WithCallbackData("Курс евро")
                        },
                         new[]
                        {
                        InlineKeyboardButton.WithCallbackData("Курс рф рубля"),
                        InlineKeyboardButton.WithCallbackData("Курс гривны")
                        }
                        
            });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню", replyMarkup: InlineKeyboard2);
                   break;

                default:
                    break;
                       
            }
        }
    }
} 
 