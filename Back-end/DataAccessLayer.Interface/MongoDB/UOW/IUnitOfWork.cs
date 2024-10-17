using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using MongoDB.Driver;

namespace DataAccessLayer.Interface.MongoDB.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TDocument> GetRepository<TDocument>() where TDocument : class;
        IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : class;
    }
}
