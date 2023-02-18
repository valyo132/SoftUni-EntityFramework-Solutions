using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MiniORM
{
    internal class ChangeTracker<T>
        where T : class, new()
    {
        private readonly List<T> _allEntities;
        private readonly List<T> _added;
        private readonly List<T> _deleted;

        public ChangeTracker(IEnumerable<T> entities)
        {
            _added = new List<T>();
            _deleted = new List<T>();
            _allEntities = CloneEntities(entities);
        }

        public IReadOnlyCollection<T> AllEntities => _allEntities.AsReadOnly();
        public IReadOnlyCollection<T> Added => _added.AsReadOnly();
        public IReadOnlyCollection<T> Removed => _deleted.AsReadOnly();

        private List<T>? CloneEntities(IEnumerable<T> entities)
        {
            var clonedEntities = new List<T>();

            var properiesToClone = typeof(T).GetProperties()
                .Where(p => DbContext.AllowedSqlTypes.Contains(p.PropertyType))
                .ToArray();

            foreach (var entity in entities)
            {
                var clonedEntity = Activator.CreateInstance<T>();
                foreach (var property in properiesToClone)
                {
                    var value = property.GetValue(entity);
                    property.SetValue(clonedEntity, value);
                }

                clonedEntities.Add(clonedEntity);
            }

            return clonedEntities;
        }

        public IEnumerable<T> GetModifiedEntities(DbSet<T> dbSet)
        {
            var modifiedEntities = new List<T>();

            var primaryKeys = typeof(T).GetProperties()
                .Where(p => p.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (var proxyEntity in AllEntities)
            {
                var primaryKeyValues = GetPrimaryKeyValues(primaryKeys, proxyEntity).ToArray();

                var entity = dbSet.Entities
                    .Single(e => GetPrimaryKeyValues(primaryKeys, e).SequenceEqual(primaryKeyValues));

                var isModified = IsModified(proxyEntity, entity);

                if (isModified)
                    modifiedEntities.Add(entity);
            }

            return modifiedEntities;
        }

        public void Add(T item)
            => _added.Add(item);

        public void Remove(T item)
            => _deleted.Remove(item);

        private static IEnumerable<object> GetPrimaryKeyValues(IEnumerable<PropertyInfo> primaryKeys, T entity)
            => primaryKeys.Select(pk => pk.GetValue(entity));

        private static bool IsModified(T entity, T proxyEntity)
        {
            var monitoredProperties = typeof(T).GetProperties()
                .Where(p => DbContext.AllowedSqlTypes.Contains(p.PropertyType));

            var modifiesProperties = monitoredProperties
                .Where(p => !Equals(p.GetValue(entity), p.GetValue(proxyEntity)))
                .ToArray();

            var isModified = modifiesProperties.Any();

            return isModified;
        }
    }
}