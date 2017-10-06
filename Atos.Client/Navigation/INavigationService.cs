using System.Threading.Tasks;

namespace Atos.Client.Navigation
{
	public interface INavigationService
	{
		Task NavigateToAsync<TViewModel>() where TViewModel : ScreenViewModel;
		Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : ScreenViewModel;
		void GoBack();
	}
}