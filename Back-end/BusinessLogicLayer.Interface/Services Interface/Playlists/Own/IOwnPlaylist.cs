

using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Own
{
	public interface IOwnPlaylist
	{
		Task GetAllPlaylistAsync();
		Task AddNewPlaylistAsync();
		Task DeletePlaylistAsync(string playlistID);
		Task UpdatePlaylistAsync(string playlistID, UpdatePlaylistRequestModel request);

		Task PinPlaylistAsync(string playlistID);
		Task UnpinPlaylistAsync(string playlistID);
	}
}
