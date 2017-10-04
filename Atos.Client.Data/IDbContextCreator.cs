namespace Atos.Client.Data
{
	public interface IDbContextCreator
	{
		IDbContext Create();
	}
}