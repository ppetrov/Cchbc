namespace Cchbc.AppBuilder.UI
{
	public sealed class Context
	{
		public static Core Core { get; } = new Core { ModalDialog = new ModalDialog() };
	}
}