using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SelDatUnilever_Ver1._00.Communication.HttpBridge
{
   public class BridgeClientRequest
    {
        public event Action<String> ReceiveResponseHandler;
        public event Action<int> ErrorBridgeHandler;
        public BridgeClientRequest() { }
        public async Task<String> PostCallAPI(string url, String jsonObject)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                   
                    var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    String jsonString = await response.Content.ReadAsStringAsync();
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
               
            }
            return "";
        }
        public static async Task<string> _PostCallAPIAsync(string url, String jsonObject)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    String jsonString = await response.Content.ReadAsStringAsync();
                    return jsonString;
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
        public async Task<String> GetCallAPI(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    String jsonString = await response.Content.ReadAsStringAsync();
                    return jsonString;
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
    }
}
