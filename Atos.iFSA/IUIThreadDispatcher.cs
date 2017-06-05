using System;

namespace Atos.iFSA
{
	public interface IUIThreadDispatcher
	{
		void Dispatch(Action operation);
	}
}