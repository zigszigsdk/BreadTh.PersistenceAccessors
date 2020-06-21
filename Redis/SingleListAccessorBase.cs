using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using StackExchange.Redis;

using BreadTh.PersistenceAccessors.Redis.Core;

namespace BreadTh.PersistenceAccessors.Redis
{
    public abstract class SingleListAccessorBase<T> : SingleAccessorBase<T[]>
    {
        protected SingleListAccessorBase(IConnectionMultiplexer connectionMultiplexer) : base(connectionMultiplexer) { }

        override public GetResult<T[]> Get()
        {
            string resultRaw = _connectionMultiplexer.GetDatabase().StringGet(GetAccessorSpecificKey());
            if (string.IsNullOrWhiteSpace(resultRaw))
                return new GetResult<T[]>(TryGetStatus.Empty, Array.Empty<T>());
            else
                try
                {
                    return new GetResult<T[]>(TryGetStatus.Ok, JsonConvert.DeserializeObject<T[]>("[" + resultRaw + "]"));
                }
                catch (JsonSerializationException)
                {
                    return new GetResult<T[]>(TryGetStatus.DataIsNotValidJson, Array.Empty<T>());
                }
        }

        override public async Task Set(T[] value) =>
            await
                _connectionMultiplexer.GetDatabase()
                    .StringSetAsync(GetAccessorSpecificKey(), JsonConvert.SerializeObject(value) + ",", GetDurability())
                    .ConfigureAwait(true);

        public async Task Append(T value)
        {
            await
                _connectionMultiplexer.GetDatabase()
                    .StringAppendAsync(GetAccessorSpecificKey(), JsonConvert.SerializeObject(value) + ",")
                    .ConfigureAwait(true);
        }
    }
}
