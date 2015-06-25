using System;
using System.Threading.Tasks;
using Journalist.EventSourced.Entities;
using Journalist.Options;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventSourced.Application.UnitTests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture()
                .Customize(new AutoConfiguredMoqCustomization()))
        {

        }
    }

    public class TodoState : AbstractAggregateState
    {
    }

    public sealed class TodoId : AbstractIdentity<Guid>
    {
        public TodoId(Guid id)
        {
            Require.NotEmpty(id, "id");

            Id = id;
        }

        public static TodoId Create()
        {
            return new TodoId(Guid.NewGuid());
        }

        public override string GetTag()
        {
            return "todo";
        }

        public override Guid Id
        {
            get; protected set;
        }
    }

    public class AggregationRootRepositoryTests
    {
        [Theory]
        [AutoMoqData]
        public async Task LoadAsync_WhenAggregateStateNotFound_ReturnsEmptyAggregate(
            TodoRepository repository)
        {
            var todo = await repository.TryLoadAsync(TodoId.Create());

            Assert.Equal(0.MayBe(), todo.Select(t => t.State.OriginalStateVersion));
        }
    }

    public interface IAggregateStateStorage<TState>
        where TState : IAggregateState
    {
        Task<Option<TState>> RestoreStateAsync(IIdentity identity);
    }

    public class TodoRepository
    {
        private readonly IAggregateStateStorage<TodoState> m_stateStorage;

        public TodoRepository(IAggregateStateStorage<TodoState> stateStorage)
        {
            Require.NotNull(stateStorage, "stateStorage");

            m_stateStorage = stateStorage;
        }

        public async Task<Option<Todo>> TryLoadAsync(TodoId todoId)
        {
            var state = await m_stateStorage.RestoreStateAsync(todoId);

            return state.Select(s => new Todo(s));
        }
    }
}
