using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool
{
    public class SpotifyPoolDBContext(MongoDBSetting mongoDBSettings, IMongoClient mongoClient, ILogger<SpotifyPoolDBContext> logger)
    {
        private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);
        private readonly ILogger<SpotifyPoolDBContext> _logger = logger;

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Playlist> Playlists => _database.GetCollection<Playlist>("Playlist");
        public IMongoCollection<Track> Tracks => _database.GetCollection<Track>("Track");
        public IMongoCollection<Artist> Artists => _database.GetCollection<Artist>("Artist");
        public IMongoCollection<Payment> Albums => _database.GetCollection<Payment>("Payment");
    }
}
