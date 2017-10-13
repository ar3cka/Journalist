using System;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTable
    {
        IBatchOperation PrepareBatchOperation();

        ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string rowKey, string[] properties);

        ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string rowKey, string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string[] properties);

        ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntityRangeQuery PrepareEntityFilterRangeQuery(string filter, string[] properties);

        ICloudTableEntityRangeQuery PrepareEntityFilterRangeQuery(string filter, string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, string[] properties);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, int count, string[] properties);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, int count, string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntityRangeQuery PrepareEntityGetAllQuery(string[] properties);

        ICloudTableEntityRangeQuery PrepareEntityGetAllQuery(string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(string[] properties);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(string[] properties, Action<ITableRequestOptions> setupOptions);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(int count, string[] properties);

        ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(int count, string[] properties, Action<ITableRequestOptions> setupOptions);
    }
}
