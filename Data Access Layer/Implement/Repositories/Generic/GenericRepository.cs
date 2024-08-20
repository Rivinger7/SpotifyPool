using DataAccessLayer.Interface.Interface.IRepositories;
using MongoDB.Driver;

namespace Data_Access_Layer.Implement.Repositories.Generic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;

        public GenericRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public IQueryable<T> Entities => _collection.AsQueryable();

        public async Task DeleteAsync(object id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.DeleteOneAsync(filter);
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.Find(_ => true).ToEnumerable();
        }

        public async Task<IList<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public T? GetById(object id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return _collection.Find(filter).FirstOrDefault();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(T obj)
        {
            await _collection.InsertOneAsync(obj);
        }

        public async Task InsertRangeAsync(IList<T> obj)
        {
            await _collection.InsertManyAsync(obj);
        }

        public async Task UpdateAsync(object id, T obj)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.ReplaceOneAsync(filter, obj);
        }
    }
}
