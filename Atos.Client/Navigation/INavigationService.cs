using System.Threading.Tasks;

namespace Atos.Client.Navigation
{
	public interface INavigationService
	{
		Task NavigateToAsync<TViewModel>() where TViewModel : PageViewModel;
		Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : PageViewModel;
		void GoBack();
	}
}