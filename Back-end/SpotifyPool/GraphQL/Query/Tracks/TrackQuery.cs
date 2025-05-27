using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SpotifyPool.GraphQL.Query;


namespace SpotifyPool.GraphQL.Query.Tracks
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

        public async Task<Track> TestQuery(string id, IResolverContext context, [Service] IUnitOfWork unitOfWork)
        {
            var selectedFields = GetSelectedFieldNames(context);
            var projection = BuildProjection<Track>(selectedFields);

            Console.WriteLine("[SelectedFields] " + string.Join(", ", selectedFields));


            var track = await unitOfWork.GetCollection<Track>().Find(u => u.Id == id)
                .Project<BsonDocument>(projection)
                .FirstOrDefaultAsync();

            //Console.WriteLine(BsonSerializer.Deserialize<Track>(track).ToString());
            //Console.WriteLine(BsonSerializer.Deserialize<Track>(track).ToJson());

            return BsonSerializer.Deserialize<Track>(track);
        }

        public IReadOnlyList<string> GetSelectedFieldNames(IResolverContext context)
        {
            return context.Selection.SyntaxNode.SelectionSet?.Selections
                .OfType<FieldNode>()
                .Select(f => f.Name.Value)
                .ToList()
                ?? new List<string>();
        }

        public static ProjectionDefinition<T> BuildProjection<T>(IEnumerable<string> fields)
        {
            var builder = Builders<T>.Projection;

            // Bắt buộc phải có _id (nếu không sẽ lỗi)
            //var projection = builder.Include("_id").Include("Name").Include("AudioFeatures");
            var projection = builder.Include("_id");

            foreach (var field in fields)
            {
                string capitalizedField = char.ToUpper(field[0]) + field.Substring(1);

                projection = projection.Include(capitalizedField);
                Console.WriteLine("===================================");
                Console.WriteLine(capitalizedField);
                Console.WriteLine("===================================");
            }

            return projection;
        }

    }

}
