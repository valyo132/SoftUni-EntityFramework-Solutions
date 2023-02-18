using System.Collections;

namespace MiniORM
{
    public class DbSet<TEntity> : ICollection<TEntity>
        where TEntity : class, new()
    {
        public DbSet(IEnumerable<TEntity> entities)
        {
            Entities = entities.ToList();
            ChangeTracker = new ChangeTracker<TEntity>(entities);
        }

        public int Count => Entities.Count;

        public bool IsReadOnly => Entities.IsReadOnly;

        internal ChangeTracker<TEntity> ChangeTracker { get; set;}

        internal IList<TEntity> Entities { get; set;}

        public void Add(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("Item cannot be null");

            Entities.Add(entity);
            ChangeTracker.Add(entity);
        }

        public void Clear()
        {
            while (Entities.Any())
            {
                var entity = Entities.First();
                Remove(entity);
            }
        }

        public bool Contains(TEntity item)
            => Entities.Contains(item);

        public void CopyTo(TEntity[] array, int arrIndex)
            => Entities.CopyTo(array, arrIndex);

        public IEnumerator<TEntity> GetEnumerator()
            => Entities.GetEnumerator();

        public bool Remove(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException("Item cannot be null");

            var removeSuccessfully = Entities.Remove(item);

            if (removeSuccessfully)
                ChangeTracker.Remove(item);

            return removeSuccessfully;
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Remove(entity);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
