using MongoDB.Driver;
using SetupLayer.Setting.Database;

namespace DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool
{
    public class SpotifyPoolDBContext(MongoDBSetting mongoDBSettings, IMongoClient mongoClient)
    {
        private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
