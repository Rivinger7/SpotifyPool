using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using HotChocolate;

namespace SpotifyPool.GraphQL
{
    public class QueryTrack(ITrack trackService)
    {
        private readonly ITrack _trackService = trackService;

        [GraphQLName("getTracks")]
        public async Task<IEnumerable<TrackResponseModel>> GetTracksAsync(TrackFilterModel filterModel, int offset = 1, int limit = 10)
        {
            return await _trackService.GetAllTracksAsync(offset, limit, filterModel);
        }   
    }
}
