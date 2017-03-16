using System;

namespace iFSA
{
	public interface IAppNavigator
	{
		void NavigateTo(AppScreen screen, object args);
		void GoBack();
	}

	public interface IUIThreadDispatcher
	{
		void Dispatch(Action action);
	}
}