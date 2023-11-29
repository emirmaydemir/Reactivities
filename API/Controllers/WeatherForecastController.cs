using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]// Model Doğrulama, HTTP 400 Hata Kodları, Dolaylı Bağlama vb özellikleri kazandırız bize
[Route("[controller]")] //[controller] yer tutucusu, Controller sınıfının adını alır ve bu adı route şablonunda kullanır. Bu kullanım, Controller sınıflarının adının değiştiğinde veya başka bir Controller sınıfı eklediğinizde, route şablonunu her seferinde güncellemenize gerek olmadan dinamik bir şekilde uyum sağlamak için kullanışlıdır.
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

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
