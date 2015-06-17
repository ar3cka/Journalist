using System;
using System.Threading;

namespace Journalist.WindowsAzure.Storage.Internals
{
    public abstract class CloudEntityAdapter<TEntity>
    {
        private readonly Lazy<TEntity> m_entity;

        protected CloudEntityAdapter(Func<TEntity> entityFactory)
        {
            Require.NotNull(entityFactory, "entityFactory");

            m_entity = new Lazy<TEntity>(entityFactory, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected TEntity CloudEntity
        {
            get { return m_entity.Value; }
        }
    }
}
