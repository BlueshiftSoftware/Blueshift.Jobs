using System;
using Xunit;

namespace Blueshift.Jobs.Repositories.MemoryCache.Tests
{
    public class ConcurrentCacheTests
    {
        private ConcurrentCache<string, ConcurrentCacheTestItem> _concurrentCache;

        public ConcurrentCacheTests()
        {
            _concurrentCache = new ConcurrentCache<string, ConcurrentCacheTestItem>();
        }

        [Fact]
        public void TryAddValue_returns_true_when_item_not_yet_present()
        {
            var concurrentCacheTestItem = new ConcurrentCacheTestItem();

            Assert.True(_concurrentCache.TryAddValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
        }

        [Fact]
        public void TryAddValue_returns_false_when_item_not_already_present()
        {
            var concurrentCacheTestItem = new ConcurrentCacheTestItem();

            Assert.True(_concurrentCache.TryAddValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
            Assert.False(_concurrentCache.TryAddValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
        }

        [Fact]
        public void TryGetValue_returns_true_when_item_present()
        {
            var concurrentCacheTestItem = new ConcurrentCacheTestItem();

            Assert.True(_concurrentCache.TryAddValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
            Assert.True(_concurrentCache.TryGetValue(concurrentCacheTestItem.StringProperty, out ConcurrentCacheTestItem fetchedConcurrentCacheTestItem));
            Assert.Same(concurrentCacheTestItem, fetchedConcurrentCacheTestItem);
        }

        [Fact]
        public void TryGetValue_returns_false_when_item_not_present()
        {
            string missingKey = Guid.NewGuid().ToString("N");
            Assert.False(_concurrentCache.TryGetValue(missingKey, out ConcurrentCacheTestItem fetchedConcurrentCacheTestItem));
        }

        [Fact]
        public void TryRemoveValue_returns_true_and_removes_item_when_item_present()
        {
            var concurrentCacheTestItem = new ConcurrentCacheTestItem();

            Assert.True(_concurrentCache.TryAddValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
            Assert.True(_concurrentCache.TryGetValue(concurrentCacheTestItem.StringProperty, out ConcurrentCacheTestItem fetchedConcurrentCacheTestItem));
            Assert.Same(concurrentCacheTestItem, fetchedConcurrentCacheTestItem);
            Assert.True(_concurrentCache.TryRemoveValue(concurrentCacheTestItem.StringProperty));
            Assert.False(_concurrentCache.TryGetValue(concurrentCacheTestItem.StringProperty, out fetchedConcurrentCacheTestItem));
        }

        [Fact]
        public void TryRemoveValue_returns_false_when_item_not_present()
        {
            string missingKey = Guid.NewGuid().ToString("N");
            Assert.False(_concurrentCache.TryRemoveValue(missingKey));
        }

        [Fact]
        public void SetValue_adds_and_echoes_value_when_missing()
        {
            var concurrentCacheTestItem = new ConcurrentCacheTestItem();

            Assert.False(_concurrentCache.TryGetValue(concurrentCacheTestItem.StringProperty, out ConcurrentCacheTestItem _));
            Assert.Same(concurrentCacheTestItem, _concurrentCache.SetValue(concurrentCacheTestItem.StringProperty, concurrentCacheTestItem));
            Assert.True(_concurrentCache.TryGetValue(concurrentCacheTestItem.StringProperty, out ConcurrentCacheTestItem fetchedConcurrentCacheTestItem));
            Assert.Same(concurrentCacheTestItem, fetchedConcurrentCacheTestItem);
        }

        [Fact]
        public void SetValue_replaces_and_echoes_value_when_key_already_present()
        {
            var concurrentCacheTestItem1 = new ConcurrentCacheTestItem();
            var concurrentCacheTestItem2 = new ConcurrentCacheTestItem();

            Assert.False(_concurrentCache.TryGetValue(concurrentCacheTestItem1.StringProperty, out ConcurrentCacheTestItem _));
            Assert.False(_concurrentCache.TryGetValue(concurrentCacheTestItem2.StringProperty, out _));
            Assert.Same(concurrentCacheTestItem1, _concurrentCache.SetValue(concurrentCacheTestItem1.StringProperty, concurrentCacheTestItem1));
            Assert.Same(concurrentCacheTestItem2, _concurrentCache.SetValue(concurrentCacheTestItem1.StringProperty, concurrentCacheTestItem2));
            Assert.True(_concurrentCache.TryGetValue(concurrentCacheTestItem1.StringProperty, out ConcurrentCacheTestItem fetchedConcurrentCacheTestItem));
            Assert.NotSame(concurrentCacheTestItem1, fetchedConcurrentCacheTestItem);
            Assert.Same(concurrentCacheTestItem2, fetchedConcurrentCacheTestItem);
            Assert.False(_concurrentCache.TryGetValue(concurrentCacheTestItem2.StringProperty, out fetchedConcurrentCacheTestItem));
        }

        [Fact]
        public void HasKey_returns_true_when_present_in_cache()
        {
            var concurrentCacheTestItem1 = new ConcurrentCacheTestItem();

            Assert.True(_concurrentCache.TryAddValue(concurrentCacheTestItem1.StringProperty, concurrentCacheTestItem1));
            Assert.True(_concurrentCache.HasKey(concurrentCacheTestItem1.StringProperty));
        }

        [Fact]
        public void HasKey_returns_false_when_not_present_in_cache()
        {
            string missingKey = Guid.NewGuid().ToString("N");

            Assert.False(_concurrentCache.HasKey(missingKey));
        }

        public class ConcurrentCacheTestItem
        {
            public string StringProperty { get; } = Guid.NewGuid().ToString("D");
        }
    }
}
