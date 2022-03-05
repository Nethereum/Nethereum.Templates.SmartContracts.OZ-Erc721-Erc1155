using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace ERC721ContractLibrary.Testing
{
    public class IPFSFileInfo
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Hash { get; set; }

    }

    /// <summary>
    ///  Simple http ipfs to add a file, for a complete implementation please use https://github.com/richardschneider/net-ipfs-http-client
    /// </summary>
    public class SimpleHttpIPFS
    {
        public string Url { get; }
        public Uri Uri { get; }
        private AuthenticationHeaderValue _authHeaderValue;

        public SimpleHttpIPFS(string url)
        {
            if (!url.EndsWith("api/v0"))
            {
                url = url.TrimEnd('/') + "/api/v0";
            }

            Url = url;
        }

        public SimpleHttpIPFS(string url, string userName, string password):this(url)
        {
            _authHeaderValue = GetBasicAuthenticationHeaderValue(userName, password);
        }

        public AuthenticationHeaderValue GetBasicAuthenticationHeaderValue(string userName, string password)
        {
            var byteArray = Encoding.UTF8.GetBytes(userName + ":" + password);
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<IPFSFileInfo> AddAsync(byte[] fileBytes, string fileName, bool pin = true)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new ByteArrayContent(fileBytes);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", fileName);
            using (var httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Authorization = _authHeaderValue;
                var query = pin ? "?pin=true&cid-version=1" : "?cid-version=1";
                var fullUrl = Url + "/add" + query;
                var httpResponseMessage = await httpClient.PostAsync(fullUrl, content);
                httpResponseMessage.EnsureSuccessStatusCode();
                var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create();
                    var message = serializer.Deserialize<IPFSFileInfo>(reader);

                    return message;
                }
            }
        }

        public async Task<IPFSFileInfo> AddObjectAsJson<T>(T objectToSerialise, string fileName, bool pin = true)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new JsonSerializer();
                var jsonTextWriter = new JsonTextWriter(new StreamWriter(ms));
                serializer.Serialize(jsonTextWriter, objectToSerialise);
                jsonTextWriter.Flush();
                ms.Position = 0;
                var node = await AddAsync(ms.ToArray(), fileName, true);
                return node;
            }
        }

        public Task<IPFSFileInfo> AddFileAsync(string path, bool pin = true)
        {
            var fileBytes = File.ReadAllBytes(path);
            var fileName = Path.GetFileName(path);
            return AddAsync(fileBytes, fileName, pin);
        }
    }
}
