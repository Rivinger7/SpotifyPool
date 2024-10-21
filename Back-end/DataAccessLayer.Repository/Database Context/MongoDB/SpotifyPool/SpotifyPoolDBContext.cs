using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using SetupLayer.Setting.Database;

namespace DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool
{
    public class SpotifyPoolDBContext(MongoDBSetting mongoDBSettings, IMongoClient mongoClient, ILogger<SpotifyPoolDBContext> logger)
    {
        private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);
        private readonly ILogger<SpotifyPoolDBContext> _logger = logger;

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
