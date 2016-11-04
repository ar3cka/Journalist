using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ITableEntityConverter
    {
        DynamicTableEntity CreateDynamicTableEntityFromProperties(IReadOnlyDictionary<string, object> properties);

        DynamicTableEntity CreateDynamicTableEntityFromProperties(string propertyName, object propertyValue);

        Dictionary<string, object> CreatePropertiesFromDynamicTableEntity(DynamicTableEntity entity);
    }
}
