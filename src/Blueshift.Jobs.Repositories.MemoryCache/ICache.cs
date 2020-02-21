using System.Linq;

namespace Blueshift.Jobs.Repositories.MemoryCache
{
    public interface ICache<TKey, TValue>
    {
        bool TryAddValue(TKey key, TValue value);

        bool TryGetValue(TKey key, out TValue value);

        bool TryRemoveValue(TKey key);
        
        TValue SetValue(TKey key, TValue value);

        IQueryable<TValue> Query();

        bool HasKey(TKey key);
    }
}
