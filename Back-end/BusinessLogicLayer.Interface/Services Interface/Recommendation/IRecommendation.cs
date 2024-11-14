using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interface.Services_Interface.Recommendation
{
    public interface IRecommendation
    {
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, int k = 5);
    }
}
