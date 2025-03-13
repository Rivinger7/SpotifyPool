using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Aggregate_Storage
{
    public class ASAlbum : Album
    {
        public required Artist Artist { get; set; }
    }
}
