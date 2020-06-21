using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using StackExchange.Redis;

using BreadTh.PersistenceAccessors.Redis.Core;

namespace BreadTh.PersistenceAccessors.Redis
{
    public abstract class ManyListsAccessorBase<T> : ManyAccessorBase<T[]>
    {
        protected ManyListsAccessorBase(IConnectionMultiplexer connectionMultiplexer) : base(connectionMultiplexer) { }

        override public GetResult<T[]> Get(string key)
        {
            string resultRaw = _connectionMultiplexer.GetDatabase().StringGet(GetAccessorSpecificKey(key));
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

        override public async Task Set(string key, T[] value) =>
            await
                _connectionMultiplexer.GetDatabase()
                    .StringSetAsync(GetAccessorSpecificKey(key), JsonConvert.SerializeObject(value) + ",", GetDurability())
                    .ConfigureAwait(true);
        
        public async Task Append(string key, T value)
        {
            await 
                _connectionMultiplexer.GetDatabase()
                    .StringAppendAsync(GetAccessorSpecificKey(key), JsonConvert.SerializeObject(value) + ",")
                    .ConfigureAwait(true);
        }
    }
}
