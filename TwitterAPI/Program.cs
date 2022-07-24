using Microsoft.Extensions.Configuration;

namespace TwitterAPI
{
    internal class Program
    {
        static async Task Main()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            APIInfo auth = configuration.GetSection("APIInfo").Get<APIInfo>();
            var request = new SampleRequest(auth);
            await request.GetTwitterSampleStream();
        }
    }
}