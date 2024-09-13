using dotenv.net;

namespace SpotifyPool.Infrastructure.EnvironmentVariable
{
    public static class EnvironmentVariableLoader
    {
        public static void LoadEnvironmentVariable()
        {
            // Construct the full path to the .env file located in the "4. Application" folder
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "4. Application", ".env");

            // Load the .env file from the specific path using DotEnvOptions
            var options = new DotEnvOptions(
                // Pass the path to the .env file
                envFilePaths: [envFilePath],
                // No need to probe for .env as we are specifying the path
                probeForEnv: false
            );
            DotEnv.Load(options);
        }
    }
}
