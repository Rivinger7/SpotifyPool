using CsvHelper.Configuration.Attributes;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request
{
    public class TrackCsvModel
    {
        [Name("Track ID")]
        public string TrackId { get; set; } = default!;

        [Name("Track Name")]
        public string TrackName { get; set; } = default!;

        [Name("Artist Name(s)")]
        public string ArtistNames { get; set; } = default!;

        [Name("Duration (ms)")]
        public string DurationMs { get; set; } = default!;

        [Name("Popularity")]
        public string Popularity { get; set; } = default!;

        [Name("Danceability")]
        public string Danceability { get; set; } = default!;

        [Name("Energy")]
        public string Energy { get; set; } = default!;

        [Name("Key")]
        public string Key { get; set; } = default!;

        [Name("Loudness")]
        public string Loudness { get; set; } = default!;

        [Name("Mode")]
        public string Mode { get; set; } = default!;

        [Name("Speechiness")]
        public string Speechiness { get; set; } = default!;

        [Name("Acousticness")]
        public string Acousticness { get; set; } = default!;

        [Name("Instrumentalness")]
        public string Instrumentalness { get; set; } = default!;

        [Name("Liveness")]
        public string Liveness { get; set; } = default!;

        [Name("Valence")]
        public string Valence { get; set; } = default!;

        [Name("Tempo")]
        public string Tempo { get; set; } = default!;

        [Name("Time Signature")]
        public string TimeSignature { get; set; } = default!;
    }
}
