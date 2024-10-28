using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace DataAccessLayer.Implement.MongoDB.Generic_Repository
{
    public class GenericRepository<TDocument>(IMongoDatabase database) : IGenericRepository<TDocument> where TDocument : class
    {
        private readonly IMongoDatabase _database = database;

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
            return await Collection.Find(Builders<TDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task AddAsync(TDocument entity)
        {
            await Collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, TDocument entity)
        {
            await Collection.ReplaceOneAsync(Builders<TDocument>.Filter.Eq("_id", id), entity);
        }

        public async Task DeleteAsync(string id)
        {
            await Collection.DeleteOneAsync(Builders<TDocument>.Filter.Eq("_id", id));
        }


        public async Task<IEnumerable<TDocument>> Paging(int offset, int limit, FilterDefinition<TDocument>? filter = null, SortDefinition<TDocument>? sort = null)
        {
            sort ??= Builders<TDocument>.Sort.Ascending("_id");
            filter ??= Builders<TDocument>.Filter.Empty;

            // NOTE: aggregate facet là 1 phần trong aggreagate dùng để thu thập dữ liệu tổng hợp từ 1 tập dữ liệu. Trong facet có thể có nhiều pipeline

            // tạo một facet "count" để đếm số lượng dữ liệu
            AggregateFacet<TDocument, AggregateCountResult> countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
            PipelineStageDefinitionBuilder.Count<TDocument>()
                }
            ));

            // tạo một facet "data" để lấy dữ liệu + phân trang
            AggregateFacet<TDocument, TDocument> dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[] // build 1 pipeline để sắp xếp, bỏ qua (skip) và giới hạn (limit) số lượng dữ liệu trả về. thứ trả về là 1 TDocument
                {
            PipelineStageDefinitionBuilder.Sort(sort),
            PipelineStageDefinitionBuilder.Skip<TDocument>((offset - 1) * limit),
            PipelineStageDefinitionBuilder.Limit<TDocument>(limit)
                }
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
