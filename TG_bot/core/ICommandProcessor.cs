using System.Threading.Tasks;

namespace TG_bot.Core
{
    public interface ICommandProcessor
    {
        Task<string> ProcessCommand();
    }
}
