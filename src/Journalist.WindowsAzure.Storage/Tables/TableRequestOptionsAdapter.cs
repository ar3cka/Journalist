using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class TableRequestOptionsAdapter : ITableRequestOptions
    {
        public TableRequestOptions Options { get; }

        public TableRequestOptionsAdapter()
        {
            Options = new TableRequestOptions();
        }

        public ITableRequestOptions ReadFromSecondaryLocation()
        {
            Options.LocationMode = LocationMode.SecondaryOnly;

            return this;
        }
    }
}
