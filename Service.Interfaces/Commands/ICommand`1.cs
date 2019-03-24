using System.Threading.Tasks;

namespace Service.Interfaces.Commands
{
	public interface ICommand<TArgument>
	{
		Task ExecuteAsync(TArgument argument);
	}
}
