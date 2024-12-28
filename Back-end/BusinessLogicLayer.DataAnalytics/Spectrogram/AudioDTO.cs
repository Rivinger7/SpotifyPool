using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DataAnalytics.Spectrogram
{
    public class AudioDTO
    {
        public float Duration { get; set; }
        public float Key { get; set; }
        public float TimeSignature { get; set; }
        public float Mode { get; set; }
        public float Acousticness { get; set; }
        public float Danceability { get; set; }
        public float Energy { get; set; }
        public float Instrumentalness { get; set; }
        public float Liveness { get; set; }
        public float Loudness { get; set; }
        public float Speechiness { get; set; }
        public float Tempo { get; set; }
        public float Valence { get; set; }
    }
}
