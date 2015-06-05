using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ITableEntityConverter
    {
        DynamicTableEntity CreateDynamicTableEntityFromProperties(IReadOnlyDictionary<string, object> properties);

        Dictionary<string, object> CreatePropertiesFromDynamicTableEntity(DynamicTableEntity entity);
    }
}