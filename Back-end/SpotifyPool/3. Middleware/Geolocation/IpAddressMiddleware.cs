namespace SpotifyPool._3._Middleware.Geolocation
{
    public class IpAddressMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy địa chỉ IP từ HttpContext
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            Console.WriteLine($"User IP Address: {ipAddress}");

            // Thực hiện các thao tác khác với địa chỉ IP nếu cần

            // Tiếp tục xử lý middleware tiếp theo trong pipeline
            await _next(context);
        }
    }

}
