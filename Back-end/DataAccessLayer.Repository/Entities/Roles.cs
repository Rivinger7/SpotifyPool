using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    [CollectionName("Roles")]
    public class Roles : MongoIdentityRole<ObjectId>
    {
    }
}
