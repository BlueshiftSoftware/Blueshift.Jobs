using System.Collections.Concurrent;
using System.Linq;

namespace Blueshift.Jobs.Repositories.MemoryCache
{
    public class ConcurrentCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary = new ConcurrentDictionary<TKey, TValue>();

        public TValue SetValue(TKey key, TValue value)
            => _dictionary.AddOrUpdate(
                    key,
                    value,
                    (TKey existingKey, TValue existingValue) => value);

        public bool TryAddValue(TKey key, TValue value)
            => _dictionary.TryAdd(key, value);

        public bool TryGetValue(TKey key, out TValue value)
            => _dictionary.TryGetValue(key, out value);

        public bool TryRemoveValue(TKey key)
            => _dictionary.TryRemove(key, out TValue value);

        public IQueryable<TValue> Query()
            => _dictionary.Values.AsQueryable();

        public bool HasKey(TKey key)
            => _dictionary.ContainsKey(key);
    }
}
