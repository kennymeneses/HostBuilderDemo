using Serilog;
using System.Net.Http;
using System.Threading.Tasks;

namespace HostBuilderDemo.Host
{
    public class WarmupHttpRequest
    {
        private readonly ILogger _logger = Log.Logger.ForContext<WarmupHttpRequest>();

        public WarmupHttpRequest()
        {

        }

        public async Task StarWarmup()
        {
            var url = "https://pokeapi.co/api/v2/pokemon/ditto";

            using (var client = new HttpClient())
            {

                var Answer = await client.GetAsync(url);
                var AnswerToString = await Answer.Content.ReadAsStringAsync();
                var Final = System.Text.Json.JsonSerializer.Deserialize<object>(AnswerToString);

                _logger.Debug("Warmup was finished to pokeApi was succesfully");
            }
        }
    }
}
