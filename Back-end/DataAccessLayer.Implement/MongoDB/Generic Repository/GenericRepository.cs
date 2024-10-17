using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using MongoDB.Driver;

namespace DataAccessLayer.Implement.MongoDB.Generic_Repository
{
    public class GenericRepository<TDocument>(IMongoDatabase database) : IGenericRepository<TDocument> where TDocument : class
    {
        public IMongoCollection<TDocument> Collection => database.GetCollection<TDocument>(typeof(TDocument).Name);

        public async Task<IEnumerable<TDocument>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync();
        }

        public async Task<TDocument> GetByIdAsync(string id)
        {
            return await Collection.Find(Builders<TDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task AddAsync(TDocument entity)
        {
            await Collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, TDocument entity)
        {
            await Collection.ReplaceOneAsync(Builders<TDocument>.Filter.Eq("_id", id), entity);
        }

        public async Task DeleteAsync(string id)
        {
            await Collection.DeleteOneAsync(Builders<TDocument>.Filter.Eq("_id", id));
        }
    }
}
