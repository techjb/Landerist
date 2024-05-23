using GenerativeAI;
using System.ComponentModel;

namespace landerist_library.Parse.Listing.Gemini
{
    public enum Unit
    {
        Celsius,
        Fahrenheit,
        Imperial
    }

    public class Weather
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public Unit Unit { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    [GenerativeAIFunctions]
    public interface IWeatherFunctions
    {
        [Description("Get the current weather in a given location")]
        public Weather GetCurrentWeatherAsync(
            [Description("The city and state, e.g. San Francisco, CA")] string location,
            Unit unit = Unit.Celsius,
            CancellationToken cancellationToken = default);
    }

    public class WeatherService : IWeatherFunctions
    {
        public Weather GetCurrentWeatherAsync(string location, Unit unit = Unit.Celsius, CancellationToken cancellationToken = default)
        {
            return new Weather
            {
                Location = location,
                Temperature = 22.0,
                Unit = unit,
                Description = "Sunny",
            };
        }
    }
}
