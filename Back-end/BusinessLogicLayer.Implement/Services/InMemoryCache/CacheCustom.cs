using Business_Logic_Layer.Services_Interface.InMemoryCache;
using BusinessLogicLayer.Implement.CustomExceptions;
using Microsoft.Extensions.Caching.Memory;

namespace BusinessLogicLayer.Implement.Services.InMemoryCache
{
    public class CacheCustom(IMemoryCache cache) : ICacheCustom
    {
        private readonly IMemoryCache _cache = cache;

        public T GetOrSet<T>(string cacheKey, Func<T> getDataFunc, int cacheDuration = 10)
        {
            // Nếu không truyền cacheDuration, đặt mặc định là 10 phút
            TimeSpan cacheDurationTimeSpan = TimeSpan.FromMinutes(cacheDuration);

            // Tạo fullCacheKey duy nhất
            string fullCacheKey = $"{typeof(T).Name}_{cacheKey}";

            // Kiểm tra xem cache đã tồn tại chưa
            if (!_cache.TryGetValue(fullCacheKey, out T cacheValue))
            {
                // Nếu chưa, gọi hàm để lấy dữ liệu
                cacheValue = getDataFunc();

                // Kiểm tra nếu cacheValue là null (đối với object)
                if (cacheValue is null)
                {
                    throw new DataNotFoundCustomException($"Not found {typeof(T).Name} with ID {cacheKey}");
                }

                // Kiểm tra nếu cacheValue là List hoặc IEnumerable
                if (cacheValue is IEnumerable<T> list && !list.Any())
                {
                    return cacheValue;
                }

                // Cấu hình cache entry
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDurationTimeSpan
                };

                // Lưu dữ liệu vào cache
                _cache.Set(fullCacheKey, cacheValue, cacheEntryOptions); 
            }

            return cacheValue;
        }

        public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getDataFunc, int cacheDuration = 10)
        {
            // Nếu không truyền cacheDuration, đặt mặc định là 10 phút
            TimeSpan cacheDurationTimeSpan = TimeSpan.FromMinutes(cacheDuration);

            // Tạo fullCacheKey duy nhất
            string fullCacheKey = $"{typeof(T).Name}_{cacheKey}";

            // Kiểm tra xem cache đã tồn tại chưa
            if (!_cache.TryGetValue(fullCacheKey, out T cacheValue))
            {
                // Nếu chưa có trong cache, gọi hàm để lấy dữ liệu
                cacheValue = await getDataFunc();

                // Kiểm tra nếu cacheValue là null (đối với object)
                if (cacheValue is null)
                {
                    throw new DataNotFoundCustomException($"Not found {typeof(T).Name} with ID {cacheKey}");
                }

                // Kiểm tra nếu cacheValue là List hoặc IEnumerable và rỗng
                if (cacheValue is IEnumerable<object> list && !list.Any())
                {
                    // Không lưu vào cache nếu danh sách rỗng
                    return cacheValue;
                }

                // Cấu hình cache entry
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDurationTimeSpan
                };

                // Lưu dữ liệu vào cache
                _cache.Set(fullCacheKey, cacheValue, cacheEntryOptions);
            }

            return cacheValue;
        }


        public T GetOrSet<T>(string cacheKey, T dataObject, int cacheDuration = 10)
        {
            // Nếu không truyền cacheDuration, đặt mặc định là 10 phút
            TimeSpan cacheDurationTimeSpan = TimeSpan.FromMinutes(cacheDuration);

            // Tạo fullCacheKey duy nhất
            string fullCacheKey = $"{typeof(T).Name}_{cacheKey}";

            // Kiểm tra xem cache đã tồn tại chưa
            if (!_cache.TryGetValue(fullCacheKey, out T cacheValue))
            {
                // Nếu chưa, dùng đối tượng được truyền vào
                cacheValue = dataObject;

                // Cấu hình cache entry
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDurationTimeSpan
                };

                // Lưu dữ liệu vào cache
                _cache.Set(fullCacheKey, cacheValue, cacheEntryOptions);
            }

            return cacheValue;
        }


        public void RemoveCache<T>(string cacheKey)
        {
            // Tạo fullCacheKey tương tự như hàm GetOrSet
            string fullCacheKey = $"{typeof(T).Name}_{cacheKey}";

            // Xóa mục cache
            _cache.Remove(fullCacheKey);
        }
    }
}
