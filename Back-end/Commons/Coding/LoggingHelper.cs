using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Utility.Coding
{
    public class LoggingHelper
    {
        /// <summary>
        /// Lấy ILogger từ IServiceProvider để sử dụng trong toàn bộ ứng dụng
        /// </summary>
        /// <param name="services">IServiceProvider từ DI container</param>
        /// <returns>ILogger</returns>
        public static ILogger GetLogger(IServiceProvider services)
        {
            return services.GetRequiredService<ILogger<LoggingHelper>>();
        }
    }
}
