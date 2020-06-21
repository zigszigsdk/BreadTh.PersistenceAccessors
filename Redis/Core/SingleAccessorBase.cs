using System;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace BreadTh.PersistenceAccessors.Redis.Core
{
    abstract public class SingleAccessorBase<T>
    {
        protected readonly IConnectionMultiplexer _connectionMultiplexer;

        protected SingleAccessorBase(IConnectionMultiplexer connectionMultiplexer) =>
            _connectionMultiplexer = connectionMultiplexer;

        abstract public GetResult<T> Get();

        abstract public Task Set(T value);

        public async Task Delete() =>
            await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(GetAccessorSpecificKey());

        public GetResult<T> GetAndDelete()
        {
            GetResult<T> result = Get();

            if (result.status == TryGetStatus.Ok)
                _connectionMultiplexer.GetDatabase().KeyDelete(GetAccessorSpecificKey());

            return result;
        }

        abstract public TimeSpan? GetDurability();
        abstract protected string GetAccessorSpecificKey();
    }
}
