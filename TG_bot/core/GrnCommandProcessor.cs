
using AngleSharp.Html.Parser;
using System.Net.Http;
using System.Threading.Tasks;

namespace TG_bot.Core
{
    public class GrnCommandProcessor : ICommandProcessor
    {
        public async Task<string> ProcessCommand()
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync("https://finance.tut.by/kurs/minsk/");
            var parser = new HtmlParser();

            var document = await parser.ParseDocumentAsync(html);

            return document.QuerySelector("#content-band > div.col-c > div > div > div.b-cnt > table > tbody > tr:nth-child(7) > td.inctances > div > p:nth-child(2)").TextContent;
        }
    }
}