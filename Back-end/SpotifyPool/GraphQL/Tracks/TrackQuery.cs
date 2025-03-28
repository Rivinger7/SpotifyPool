using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using HotChocolate.Types;
using SpotifyPool.GraphQL.Query;

namespace SpotifyPool.GraphQL.Tracks
{
    [ExtendObjectType(typeof(QueryInitialization))]
    public class TrackQuery(ITrack trackService)
    {
        private readonly ITrack _trackService = trackService;

        public async Task<IEnumerable<TrackResponseModel>> GetTracksAsync(TrackFilterModel filterModel, int offset = 1, int limit = 10)
        {
            return await _trackService.GetAllTracksAsync(offset, limit, filterModel);
        }

        public async Task<TrackResponseModel> GetTrackByIdAsync(string id)
        {
            return await _trackService.GetTrackAsync(id);
        }
    }
}
