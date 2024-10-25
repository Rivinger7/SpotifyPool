using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using Utility.Coding;

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

        Task<BasePaginatedList<BsonDocument>> Paging(IMongoCollection<TDocument> collection, ProjectionDefinition<TDocument> projection, int currentPageIndex, int itemsPerPage);


		Task<IEnumerable<TResult>> GetAllDocumentsWithLookupAsync<TForeignDocument, TResult>(
            Expression<Func<TDocument, IEnumerable<object>>> localField,
            Expression<Func<TForeignDocument, object>> foreignField,
            Expression<Func<TResult, IEnumerable<TForeignDocument>>> resultField);
    }
}
