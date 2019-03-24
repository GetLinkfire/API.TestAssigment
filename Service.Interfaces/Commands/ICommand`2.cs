using System.Threading.Tasks;

namespace Service.Interfaces.Commands
{
    /// <summary>
    /// Command with Payload and Arguments
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    /// <typeparam name="TArgument"></typeparam>
    public interface ICommand<TPayload, TArgument>
    {
        Task<TPayload> ExecuteAsync(TArgument argument);
    }
}
