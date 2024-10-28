using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using Utility.Coding;

namespace DataAccessLayer.Interface.MongoDB.Generic_Repository
{
    public interface IGenericRepository<TDocument> where TDocument : class
    {
        IMongoCollection<TDocument> Collection { get; }
        Task<IEnumerable<TDocument>> GetAllAsync();
        Task<TDocument> GetByIdAsync(string id);
        Task AddAsync(TDocument entity);
        Task UpdateAsync(string id, TDocument entity);
        Task DeleteAsync(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection">Collection có kiểu IMongoCollection</param>
        /// <param name="filter">Điều kiện cho danh sách lấy ra</param>
        /// <param name="sort">Cách sắp xếp tự định nghĩa</param>
        /// <param name="offset">Chỉ mục (index) của trang hiện tại cần phân trang</param>
        /// <param name="limit">Số lượng elements muốn có trong 1 trang</param>
        /// <returns></returns>
        Task<IEnumerable<TDocument>> Paging(int offset, int limit, FilterDefinition<TDocument>? filter = null, SortDefinition<TDocument>? sort = null);

		Task<IEnumerable<TResult>> GetAllDocumentsWithLookupAsync<TForeignDocument, TResult>(
            Expression<Func<TDocument, IEnumerable<object>>> localField,
            Expression<Func<TForeignDocument, object>> foreignField,
            Expression<Func<TResult, IEnumerable<TForeignDocument>>> resultField);
    }
}
