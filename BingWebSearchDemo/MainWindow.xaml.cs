using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BingWebSearchDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string WebSearchEndPoint = "https://api.cognitive.microsoft.com/bing/v7.0/search?";
        public MainWindow()
        {
            InitializeComponent();
            WebSearchClient = new HttpClient();
            WebSearchClient.DefaultRequestHeaders.Add("Siehe Buch", "Ihr API Schlüssel");
        }
       
        public HttpClient WebSearchClient
        {
            get;
            set;
        }

        async Task<IEnumerable<WebSearch>> SearchBingWeb(string searchText)
        {
            List<WebSearch> websearch = new List<WebSearch>();

            string count = "20";
            string offset = "0";
            string mkt = "en-us";
            var result = await RequestAndAutoRetryWhenThrottled(() => WebSearchClient.GetAsync(string.Format("{0}q={1}&count={2}&offset={3}&mkt={4}", WebSearchEndPoint, WebUtility.UrlEncode(searchText), count, offset, mkt)));
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(json);
            for (int i = 0; i < 20; i++)
            {
                websearch.Add(new WebSearch
                {
                    Id = data.webPages.value[i].id,
                    Name = data.webPages.value[i].name,                   
                    Snippet = data.webPages.value[i].snippet                    
                });
            }
            return websearch;
        }

        private async Task<HttpResponseMessage> RequestAndAutoRetryWhenThrottled(Func<Task<HttpResponseMessage>> action)
        {
            int retriesLeft = 6;
            int delay = 500;

            HttpResponseMessage response = null;

            while (true)
            {
                response = await action();

                if ((int)response.StatusCode == 429 && retriesLeft > 0)
                {
                    await Task.Delay(delay);
                    retriesLeft--;
                    delay *= 2;
                    continue;
                }
                else
                {
                    break;
                }
            }

            return response;
        }

        private async void tbSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (tbSearchText.Text != null)
                    lstView.ItemsSource = await SearchBingWeb(tbSearchText.Text);
            }
            catch (Exception ex)
            {
                //ToDo
            }
        }
    }
    public class WebSearch
    {
        public string Id { get; set; }
        public string Name { get; set; }      
        public string Snippet { get; set; }
        
    }
}
