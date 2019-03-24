using System.Threading.Tasks;

namespace Service.Interfaces.Commands
{
    /// <summary>
    /// Command with argument
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
	public interface ICommand<TArgument>
	{
		Task ExecuteAsync(TArgument argument);
	}
}
