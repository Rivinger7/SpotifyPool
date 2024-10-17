using MongoDB.Driver;

namespace DataAccessLayer.Interface.MongoDB.Generic_Repository
{
    public interface IGenericRepository<TDocument> where TDocument : class
    {
        IMongoCollection<TDocument> Collection { get; }
        Task<IEnumerable<TDocument>> GetAllAsync();
        Task<TDocument> GetByIdAsync(string id);
        Task AddAsync(TDocument entity);
        Task UpdateAsync(string id, TDocument entity);
        Task DeleteAsync(string id);
    }
}
