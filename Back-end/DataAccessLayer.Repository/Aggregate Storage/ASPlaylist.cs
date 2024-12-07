using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Aggregate_Storage
{
    public class ASPlaylist : Playlist
    {
        public required User User { get; set; }
    }
}
