using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;

using Weather.Models;

namespace Weather.Services
{
    //You replace this class witth your own Service from Project Part A
    public class OpenWeatherService
    {

        HttpClient httpClient = new HttpClient();
        readonly string apiKey = "b7218ab5e346e4579e66e7686f6814dc"; // Your API Key
        //b7218ab5e346e4579e66e7686f6814dc

        // part of your event and cache code here
        ConcurrentDictionary<string, (DateTime, Forecast)> data = new ConcurrentDictionary<string, (DateTime, Forecast)>();

        public async Task<Forecast> GetForecastAsync(string City)
        {
            //part of cache code here
            if (data.TryGetValue(City, out var _cacheCity))
            {
                if ((int)(DateTime.Now - _cacheCity.Item1).TotalSeconds <= 60)
                {
                    return _cacheCity.Item2;
                }
            }


            //https://openweathermap.org/current
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            //part of event and cache code here
            data.TryAdd(City, (DateTime.Now, forecast));


            //generate an event with different message if cached data
            return forecast;

        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            //part of cache code here
            if (data.TryGetValue($"{latitude},{longitude}", out var _cacheCity))
            {
                if ((int)(DateTime.Now - _cacheCity.Item1).TotalSeconds <= 60)
                {
                    return _cacheCity.Item2;
                }
            }

            //https://openweathermap.org/current
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            //part of event and cache code here
            data.TryAdd($"{latitude},{longitude}", (DateTime.Now, forecast));
            //generate an event with different message if cached data
            return forecast;
        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            // part of your read web api code here
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();

            // part of your data transformation to Forecast here
            List<ForecastItem> forecastItems = wd.list.Select(x => new ForecastItem
            {
                Temperature = x.main.temp,
                DateTime = UnixTimeStampToDateTime(x.dt),
                WindSpeed = x.wind.speed,
                Description = x.weather[0].description,
                Icon = x.weather[0].icon
            }).ToList();

            Forecast forecast = new Forecast();
            forecast.City = wd.city.name;
            forecast.Items = forecastItems;
            //generate an event with different message if cached data

            return forecast;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
