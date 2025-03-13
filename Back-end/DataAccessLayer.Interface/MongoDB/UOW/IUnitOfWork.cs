﻿using MongoDB.Driver;

namespace DataAccessLayer.Interface.MongoDB.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : class;
    }
}
