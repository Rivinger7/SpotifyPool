using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Aggregate_Storage
{
    public class ASUser : User
    {
        public Artist Artist { get; set; } = null!;
    }
}
