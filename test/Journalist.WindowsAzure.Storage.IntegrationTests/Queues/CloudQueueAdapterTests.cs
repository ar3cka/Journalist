using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Queues;
using Xunit;

namespace Journalist.WindowsAzure.Storage.IntegrationTests.Queues
{
    public class CloudQueueAdapterTests
    {
        public CloudQueueAdapterTests()
        {
            Factory = new StorageFactory();
            Queue = Factory.CreateQueue("UseDevelopmentStorage=true", "cloud-queue-tests-" + Guid.NewGuid().ToString("N"));
        }

        [Fact]
        public async Task AddedMessage_CanBeObtained()
        {
            // arrange
            var sendedContent = Encoding.UTF8.GetBytes("Hello!");

            // act
            await Queue.AddMessageAsync(sendedContent);
            var receivedMessage = await Queue.GetMessageAsync();

            // assert
            Assert.Equal(sendedContent, receivedMessage.Content);
        }

        [Fact]
        public async Task AddedMessages_CanBeObtainedInBatch()
        {
            // arrange
            foreach (var number in Enumerable.Range(1, 10))
            {
                var sendedContent = Encoding.UTF8.GetBytes("Message #" + number);
                await Queue.AddMessageAsync(sendedContent);
            }

            // act
            var receivedMessages = await Queue.GetMessagesAsync();

            // assert
            Assert.Equal(10, receivedMessages.Count);
        }

        [Fact]
        public async Task DeletedMessages_CanNotBeObtained()
        {
            // arrange
            await Queue.AddMessageAsync(Encoding.UTF8.GetBytes("Hello!"));
            var message = await Queue.GetMessageAsync();

            // act
            await Queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            var receivedMessages = await Queue.GetMessageAsync();

            // assert
            Assert.Null(receivedMessages);
        }

        public StorageFactory Factory { get; set; }

        public ICloudQueue Queue { get; set; }
    }
}
