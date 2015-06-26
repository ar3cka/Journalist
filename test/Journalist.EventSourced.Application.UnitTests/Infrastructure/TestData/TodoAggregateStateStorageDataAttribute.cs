using System;
using System.Threading.Tasks;
using Journalist.EventSourced.Application.Repositories;
using Journalist.EventSourced.Entities;
using Journalist.Options;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventSourced.Application.UnitTests.Infrastructure.TestData
{
    public class TodoAggregateStateStorageDataAttribute : AutoMoqDataAttribute
    {
        public TodoAggregateStateStorageDataAttribute(bool emptyState = false)
        {
            Fixture.Customize(new AutoConfiguredMoqCustomization());

            Func<Task<Option<TodoState>>> state = () =>
            {
                var s = emptyState
                    ? Option.None()
                    : Fixture.Create<TodoState>().MayBe();

                return s.YieldTask();
            };

            Fixture.Customize<Mock<IAggregateStateStorage<TodoState>>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.RestoreStateAsync(It.IsAny<IIdentity>()))
                    .Returns(state))
                .Do(mock => mock
                    .Setup(self => self.PersistAsync(
                        It.IsAny<IIdentity>(),
                        It.IsAny<TodoState>()))
                    .Returns(TaskDone.Done)));
        }
    }
}
