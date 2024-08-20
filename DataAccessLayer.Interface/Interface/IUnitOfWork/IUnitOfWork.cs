using DataAccessLayer.Interface.Interface.IRepositories;

namespace DataAccessLayer.Interface.Interface.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>(string collectionName) where T : class;
        void Save();
        Task SaveAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
