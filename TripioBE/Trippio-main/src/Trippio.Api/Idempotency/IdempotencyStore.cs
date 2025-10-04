using StackExchange.Redis;

namespace Trippio.Api.Idempotency
{
    public interface IIdempotencyStore
    {
        Task<bool> TryUseAsync(string key, TimeSpan ttl);
    }

    public class RedisIdempotencyStore : IIdempotencyStore
    {
        private readonly IDatabase _db;
        public RedisIdempotencyStore(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

        public async Task<bool> TryUseAsync(string key, TimeSpan ttl)
            => await _db.StringSetAsync($"idem:{key}", "1", ttl, When.NotExists);
    }
}
