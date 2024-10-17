using DataAccessLayer.Implement.MongoDB.Generic_Repository;
using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using MongoDB.Driver;

namespace DataAccessLayer.Implement.MongoDB.UOW
{
    public class UnitOfWork(SpotifyPoolDBContext dbContext) : IUnitOfWork
    {
        private readonly IMongoDatabase _database = dbContext.GetDatabase();
        private readonly Dictionary<Type, object> _repositories = [];
        private bool disposedValue;
        public IGenericRepository<TDocument> GetRepository<TDocument>() where TDocument : class
        {
            if (_repositories.ContainsKey(typeof(TDocument)))
            {
                return (IGenericRepository<TDocument>)_repositories[typeof(TDocument)];
            }

            GenericRepository<TDocument> repository = new(_database);
            _repositories.Add(typeof(TDocument), repository);
            return repository;

            //return new GenericRepository<TDocument>(_database);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : class
        {
            return _database.GetCollection<TDocument>(typeof(TDocument).Name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SpotifyPoolDBContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
