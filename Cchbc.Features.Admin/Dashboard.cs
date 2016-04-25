using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureCountsModule;

namespace Cchbc.Features.Admin
{
    public sealed class Dashboard
    {
        public FeatureUsagesViewModel FeatureUsagesViewModel { get; }

        public Dashboard(ITransactionContextCreator contextCreator)
        {
            if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

            this.FeatureUsagesViewModel = new FeatureUsagesViewModel(contextCreator, null);
        }

        public void Load()
        {
            // TODO : !!!
        }

    }
}