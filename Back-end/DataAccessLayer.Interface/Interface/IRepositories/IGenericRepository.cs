using MongoDB.Driver.Linq;

namespace DataAccessLayer.Interface.Interface.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        IMongoQueryable<T> Entities { get; }

        Task DeleteAsync(object id);

        IEnumerable<T> GetAll();

        Task<IList<T>> GetAllAsync();

        T? GetById(object id);

        Task<T?> GetByIdAsync(object id);

        Task InsertAsync(T obj);

        Task InsertRangeAsync(IList<T> obj);

        Task UpdateAsync(object id, T obj);
    }
}
