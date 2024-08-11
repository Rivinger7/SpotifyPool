using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SpotifyPool.Models;

namespace SpotifyPool.Data
{
	public class SpotifyPoolDBContext
	{
		private readonly IMongoDatabase _database;

		public SpotifyPoolDBContext() { }

		public SpotifyPoolDBContext(IOptions<SpotifyPoolDatabaseSettings> mongoDBSettings)
		{
			var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
			_database = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
		}

		public IMongoCollection<User> Users => _database.GetCollection<User>("User");
		public IMongoCollection<Playlist> Playlists => _database.GetCollection<Playlist>("Playlist");
		public IMongoCollection<Track> Tracks => _database.GetCollection<Track>("Track");
		public IMongoCollection<Artist> Artists => _database.GetCollection<Artist>("Artist");
		public IMongoCollection<Payment> Albums => _database.GetCollection<Payment>("Payment");
	}
}
