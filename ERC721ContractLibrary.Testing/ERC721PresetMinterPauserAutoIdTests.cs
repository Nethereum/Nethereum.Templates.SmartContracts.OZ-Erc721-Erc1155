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
using System.Threading;
using ERC721ContractLibrary.Contracts.ERC721PresetMinterPauserAutoId;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using ERC721ContractLibrary.Contracts.ERC721PresetMinterPauserAutoId.ContractDefinition;

namespace ERC721ContractLibrary.Testing
{
    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class ERC721PresetMinterPauserAutoIdTests
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        public ERC721PresetMinterPauserAutoIdTests(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
        }

        //Example based on https://forum.openzeppelin.com/t/create-an-nft-and-deploy-to-a-public-testnet-using-truffle/ by Andrew B Coathup @abcoathup

        [Fact]
        public async void ShouldDeployAndMintNFTToken()
        {

            var destinationAddress = "0x6C547791C3573c2093d81b919350DB1094707011";
            //Using rinkeby to demo opensea, if we dont want to use the configured client
            var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Rinkeby);
            //var web3 = _ethereumClientIntegrationFixture.GetWeb3();

            //using https://my-json-server.typicode.com/ to host the metadata database, this creates and auto json server api based on data hosted in github
            //see https://github.com/juanfranblanco/samplenftdb/blob/main/db.json
            var erc721PresetMinter = new ERC721PresetMinterPauserAutoIdDeployment() {BaseURI = "https://my-json-server.typicode.com/juanfranblanco/samplenftdb/tokens/", Name = "NFTArt", Symbol = "NFA"};
            
            //Deploy the erc721Minter
            var deploymentReceipt = await ERC721PresetMinterPauserAutoIdService.DeployContractAndWaitForReceiptAsync(web3, erc721PresetMinter);

            var erc721PresetMinterService = new ERC721PresetMinterPauserAutoIdService(web3, deploymentReceipt.ContractAddress);

            var addressToGiveFirstToken = "0x0445c33bdce670d57189158b88c0034b579f37ce";

            var mintReceipt = await erc721PresetMinterService.MintRequestAndWaitForReceiptAsync(
                addressToGiveFirstToken);

            //we have just minted our first nft so the nft will have the id of 0. 
            var ownerOfTokem = await erc721PresetMinterService.OwnerOfQueryAsync(0);

            
            Assert.True(ownerOfTokem.IsTheSameAddress(addressToGiveFirstToken));

            //View example token here https://testnets.opensea.io/assets/0xe4ac2f6980d7c5be4e4d14d381670648b0687398/0
            //Url format  https://testnets.opensea.io/assets/[nftAddress]/[id]

        }


    }
}