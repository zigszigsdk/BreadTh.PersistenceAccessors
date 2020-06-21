using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace BreadTh.PersistenceAccessors.Postgres
{
    public delegate T Factory<T>();

    public enum TryGetOneStatus { Undefined, Ok, DoesNotExist, MultipleExist }
    public enum TryGetMultipleStatus { Undefined, Ok, DoesNotExist }

    public abstract class PostgresAccessorBase<DBCONTEXT, ENTRY>
        where DBCONTEXT : PostgresDbContextBase
        where ENTRY : class
    {
        readonly Factory<DBCONTEXT> _dbContextFactory;

        protected PostgresAccessorBase(Factory<DBCONTEXT> dbContextFactory) =>
            _dbContextFactory = dbContextFactory;

        abstract protected DbSet<ENTRY> GetGenericsDbSet(DBCONTEXT db);

        public async Task AddOne(ENTRY newEntry)
        {
            using DBCONTEXT db = _dbContextFactory();
            await GetGenericsDbSet(db).AddAsync(newEntry);
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        public GetOneResult<ENTRY> GetOneBySelector(Func<ENTRY, bool> selector)
        {
            List<ENTRY> results = GetGenericsDbSet(_dbContextFactory()).Where(selector).ToList();

            return results.Count switch
            {   0 => new GetOneResult<ENTRY>(TryGetOneStatus.DoesNotExist, null)
            ,   1 => new GetOneResult<ENTRY>(TryGetOneStatus.Ok, results.First())
            ,   _ => new GetOneResult<ENTRY>(TryGetOneStatus.MultipleExist, null)
            };
        }

        public List<ENTRY> GetManyBySelector(Func<ENTRY, bool> selector) =>
            GetGenericsDbSet(_dbContextFactory()).Where(selector).ToList();
        

        public async Task DeleteBySelector(Func<ENTRY, bool> selector)
        {
            using DBCONTEXT dbContext = _dbContextFactory();
            DbSet<ENTRY> collection = GetGenericsDbSet(dbContext);
            collection.RemoveRange(collection.Where(selector));
            await dbContext.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task ModifyBySelector(Func<ENTRY, bool> selector, Action<ENTRY> modifier)
        {
            using DBCONTEXT dbContext = _dbContextFactory();
            GetGenericsDbSet(dbContext).Where(selector).ToList().ForEach(x => modifier(x));
            await dbContext.SaveChangesAsync().ConfigureAwait(true);
        }
    }
}
