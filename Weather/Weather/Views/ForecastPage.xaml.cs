using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Weather.Models;
using Weather.Services;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class ForecastPage : ContentPage
    {
        OpenWeatherService service;
        GroupedForecast groupedforecast;
        string city;

        public ForecastPage()
        {
            InitializeComponent();
            
            service = new OpenWeatherService();
            groupedforecast = new GroupedForecast();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Code here will run right before the screen appears
            //You want to set the Title or set the City
            city = this.Title; 
            

            //This is making the first load of data
            MainThread.BeginInvokeOnMainThread(async () => {await LoadForecast();});
        }

        private async Task LoadForecast()
        {
            //Here you load the forecast
            
            var result = await service.GetForecastAsync(city);

            groupedforecast.City = result.City;
            groupedforecast.Items = result.Items.GroupBy(x => x.DateTime.Date);
            WeatherDataList.ItemsSource = groupedforecast.Items;
        }

        private async void MenuItem_OnClicked(object sender, EventArgs e) 
        {
            await LoadForecast();
        }
    }
}