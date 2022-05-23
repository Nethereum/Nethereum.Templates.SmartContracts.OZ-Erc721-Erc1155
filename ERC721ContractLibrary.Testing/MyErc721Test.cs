using System;
using System.Numerics;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.XUnitEthereumClients;
using Xunit;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ERC721ContractLibrary.Contracts.MyERC721;
using ERC721ContractLibrary.Contracts.MyERC721.ContractDefinition;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Newtonsoft.Json;


namespace ERC721ContractLibrary.Testing
{
    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class MyErc721Test
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        public MyErc721Test(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
        }


        [Fact]
        public async void ShouldDeployAndMintNFTToken()
        {
            //Using rinkeby to demo opensea, if we dont want to use the configured client
            //please input your infura id in appsettings.test.json
            var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Rinkeby);
            //var web3 = _ethereumClientIntegrationFixture.GetWeb3(); //if you want to use your local node (ie geth, uncomment this, see appsettings.test.json for further info)
            //example of configuration as legacy (not eip1559) to work on L2s
            web3.Eth.TransactionManager.UseLegacyAsDefault = true;

            //creating our deployment information (this includes the bytecode already)
            //This example creates an NFT Property (Real state) registry
            var erc721Deployment = new MyERC721Deployment() { Name = "Property Registry", Symbol = "PR" };

            //Deploy the erc721Minter
            var deploymentReceipt = await MyERC721Service.DeployContractAndWaitForReceiptAsync(web3, erc721Deployment);

            //creating a new service with the new contract address
            var erc721Service = new MyERC721Service(web3, deploymentReceipt.ContractAddress);

            //uploading to ipfs our documents
            var nftIpfsService = new NFTIpfsService("https://ipfs.infura.io:5001");
            var imageIpfs = await nftIpfsService.AddFileToIpfsAsync("Documents/TitlePlanImage.png");
            var titleIpfs = await nftIpfsService.AddFileToIpfsAsync("Documents/example_title_plan.pdf");
            var registerIpfs = await nftIpfsService.AddFileToIpfsAsync("Documents/example_register.pdf");
            
            //adding all our document ipfs links to the metadata and the description
            var metadataNFT = new NFTPropertyRegistryMetadata()
            {
                Name = "CS72510: Property registry example",
                AddressOfProperty = "23 Cottage Lane, Kerwick, PL14 3JP",
                Image = "ipfs://" + imageIpfs.Hash, //The image is what is displayed in market places like opean sea
                TitleDocument = "ipfs://" + titleIpfs.Hash,
                RegisterDocument = "ipfs://" + registerIpfs.Hash,
                PlanReference = "SD4008",
                TitleNumber = "CS72510",
                MapReference = "SY6676NE",
                ExternalUrl = "https://github.com/Nethereum/ERC721ContractLibrary.Template"

            };

            //Adding the metadata to ipfs
            var metadataIpfs =
                await nftIpfsService.AddNftsMetadataToIpfsAsync(metadataNFT, "PropertyRegistryMetadata.json");

            var addressToRegisterOwnership = "0xe612205919814b1995D861Bdf6C2fE2f20cDBd68";

            //Minting the nft with the url to the ipfs metadata
            var mintReceipt = await erc721Service.SafeMintRequestAndWaitForReceiptAsync(
                addressToRegisterOwnership, "ipfs://" + metadataIpfs.Hash);

            //we have just minted our first nft so the nft will have the id of 0. 
            var ownerOfToken = await erc721Service.OwnerOfQueryAsync(0);

            Assert.True(ownerOfToken.IsTheSameAddress(addressToRegisterOwnership));

            var addressOfToken = await erc721Service.TokenURIQueryAsync(0);

            Assert.Equal("ipfs://" + metadataIpfs.Hash, addressOfToken);

            //Url format  https://testnets.opensea.io/assets/[nftAddress]/[id]
            //opening opensea testnet to visualise the nft
            var ps = new ProcessStartInfo("https://testnets.opensea.io/assets/"+ deploymentReceipt.ContractAddress+ "/0")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);

            var transfer = await erc721Service.TransferFromRequestAndWaitForReceiptAsync(ownerOfToken, ownerOfToken, 0);
           Assert.False(transfer.HasErrors());

        }


        public class NFTPropertyRegistryMetadata : Nethereum.Contracts.Standards.ERC721.NftMetadata
        {
            [JsonProperty("titleNumber")]
            public string TitleNumber { get; set; }

            [JsonProperty("addressOfProperty")]
            public string AddressOfProperty { get; set; }

            [JsonProperty("planReference")]
            public string PlanReference { get; set; }

            [JsonProperty("mapReference")]
            public string MapReference { get; set; }

            [JsonProperty("titleDocument")]
            public string TitleDocument { get; set; }

            [JsonProperty("registerDocument")]
            public string RegisterDocument { get; set; }
        }


       

    }
}