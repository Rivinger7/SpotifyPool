using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DataAccessLayer.Implement.MongoDB.Generic_Repository
{
    public class GenericRepository<TDocument>(IMongoDatabase database) : IGenericRepository<TDocument> where TDocument : class
    {
        private readonly IMongoDatabase _database = database;

        public IMongoCollection<TDocument> Collection => _database.GetCollection<TDocument>(typeof(TDocument).Name);

        public async Task<IEnumerable<TResult>> GetAllDocumentsWithLookupAsync<TForeignDocument, TResult>(
    Expression<Func<TDocument, IEnumerable<object>>> localField,    // Field in TDocument to join on
    Expression<Func<TForeignDocument, object>> foreignField,        // Field in TForeignDocument to match on
    Expression<Func<TResult, IEnumerable<TForeignDocument>>> resultField) // Field in TResult to store joined documents
        {
            // Start aggregating the collection of TDocument
            IAggregateFluent<TDocument> documentCollection = Collection.Aggregate();

            // Perform the lookup operation to join TDocument with TForeignDocument
            IAggregateFluent<TResult> pipeline = documentCollection.Lookup<TForeignDocument, TResult>(
                typeof(TForeignDocument).Name, // Foreign collection name (e.g., "artists" for Artist collection)
                new ExpressionFieldDefinition<TDocument>(localField),    // Local field from TDocument (e.g., ArtistIds in Track)
                new ExpressionFieldDefinition<TForeignDocument>(foreignField),  // Foreign field from TForeignDocument (e.g., SpotifyId in Artist)
                new ExpressionFieldDefinition<TResult>(resultField)         // Result field in TResult (e.g., Artists in ASTrack)
            ).As<TResult>();

            // Execute the pipeline and get the result as a list of TResult (e.g., ASTrack)
            IEnumerable<TResult> results = await pipeline.ToListAsync();

            return results; // Return the results (which would be of type ASTrack in your case)
        }



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
