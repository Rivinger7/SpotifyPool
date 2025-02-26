namespace BusinessLogicLayer.ModelView.Service_Model_Views.PlaybackState
{
    public class PlaybackStateSync
    {
        public string TrackId { get; set; } = default!;
        public double CurrentTime { get; set; }
        public bool IsPlaying { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
