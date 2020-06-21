using System;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace BreadTh.PersistenceAccessors.Redis.Core
{
    abstract public class ManyAccessorBase<T>
    {
        protected readonly IConnectionMultiplexer _connectionMultiplexer;

        protected ManyAccessorBase(IConnectionMultiplexer connectionMultiplexer) =>
            _connectionMultiplexer = connectionMultiplexer;

        abstract public GetResult<T> Get(string key);

        abstract public Task Set(string key, T value);

        public async Task Delete(string key) =>
            await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(GetAccessorSpecificKey(key));

        public GetResult<T> GetAndDelete(string key)
        {
            GetResult<T> result = Get(key);

            if (result.status == TryGetStatus.Ok)
                _connectionMultiplexer.GetDatabase().KeyDelete(GetAccessorSpecificKey(key));

            return result;
        }

        abstract public TimeSpan? GetDurability();
        abstract protected string GetAccessorSpecificKey(string key);
    }
}
