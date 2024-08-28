using Data_Access_Layer.DBContext;
using Data_Access_Layer.Implement.Repositories.Generic;
using DataAccessLayer.Interface.Interface.IRepositories;
using DataAccessLayer.Interface.Interface.IUnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Data_Access_Layer.Implement.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private IClientSessionHandle? _session;

        public UnitOfWork(IOptions<MongoDBSetting> mongoDBSettings)
        {
            _mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            _database = _mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _session = null; // For sure
        }

        public void BeginTransaction()
        {
            _session = _mongoClient.StartSession();
            _session.StartTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.CommitTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.AbortTransactionAsync();
            }
        }

        public IGenericRepository<T> GetRepository<T>(string collectionName) where T : class
        {
            return new GenericRepository<T>(_database, collectionName);
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        // `Save` và `SaveAsync` không có tác dụng với MongoDB do không có trạng thái (state) cần lưu.
        // Nếu bạn muốn quản lý các thay đổi với các giao dịch thì bạn đã cung cấp BeginTransaction, CommitTransactionAsync, và RollbackTransactionAsync.
        public void Save() => throw new NotImplementedException("Save is not applicable for MongoDB.");
        public Task SaveAsync() => throw new NotImplementedException("SaveAsync is not applicable for MongoDB.");
    }
}
