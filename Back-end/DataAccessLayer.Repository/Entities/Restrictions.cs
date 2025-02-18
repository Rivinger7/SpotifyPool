using SetupLayer.Enum.Services.Track;

namespace DataAccessLayer.Repository.Entities
{
    public class Restrictions
    {
        public required bool IsPlayable { get; set; }
        public required RestrictionReason Reason { get; set; }
        public string? Description { get; set; }
        public DateTime? RestrictionDate { get; set; }
    }
}
