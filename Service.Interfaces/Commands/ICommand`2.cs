using System.Threading.Tasks;

namespace Service.Interfaces.Commands
{
    public interface ICommand<TPayload, TArgument>
    {
        Task<TPayload> ExecuteAsync(TArgument argument);
    }
}
