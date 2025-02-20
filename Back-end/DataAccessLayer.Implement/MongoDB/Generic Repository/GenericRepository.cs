using DataAccessLayer.Interface.MongoDB.Generic_Repository;
using DataAccessLayer.Repository.Aggregate_Storage;

using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;


//using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace DataAccessLayer.Implement.MongoDB.Generic_Repository
{
    public class GenericRepository<TDocument>(IMongoDatabase database) : IGenericRepository<TDocument> where TDocument : class
    {
        private readonly IMongoDatabase _database = database;
        private IMongoCollection<TCollection> InCollection<TCollection>() where TCollection : class => _database.GetCollection<TCollection>(typeof(TCollection).Name);

        public IMongoCollection<TDocument> Collection => _database.GetCollection<TDocument>(typeof(TDocument).Name);

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

        public async Task UpdateAsync(string id, UpdateDefinition<TDocument> updateDefinition)
        {
            await Collection.UpdateOneAsync(Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id)), updateDefinition);
        }

        public async Task DeleteAsync(string id)
        {
            await Collection.DeleteOneAsync(Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id)));
        }

        public async Task<IEnumerable<TDocument>> Paging(int offset, int limit, FilterDefinition<TDocument>? filter = null, SortDefinition<TDocument>? sort = null)
        {
            //Nếu không có sort, mặc định sắp xếp theo _id
            sort ??= Builders<TDocument>.Sort.Ascending("_id");
            filter ??= Builders<TDocument>.Filter.Empty;

            // NOTE: aggregate facet là 1 phần trong aggreagate dùng để thu thập dữ liệu tổng hợp từ 1 tập dữ liệu. Trong facet có thể có nhiều pipeline

            //Tạo một facet "count" để đếm số lượng dữ liệu
            AggregateFacet<TDocument, AggregateCountResult> countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(
                    [
                    PipelineStageDefinitionBuilder.Count<TDocument>()
                    ]
                    ));

            //Tạo một facet "data" để lấy dữ liệu + phân trang
            AggregateFacet<TDocument, TDocument> dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(
                // build 1 pipeline để sắp xếp, bỏ qua (skip) và giới hạn (limit) số lượng dữ liệu trả về. thứ trả về là 1 TDocument
                    [
                    PipelineStageDefinitionBuilder.Sort(sort),
                    PipelineStageDefinitionBuilder.Skip<TDocument>((offset - 1) * limit),
                    PipelineStageDefinitionBuilder.Limit<TDocument>(limit)
                    ]
                    ));

            //Thực hiện Aggregate, kết hợp các facet trên cùng với filter cho ra kết quả
            IEnumerable<AggregateFacetResults> aggregation = await Collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

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
