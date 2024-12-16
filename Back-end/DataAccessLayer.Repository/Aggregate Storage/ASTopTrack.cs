
using DataAccessLayer.Repository.Entities;

namespace DataAccessLayer.Repository.Aggregate_Storage
{
	public class ASTopTrack : TopTrack
	{
		//public List<Artist> Artists { get; set; } = [];
		public List<Track> Tracks { get; set; } = [];
	}
}
