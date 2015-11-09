using System.Threading.Tasks;

namespace Cchbc.Features.Db
{
	public interface IDbFeaturesManager
	{
		Task LoadAsync();
		Task SaveAsync(FeatureEntry featureEntry);
		Task SaveAsync(ExceptionEntry exceptionEntry);
	}
}