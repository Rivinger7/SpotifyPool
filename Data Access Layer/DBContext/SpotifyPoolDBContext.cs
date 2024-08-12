using MongoDB.Driver;
using Data_Access_Layer.Repositories;
using Microsoft.Extensions.Logging;

namespace Data_Access_Layer.DBContext
{
    public class SpotifyPoolDBContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<SpotifyPoolDBContext> _logger;

        public SpotifyPoolDBContext(MongoDBSetting mongoDBSettings, ILogger<SpotifyPoolDBContext> logger)
        {
            var mongoClient = new MongoClient(mongoDBSettings.ConnectionString);
            _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);
            _logger = logger;
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
        public IMongoCollection<Playlist> Playlists => _database.GetCollection<Playlist>("Playlist");
        public IMongoCollection<Track> Tracks => _database.GetCollection<Track>("Track");
        public IMongoCollection<Artist> Artists => _database.GetCollection<Artist>("Artist");
        public IMongoCollection<Payment> Albums => _database.GetCollection<Payment>("Payment");
    }
}
