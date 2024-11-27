using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using DataAccessLayer.Repository.Aggregate_Storage;

using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;


//using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataAccessLayer.Implement.MongoDB.Generic_Repository
{
    public class GenericRepository<TDocument>(IMongoDatabase database) : IGenericRepository<TDocument> where TDocument : class
    {
        private readonly IMongoDatabase _database = database;
        private IMongoCollection<TCollection> InCollection<TCollection>() where TCollection : class => _database.GetCollection<TCollection>(typeof(TCollection).Name);

        public IMongoCollection<TDocument> Collection => _database.GetCollection<TDocument>(typeof(TDocument).Name);

        public async Task<IEnumerable<TResult>> GetAllDocumentsWithLookupAsync<TForeignDocument, TResult>(
    Expression<Func<TDocument, IEnumerable<object>>> localField,    // Field in TDocument to join on
    Expression<Func<TForeignDocument, object>> foreignField,        // Field in TForeignDocument to match on
    Expression<Func<TResult, IEnumerable<TForeignDocument>>> resultField) // Field in TResult to store joined documents
        {
            // Start aggregating the collection of TDocument
            IAggregateFluent<TDocument> documentCollection = Collection.Aggregate();

            // Perform the lookup operation to join TDocument with TForeignDocument
            IAggregateFluent<TResult> pipeline = documentCollection.Lookup<TForeignDocument, TResult>(
                typeof(TForeignDocument).Name, // Foreign collection name (e.g., "artists" for Artist collection)
                new ExpressionFieldDefinition<TDocument>(localField),    // Local field from TDocument (e.g., ArtistIds in Track)
                new ExpressionFieldDefinition<TForeignDocument>(foreignField),  // Foreign field from TForeignDocument (e.g., SpotifyId in Artist)
                new ExpressionFieldDefinition<TResult>(resultField)         // Result field in TResult (e.g., Artists in ASTrack)
            ).As<TResult>();

            // Execute the pipeline and get the result as a list of TResult (e.g., ASTrack)
            IEnumerable<TResult> results = await pipeline.ToListAsync();

            return results; // Return the results (which would be of type ASTrack in your case)
        }

        public async Task<IEnumerable<TDocument>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync();
        }

        public async Task<TDocument> GetByIdAsync(string id)
        {
            return await Collection.Find(Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();
        }

        public async Task AddAsync(TDocument entity)
        {
            await Collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, UpdateDefinition<TDocument> entity)
        {
            await Collection.UpdateOneAsync(Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id)), entity);
        }

        public async Task DeleteAsync(string id)
        {
            await Collection.DeleteOneAsync(Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id)));
        }

        public async Task<IEnumerable<ASTrack>> GetAllTracksWithArtistAsync(int offset, int limit)
        {
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline  
            IAggregateFluent<Track> pipeLine = InCollection<Track>().Aggregate();

            // Lookup  
            IAggregateFluent<ASTrack> trackPipelines = pipeLine
                .Skip((offset - 1) * limit)
                .Limit(limit)
                .Lookup<Track, Artist, ASTrack>
                (InCollection<Artist>(), // The foreign collection  
                track => track.ArtistIds, // The field in Track that are joining on  
                artist => artist.Id, // The field in Artist that are matching against  
                result => result.Artists) // The field in ASTrack to hold the matched artists  
                .Project<ASTrack>(projectionDefinition);
                
            // Pipeline to list  
            IEnumerable<ASTrack> tracks = await trackPipelines.ToListAsync();

            return tracks;
        }

        public async Task<ASTrack> GetTrackWithArtistAsync(string trackId, FilterDefinition<Track>? preFilterDefinition = null)
        {
            // Filter
            preFilterDefinition ??= Builders<Track>.Filter.Empty;

            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = InCollection<Track>().Aggregate();

            // Lookup
            ASTrack trackPipeline = await aggregateFluent
                .Match(preFilterDefinition) // Match the custom filter
                .Lookup<Track, Artist, ASTrack>
                (InCollection<Artist>(), // The foreign collection
                track => track.ArtistIds, // The field in Track that are joining on
                artist => artist.Id, // The field in Artist that are matching against
                result => result.Artists) // The field in ASTrack to hold the matched artists
                .Match(track => track.Id == trackId) // Match the track by id
                .Unwind(result => result.Artists, new AggregateUnwindOptions<ASTrack>
                {
                    PreserveNullAndEmptyArrays = true
                })
                .Project<ASTrack>(projectionDefinition)
                .FirstOrDefaultAsync();

            return trackPipeline;
        }

        public async Task<IEnumerable<ASTrack>> GetServeralTracksWithArtistAsync(FilterDefinition<Track>? preFilterDefinition = null, FilterDefinition<ASTrack>? filterDefinition = null)
        {
            // Filter
            preFilterDefinition ??= Builders<Track>.Filter.Empty;
            filterDefinition ??= Builders<ASTrack>.Filter.Empty;

            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = InCollection<Track>().Aggregate();

            // Lookup
            IEnumerable<ASTrack> trackPipeline = await aggregateFluent
                .Match(preFilterDefinition) // Match the pre custom filter
                .Lookup<Track, Artist, ASTrack>
                (InCollection<Artist>(), // The foreign collection
                track => track.ArtistIds, // The field in Track that are joining on
                artist => artist.Id, // The field in Artist that are matching against
                result => result.Artists) // The field in ASTrack to hold the matched artists
                .Match(filterDefinition) // Match the custom filter
                .Project<ASTrack>(projectionDefinition)
                .ToListAsync();

            return trackPipeline;
        }

        public async Task<IEnumerable<TDocument>> Paging(int offset, int limit, FilterDefinition<TDocument>? filter = null, SortDefinition<TDocument>? sort = null)
        {
            sort ??= Builders<TDocument>.Sort.Ascending("_id");
            filter ??= Builders<TDocument>.Filter.Empty;

            // NOTE: aggregate facet là 1 phần trong aggreagate dùng để thu thập dữ liệu tổng hợp từ 1 tập dữ liệu. Trong facet có thể có nhiều pipeline

            // tạo một facet "count" để đếm số lượng dữ liệu
            AggregateFacet<TDocument, AggregateCountResult> countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(
                [
            PipelineStageDefinitionBuilder.Count<TDocument>()
                ]
            ));

            // tạo một facet "data" để lấy dữ liệu + phân trang
            AggregateFacet<TDocument, TDocument> dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(
                // build 1 pipeline để sắp xếp, bỏ qua (skip) và giới hạn (limit) số lượng dữ liệu trả về. thứ trả về là 1 TDocument
                [
            PipelineStageDefinitionBuilder.Sort(sort),
            PipelineStageDefinitionBuilder.Skip<TDocument>((offset - 1) * limit),
            PipelineStageDefinitionBuilder.Limit<TDocument>(limit)
                ]
            ));

            //chơi aggregate, kết hợp các facet trên cùng với filter cho ra kết quả
            IEnumerable<AggregateFacetResults> aggregation = await Collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet).ToListAsync();

            //** NẾU MUỐN LẤY SỐ LƯỢNG TRANG TẤT CẢ 
            //var count = aggregation.First()
            //                       .Facets.First(x => x.Name == "count")
            //                       .Output<AggregateCountResult>().FirstOrDefault()?.Count;

            //var totalPages = (int)Math.Ceiling((double)count! / limit);

            // trường hợp nếu lấy trong aggregate có count
            IEnumerable<TDocument> data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return data;
        }
    }
}
