using MongoDB.Driver;
using SpotifyPool.Data;

namespace SpotifyPool.Services
{
	public class TestUserWithDBServices
	{
		private readonly IMongoCollection<User> _userCollection;
		public TestUserWithDBServices(SpotifyPoolDBContext context)
		{
			_userCollection = context.Users;
		}

		public async Task<List<User>> GetAllUser()
		{
			return await _userCollection.Find(_ => true).ToListAsync();
		}

		public async Task RegisterUser(User user)
		{
			await _userCollection.InsertOneAsync(user);
		}

		public async Task<User> LoginUser(string username, string password)
		{
			return await _userCollection.Find(u => u.Username ==  username && u.Password == password)
										.FirstOrDefaultAsync();
		}
	}
}
