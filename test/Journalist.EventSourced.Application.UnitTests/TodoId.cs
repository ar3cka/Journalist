using System;
using Journalist.EventSourced.Entities;

namespace Journalist.EventSourced.Application.UnitTests
{
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
}