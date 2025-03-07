using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BusinessLogicLayer.Implement.Services.MemoryUsage
{
    public class MemoryMonitor : BackgroundService
    {
        private readonly ILogger<MemoryMonitor> _logger;
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "memory_log.txt");

        public MemoryMonitor(ILogger<MemoryMonitor> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Memory Monitor Service is starting...");
            CancellationTokenSource shutdownToken = new(TimeSpan.FromMinutes(90));

            try
            {
                while (!stoppingToken.IsCancellationRequested && !shutdownToken.IsCancellationRequested)
                {
                    CheckMemoryUsage();
                    //await Task.Delay(1000 * 60 * 20, stoppingToken); // Kiểm tra mỗi 20 phút
                    await Task.Delay(1000 * 30, stoppingToken); // Kiểm tra mỗi 5 giây
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Memory Monitor Service is shutting down after 30 minutes.");
            }
            finally
            {
                _logger.LogInformation("Memory Monitor Service stopped.");
            }
        }

        private static void CheckMemoryUsage()
        {
            Process currentProcess = Process.GetCurrentProcess();
            long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
            long workingSet = currentProcess.WorkingSet64;
            long peakWorkingSet = currentProcess.PeakWorkingSet64;

            string log = $"\n[Memory Usage - {DateTime.Now}]\n" +
                         $"GC Total Memory: {totalMemory / 1024 / 1024} MB\n" +
                         $"Working Set: {workingSet / 1024 / 1024} MB\n" +
                         $"Peak Working Set: {peakWorkingSet / 1024 / 1024} MB\n";

            Console.WriteLine(log);
            //_logger.LogDebug("{Log}", log);
            //WriteLog(log);
        }

        private static void WriteLog(string log)
        {
            using StreamWriter writer = new(LogFilePath, append: true);
            writer.WriteLine(log);
        }
    }
}