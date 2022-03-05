using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ERC721ContractLibrary.Testing
{

    public class NftMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("external_url")]
        public string ExternalUrl { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class NFTIpfsService
    {
        private readonly string _userName;
        private readonly string _password;
        private readonly string _ipfsUrl;

       

        public NFTIpfsService(string ipfsUrl)
        {
            _ipfsUrl = ipfsUrl;
        }

        public NFTIpfsService(string ipfsUrl, string userName, string password) :this(ipfsUrl)
        {
            _userName = userName;
            _password = password;
        }
        public Task<IPFSFileInfo> AddNftsMetadataToIpfsAsync<T>(T metadata, string fileName) where T : NftMetadata
        {
            var ipfsClient = GetSimpleHttpIpfs();
            return ipfsClient.AddObjectAsJson(metadata, fileName);
        }

        public async Task<IPFSFileInfo> AddFileToIpfsAsync(string path)
        {
            var ipfsClient = GetSimpleHttpIpfs();
            var node = await ipfsClient.AddFileAsync(path);
            return node;

        }

        private SimpleHttpIPFS GetSimpleHttpIpfs()
        {
            if (_userName == null) return new SimpleHttpIPFS(_ipfsUrl);
            return new SimpleHttpIPFS(_ipfsUrl, _userName, _password);
        }
    }
}
