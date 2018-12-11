using System.Collections.Generic;
using Journalist.WindowsAzure.Storage.Tables.TableEntityConverters;
using Xunit;

namespace Journalist.WindowsAzure.Storage.UnitTests.Tables.TableEntityConverters
{
    public class TableEntityConverterTests
    {
        [Fact]
        public void CreateDynamicTableEntityFromProperties_WhenPropertyValueIsNull_DoesNotAddItToTheEntityProperties()
        {
            var c = new TableEntityConverter();

            var entity = c.CreateDynamicTableEntityFromProperties(new Dictionary<string, object>
            {
                { "MyProp", null }
            });

            Assert.False(entity.Properties.ContainsKey("MyProp"));
        }
    }
}
