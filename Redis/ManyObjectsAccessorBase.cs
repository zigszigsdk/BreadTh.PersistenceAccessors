using System.Threading.Tasks;

using StackExchange.Redis;
using Newtonsoft.Json;

using BreadTh.PersistenceAccessors.Redis.Core;

namespace BreadTh.PersistenceAccessors.Redis
{
    public abstract class ManyObjectsAccessorBase<T> : ManyAccessorBase<T>
    {
        protected ManyObjectsAccessorBase(IConnectionMultiplexer connectionMultiplexer) : base(connectionMultiplexer) { }

        override public GetResult<T> Get(string key)
        {
            string resultRaw = _connectionMultiplexer.GetDatabase().StringGet(GetAccessorSpecificKey(key));
            
            if (string.IsNullOrWhiteSpace(resultRaw))
                return new GetResult<T>(TryGetStatus.Empty, default);
            else
                try
                {
                    return new GetResult<T>(TryGetStatus.Ok, JsonConvert.DeserializeObject<T>(resultRaw));
                }
                catch (JsonSerializationException)
                {
                    return new GetResult<T>(TryGetStatus.DataIsNotValidJson, default);
                }
        }

        override public async Task Set(string key, T value) =>
            await _connectionMultiplexer.GetDatabase().StringSetAsync(GetAccessorSpecificKey(key), JsonConvert.SerializeObject(value), GetDurability());
    }
    
}
