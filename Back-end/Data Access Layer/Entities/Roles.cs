using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace Data_Access_Layer.Entities
{
	[CollectionName("Roles")]
	public class Roles : MongoIdentityRole<ObjectId>
	{
	}
}
