using System.Text;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
           {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<string> Get()
    {

        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
                HttpResponseMessage response = await client.GetAsync("/posts");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json); // Output the JSON response
                }
                else
                {
                    Console.WriteLine("Failed to fetch data. Status code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        return "Success";
    }

    [HttpPost]
    public async Task<string> Post([FromBody] Post post)
    {
        using (HttpClient client = new HttpClient())
        {
            var obj = new { title = "asdas", body = "asdsad", userId = 12 };

            string postJson = Newtonsoft.Json.JsonConvert.SerializeObject(post);
            var content = new StringContent(postJson, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("adding a new post");

                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
                HttpResponseMessage response = await client.PostAsync("/posts", content);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine("Failed to fetch data. Status code: " + response.StatusCode);
                }
                _logger.LogInformation("adding a new post completed");
            }
            catch (Exception ex)
            {
                _logger.LogError("Adding post threw an exception");
            }
        }

        return "Success";
    }

}


public class Post
{
    public string Title { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
}