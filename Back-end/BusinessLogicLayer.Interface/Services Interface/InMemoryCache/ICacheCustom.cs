namespace Business_Logic_Layer.Services_Interface.InMemoryCache
{
    public interface ICacheCustom
    {
        /// <summary>
        /// In-memory caching dùng cho tầng API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getDataFunc"></param>
        /// <param name="cacheDuration"></param>
        /// <returns></returns>
        T GetOrSet<T>(string cacheKey, Func<T> getDataFunc, int cacheDuration = 10);
        /// <summary>
        /// In-memory caching dùng cho tầng API Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getDataFunc"></param>
        /// <param name="cacheDuration"></param>
        /// <returns></returns>
        Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getDataFunc, int cacheDuration = 10);
        /// <summary>
        /// In-memory caching dùng cho tầng API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="dataObject"></param>
        /// <param name="cacheDuration"></param>
        /// <returns></returns>
        T GetOrSet<T>(string cacheKey, T dataObject, int cacheDuration = 10);
        /// <summary>
        /// Xóa cacheKey cho In-memory caching dùng cho tầng API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        void RemoveCache<T>(string cacheKey);
    }
}
