using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Aggregate_Storage
{
    public class ASTrack : Track
    {
        public List<Artist> Artists { get; set; } = [];
    }
}
