namespace Atos.Features.ExceptionsModule
{
	public sealed class ExceptionsSettings
	{
		public static readonly ExceptionsSettings Default = new ExceptionsSettings(10, true);

		public int MaxExceptionEntries { get; }
		public bool RemoveExcluded { get; }

		public ExceptionsSettings(int maxExceptionEntries, bool removeExcluded)
		{
			this.MaxExceptionEntries = maxExceptionEntries;
			this.RemoveExcluded = removeExcluded;
		}
	}
}