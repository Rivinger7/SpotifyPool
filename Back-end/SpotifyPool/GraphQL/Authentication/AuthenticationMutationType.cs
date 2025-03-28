using HotChocolate.Types;

namespace SpotifyPool.GraphQL.Authentication
{
    public class AuthenticationMutationType : ObjectType<AuthenticationMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<AuthenticationMutation> descriptor)
        {
            descriptor.Authorize();

            descriptor.Field(x => x.Login(default!)).AllowAnonymous();
        }
    }
}
