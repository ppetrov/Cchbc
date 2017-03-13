namespace Cchbc.iFSA
{
	public interface IAppNavigator
	{
		void NavigateTo(AppScreen screen, object args);
		void GoBack();
	}
}