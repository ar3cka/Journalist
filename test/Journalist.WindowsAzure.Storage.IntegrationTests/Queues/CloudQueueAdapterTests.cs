using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.WindowsAzure.Storage.Queues;
using Ploeh.AutoFixture.Xunit2;
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

        [Theory]
        [InlineAutoData]
        public async Task AddedMessage_CanBeObtained(byte[] content)
        {
            // act
            await Queue.AddMessageAsync(content);
            var receivedMessage = await Queue.GetMessageAsync();

            // assert
            Assert.Equal(content, receivedMessage.Content);
        }

        [Theory]
        [InlineAutoData]
        public async Task AddedMessages_CanBeObtainedInBatch(List<byte[]> messages)
        {
            // arrange
            foreach (var content in messages)
            {
                await Queue.AddMessageAsync(content);
            }

            // act
            var receivedMessages = await Queue.GetMessagesAsync();

            // assert
            Assert.Equal(messages.Count(), receivedMessages.Count);
        }

        [Theory]
        [InlineAutoData]
        public async Task UpdateMessage_UpdatesContent(byte[] originalContent, byte[] updatedContent)
        {
            // arrange
            await Queue.AddMessageAsync(originalContent);
            var originalMessage = await Queue.GetMessageAsync();

            // act
            await Queue.UpdateMessageAsync(
                originalMessage.MessageId,
                originalMessage.PopReceipt,
                updatedContent);

            var updatedMessage = await Queue.GetMessageAsync();

            // assert
            Assert.Equal(updatedContent, updatedMessage.Content);
        }

        [Theory]
        [InlineAutoData]
        public async Task UpdateMessage_UpdatesVisibility()
        {
            // arrange
            await Queue.AddMessageAsync(EmptyArray.Get<byte>());
            var message = await Queue.GetMessageAsync();

            // act
            await Queue.UpdateMessageAsync(
                message.MessageId,
                message.PopReceipt,
                TimeSpan.FromSeconds(5));

            var updatedMessage = await Queue.GetMessageAsync();

            // assert
            Assert.Null(updatedMessage);
        }

        [InlineAutoData]
        public async Task UpdateMessage_UpdatesContentAndVisibility(byte[] originalContent, byte[] updatedContent)
        {
            // arrange
            await Queue.AddMessageAsync(originalContent);
            var originalMessage = await Queue.GetMessageAsync();

            // act
            await Queue.UpdateMessageAsync(
                originalMessage.MessageId,
                originalMessage.PopReceipt,
                updatedContent,
                TimeSpan.FromSeconds(5));

            var updatedMessage1 = await Queue.GetMessageAsync();
            await Task.Delay(TimeSpan.FromSeconds(10));
            var updatedMessage2 = await Queue.GetMessageAsync();

            // assert
            Assert.Null(updatedMessage1.Content);
            Assert.Equal(updatedContent, updatedMessage2.Content);
        }


        [Theory]
        [InlineAutoData]
        public async Task DeletedMessages_CanNotBeObtained(byte[] content)
        {
            // arrange
            await Queue.AddMessageAsync(content);
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
