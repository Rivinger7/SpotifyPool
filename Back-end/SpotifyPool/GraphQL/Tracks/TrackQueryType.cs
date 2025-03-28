using HotChocolate.Types;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool.GraphQL.Tracks
{
    public class TrackQueryType : ObjectTypeExtension<TrackQuery>
    {
        protected override void Configure(IObjectTypeDescriptor<TrackQuery> descriptor)
        {
            descriptor.Authorize([nameof(UserRole.Admin)]); // Mặc định tất cả yêu cầu login

            //descriptor.Field(x => x.GetTrackByIdAsync(default!)).AllowAnonymous(); // Cho phép truy cập public
        }
    }
}
