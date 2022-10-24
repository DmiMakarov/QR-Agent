using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QR_Agent
{
    class Request
    {
        static HttpClient client;

        public Request()
        {
            var handler = new HttpClientHandler();
            // Disable certificate verification
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            client = new HttpClient(handler);
        }

        private void writeLog(string message, string log_path, int num_log_xml, string method = "")
        {
            string file_to_write = num_log_xml != -1 ? $"{log_path}\\XML\\{method}Response_{num_log_xml.ToString()}.xml" : $"{log_path}\\QR-Agent.log";
            
            using (StreamWriter sw = File.AppendText(file_to_write))
            {
                sw.WriteLine($"{DateTime.Now.ToString()} " + message);
            }
        }

        public async Task<String> PostXmlRequestKeeper(string xml_string, string url_keeper, string auth_keeper_name, string auth_keeper_pass, string log_path)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url_keeper))
            {
                var authenticationString = $"{auth_keeper_name}:{auth_keeper_pass}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                request.Content = new StringContent(xml_string, Encoding.UTF8, "application/xml"); ;
                request.Content.Headers.Add(@"Content-Length", xml_string.Length.ToString());
                string responce_str = "";
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    responce_str = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    writeLog($"\t Bad post keeper response from {url_keeper}, status code: {response.StatusCode.ToString()}", log_path, -1);
                }
                return responce_str;
            }
        }
        public async Task<String> PostJSONRequestServer(string requestId, string xml_string, string url_server, string token, string log_path)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url_server))
            {
                PostRequest postRequest = new PostRequest(requestId, Guid.NewGuid().ToString(), xml_string );
                string postRequest_str = JsonSerializer.Serialize(postRequest);
                request.Content = new StringContent(postRequest_str, Encoding.UTF8, "application/json");
                request.Headers.Add("X-AUTH-TOKEN", token);
                request.Content.Headers.Add(@"Content-Length", postRequest_str.Length.ToString());
                string responce_str = "";
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    responce_str = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    writeLog($"\t Bad post Server response from {url_server}, status code: {response.StatusCode.ToString()}", log_path, -1);
                }
                return responce_str;
            }
        }
        public async Task<string> GetJSONRequestServer(string url_server_get, string token_server, string log_path)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url_server_get))
            {
                string json_response = "";
                request.Headers.Add("X-AUTH-TOKEN", token_server);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    json_response = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    writeLog($"\t Bad get response from {url_server_get}, status code: {response.StatusCode.ToString()}", log_path, -1);
                }
                return json_response;
            }
        }

        public async Task RunAsync(string log_path,string url_server_get, string url_server_post,
            string url_keeper, string auth_keeper_name,string auth_keeper_password, string token_server)
        {
            int num_log_xml = int.Parse(ConfigurationManager.AppSettings.Get("num_log_xml"));
            try
            {
                writeLog($"Start sending requests", log_path, -1);
                var get_response_str = await GetJSONRequestServer(url_server_get, token_server, log_path);
                writeLog($"\t Recive get responce from {url_server_get}\n \t Response: {get_response_str}", log_path, -1);

                if (!string.IsNullOrEmpty(get_response_str))
                {
                    GetResponse get_response_obj = JsonSerializer.Deserialize<GetResponse>(get_response_str);

                    if (get_response_obj.data.Length == 0)
                    {
                        writeLog($"\t Empty data in responce from get request from {url_server_get}", log_path, -1);
                    }
                    else
                    {
                        foreach (DataResponse data in get_response_obj.data)
                        {

                            writeLog(data.request, log_path, num_log_xml, "Get");


                            var post_response_keeper = await PostXmlRequestKeeper(data.request, url_keeper,
                                                                                auth_keeper_name, auth_keeper_password, log_path);

                            writeLog($"\t Recive post responce from {url_keeper}", log_path, -1);
                            writeLog(post_response_keeper, log_path, num_log_xml, "PostKeeper");

                            if (!string.IsNullOrEmpty(post_response_keeper))
                            {
                                var post_response_server = await PostJSONRequestServer(data.requestId, post_response_keeper, url_server_post,
                                                                                    token_server, log_path);
                                writeLog($"\t Send post request to {url_server_post}\n \t Answer: {post_response_server}", log_path, -1);
                            }
                            else
                            {
                                writeLog($"\t Invalid response from keeper", log_path, -1);
                            }
                            num_log_xml += 1;
                        }
                    }
                }
                ConfigurationManager.AppSettings.Set("num_log_xml", num_log_xml.ToString());
                writeLog($"End sending requests to keeper", log_path, -1);
            }
            catch (Exception e)
            {
                num_log_xml += 1;
                ConfigurationManager.AppSettings.Set("num_log_xml", num_log_xml.ToString());
                writeLog($"When send requests get error: {e.Message}", log_path, -1);
            }
        }
    }

    // Structures for serializing and deserializing
    class DataResponse 
    {
        public string requestId { get; set; }
        public string request { get; set; }
    }
    class GetResponse
    {
        public string status { get; set; }
        public DataResponse[] data { get; set; }
        public string message { get; set; }
    }
    class PostRequest
    {
        public string requestId { get; set; }
        public string responseId { get; set; }
        public string response { get; set; }
        public PostRequest(string requestId, string responseId, string response)
        {
            this.requestId = requestId;
            this.responseId = responseId;
            this.response = response;
        }
    }

}

