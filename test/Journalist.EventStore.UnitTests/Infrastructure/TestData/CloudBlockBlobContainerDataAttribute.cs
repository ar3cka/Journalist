using System;
using System.Threading.Tasks;
using Journalist.Tasks;
using Journalist.WindowsAzure.Storage.Blobs;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class CloudBlockBlobContainerDataAttribute : AutoMoqDataAttribute
    {
        public CloudBlockBlobContainerDataAttribute(bool isExists = true, bool leaseLocked = false)
        {
            Fixture.Register(() => TimeSpan.FromMinutes(2));

            Fixture.Customize<Mock<ICloudBlockBlob>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.IsExistsAsync())
                    .Returns(isExists ? TaskDone.True : TaskDone.False))
                .Do(mock => mock
                    .Setup(self => self.IsLeaseLocked())
                    .Returns(leaseLocked ? TaskDone.True : TaskDone.False))
                .Do(mock => mock
                    .Setup(self => self.AcquireLeaseAsync(It.IsAny<TimeSpan?>(), null))
                    .Returns(leaseLocked ? Task.FromResult<string>(null) : Task.FromResult(Fixture.Create("LeaseId"))))
                .Do(mock => mock
                    .Setup(self => self.Metadata)
                    .ReturnsUsingFixture(Fixture))
                .Do(mock => mock
                    .Setup(self => self.FetchAttributesAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.SaveMetadataAsync(It.IsAny<string>()))
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.BreakLeaseAsync(It.IsAny<TimeSpan?>()))
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.ReleaseLeaseAsync(It.IsAny<string>()))
                    .Returns(TaskDone.True)));

            Fixture.Customize<Mock<ICloudBlobContainer>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.CreateBlockBlob(It.IsAny<string>()))
                    .ReturnsUsingFixture(Fixture)));
        }
    }
}
