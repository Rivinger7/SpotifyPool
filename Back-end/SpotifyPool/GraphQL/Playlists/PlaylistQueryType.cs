using HotChocolate.Types;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool.GraphQL.Playlists
{
    public class PlaylistQueryType : ObjectTypeExtension<PlaylistQuery>
    {
        protected override void Configure(IObjectTypeDescriptor<PlaylistQuery> descriptor)
        {
            descriptor.Field(t => t.GetPlaylistAsync())
                .Authorize([$"{nameof(UserRole.Admin)}"])
                .Name("playlists")
                .Description("Get all playlists");
        }
    }
}
