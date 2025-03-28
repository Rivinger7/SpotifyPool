using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using HotChocolate.Types;
using SpotifyPool.GraphQL.Query;

namespace SpotifyPool.GraphQL.Playlists
{
    [ExtendObjectType(typeof(QueryInitialization))]
    public class PlaylistQuery(IPlaylist playlistService)
    {
        private readonly IPlaylist _playlistService = playlistService;

        public async Task<IEnumerable<PlaylistsResponseModel>> GetPlaylistAsync()
        {
            return await _playlistService.GetPlaylistsAsync();
        }
    }
}
