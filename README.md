# BreadTh.PersistenceAccessors
Are you tired of setting up the same tried database-accessor persistence logic that does the same thing in all your ASP.NET Core 3.1 projects?
Or do you not *know* how to actually separate persistence logic from business logic?

This project kickstarts new projects' access to the underlying databases by templating and streamlining the access implementation in a flexible manner that keeps the persistence logic out of the business logic.

Most projects try to be as generic and open as possible. Because this project contains many opinions about how one should be accessing their databases from business logic, a fuller setup can be provided.

## What do you get?
- Postgres, a great run-of-the-mill "SQL" database. Ideal for permanent data like user data.
- Redis, an ultra fast key-value store with automatic cleanup. Ideal for temporary data, like a bearer token.

## Get started
### Init
Install Redis and Postgresql on your development machine.
Include the latest build of BreadTh.PersistenceAccessors from NuGet in your project.

### Redis
Extend one of the 3 base classes in `BreadTh.PersistenceAccessors.Redis` with your model as the generic implementation and implement the abstract methods.

**Example**
Implementation:
```csharp
//model
public struct BearerToken
{
    public string applicationId;
}

//accessor
public sealed class BearerTokenAccessor : ManyObjectsAccessorBase<BearerToken>
{
    public BearerTokenAccessor(IConnectionMultiplexer connectionMultiplexer) : base(connectionMultiplexer) { }

    protected override string GetAccessorSpecificKey(string key) =>
        $"bearerToken::{key}";

    public override TimeSpan? GetDurability() =>
        TimeSpan.FromHours(2); // null to last forever
}
```
Using:
```csharp
public enum TokenLookupStatus { Undefined, Ok, TokenDoesNotExist }
public readonly struct TokenLookup 
{
    public readonly TokenLookupStatus status;
    public readonly string applicationId;
}

public sealed class BearerTokenHandler
{
    private readonly BearerTokenAccessor _bearerTokenAccessor;
    public BearerTokenHandler(BearerTokenAccessor bearerTokenAccessor) 
    {
        _bearerTokenAccessor = bearerTokenAccessor;
    }

    public async Task<string> CreateNew(string applicationId)
    {
        string tokenKey = Guid.New(); //don't actually do this. Not cryptographically secure.
        await _bearerTokenAccessor.Set(tokenKey, new BearerToken(){ applicationId = applicationId } );
        return tokenKey;
    }

    public TokenLookup GetToken(string token)
    {
        GetResult<BearerToken> lookup = _bearerTokenAccessor.Get(token);
        if (lookup.status != TryGetStatus.Ok)
            return new TokenLookup(){ applicationId = "", status = TokenLookupStatus.TokenDoesNotExist };

        return new TokenLookup(){ applicationId = lookup.result.applicationId, status = TokenLookupStatus.Ok };
    }
}
```

**Model:**
Your model can be any datatype, primitive or otherwise. Fields must be public such that data can be filled in and classes must have a zero-parameter constructor. (automatic when you don't declare any constructors at all)
**Base classes:**
* **ManyObjectsAccessorBase**: The standard Redis experience. You can `.Set(key, value)`, `.Get(key)` and `.Delete(key)`  (plus `.GetAndDelete(key)`)
* **ManyListsAccessorBase**: As above, but the model object will be wrapped in a list, and, such that you can also `.Append(key, value)`.
* **SingleListAccessorBase**: A bit like the above, but there's only a single list rather than many lists, so you don't have to deal with keys. You simply `.Get()`, `.Set(values)`, `.Append(value)` and `.Delete()`

##### Methods
You need to implement the following methods:
* **`protected override string GetAccessorSpecificKey(string key)`**: Here you simply need to provide a unique string for the object based on the key. It's common Redis-practice to prepend a key with the name of the specific use and two colons, as seen in the example. if you extend `SingleListAccessorBase` there's no `key` parameter.
* **`public override TimeSpan? GetDurability()`**: How soon should the value disappear after being set? Return `null` to keep the value forever.

### Postgresql
TODO

### Aspnet
You must register the the underlying redis and postgres service in Startup.cs with the provided helpers:
```csharp
public class Startup
{
	override protected void ConfigureServices(IServiceCollection serviceCollection)
	{
	    serviceCollection.AddBreadThRedisService();
	    serviceCollection.AddBreadThPostgresService(options => new PostgresDbContext(options, _configuration));
	}

	override protected void Configure(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
	{
	    serviceProvider.ConfigureBreadThPostgresService<PostgresDbContext>();
	}
}
```

## Honorably mentions
This package wraps and builds on top of:
* https://github.com/StackExchange/StackExchange.Redis  
* https://github.com/npgsql/efcore.pg
* https://github.com/JamesNK/Newtonsoft.Json

(you don't need to install them yourself)

## Contributions are welcome
Not sure how to help? Here are a few ideas:
* Get rid of `IConnectionMultiplexer` from the redis-implementing constructors.
* Transform the `AddBreadThPostgresService` helper to a generic method like `ConfigureBreadThPostgresService`.
* Optimize performance of queries in `PostgresAccessorBase`½
* Add `SingleObjectAccessorBase` to the Redis accessors.

Note that all contributons will fall under The Unlicense, just like the rest of this repository.