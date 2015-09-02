using System;
using System.Collections.Generic;
using System.IO;
using Journalist.Extensions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables.TableEntityConverters
{
    public class TableEntityConverter : ITableEntityConverter
    {
        public Dictionary<string, object> CreatePropertiesFromDynamicTableEntity(DynamicTableEntity entity)
        {
            var result = new Dictionary<string, object>();

            foreach (var entityProperty in entity.Properties)
            {
                object value;
                switch (entityProperty.Value.PropertyType)
                {
                    case EdmType.String:
                        value = entityProperty.Value.StringValue;
                        break;

                    case EdmType.Binary:
                        value = ReadBinaryEntityProperty(entityProperty.Value);
                        break;

                    case EdmType.Boolean:
                        value = entityProperty.Value.BooleanValue;
                        break;

                    case EdmType.DateTime:
                        value = entityProperty.Value.DateTimeOffsetValue;
                        break;

                    case EdmType.Double:
                        value = entityProperty.Value.DoubleValue;
                        break;

                    case EdmType.Guid:
                        value = entityProperty.Value.GuidValue;
                        break;

                    case EdmType.Int32:
                        value = entityProperty.Value.Int32Value;
                        break;

                    case EdmType.Int64:
                        value = entityProperty.Value.Int64Value;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Reading property type '{0}' not supported.".FormatString(entityProperty.Value.PropertyType));
                }

                if (value != null)
                {
                    result.Add(entityProperty.Key, value);
                }
            }

            result.Add(KnownProperties.PartitionKey, entity.PartitionKey);
            result.Add(KnownProperties.RowKey, entity.RowKey);
            result.Add(KnownProperties.ETag, entity.ETag);
            result.Add(KnownProperties.Timestamp, entity.Timestamp);

            return result;
        }

        public DynamicTableEntity CreateDynamicTableEntityFromProperties(IReadOnlyDictionary<string, object> properties)
        {
            var result = new DynamicTableEntity();

            foreach (var property in properties)
            {
                switch (property.Value.GetType().FullName)
                {
                    case "System.String":
                        result.Properties.Add(property.Key, new EntityProperty((string)property.Value));
                        break;

                    case "Journalist.IO.EmptyMemoryStream+NotDisposableEmptyMemoryStream":
                    case "System.IO.MemoryStream":
                        var stream = (MemoryStream)property.Value;
                        result.Properties.Add(property.Key, WriteBinaryEntityProperty(stream));
                        break;

                    case "System.Boolean":
                        result.Properties.Add(property.Key, new EntityProperty((bool)property.Value));
                        break;

                    case "System.DateTimeOffset":
                        result.Properties.Add(property.Key, new EntityProperty((DateTimeOffset)property.Value));
                        break;

                    case "System.Double":
                        result.Properties.Add(property.Key, new EntityProperty((double)property.Value));
                        break;

                    case "System.Int32":
                        result.Properties.Add(property.Key, new EntityProperty((int)property.Value));
                        break;

                    case "System.Int64":
                        result.Properties.Add(property.Key, new EntityProperty((long)property.Value));
                        break;

                    case "System.Guid":
                        result.Properties.Add(property.Key, new EntityProperty((Guid)property.Value));
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Unsupported property type '{0}'.".FormatString(property.Value.GetType()));
                }
            }

            return result;
        }

        protected virtual EntityProperty WriteBinaryEntityProperty(MemoryStream stream)
        {
            return new EntityProperty(stream.ToArray());
        }

        protected virtual MemoryStream ReadBinaryEntityProperty(EntityProperty property)
        {
            return new MemoryStream(property.BinaryValue, false);
        }
    }
}
