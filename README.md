# ERC721ContractLibrary.Template
Template to get started creating ERC721 NFTs using OpenZeppelin

For more information on ERC721 check the spec https://eips.ethereum.org/EIPS/eip-721

This template provides an example of creating an NFT using the preset ERC721 smart contract provided by OpenZeppelin "ERC721PresetMinterPauserAutoId", an ERC721 standard with minting, pausing and auto id (evertime you mint an nft the id automatically created sequentially).

It also provides a development environment to work with OpenZeppelin smart contracts and Nethereum.

# Instructions 
Temporary instructions until this is created as a template

+ Clone this repo
+ Npm install to download all the open zeppelin libraries

# The integration example

+ The preset contract can be found [here](contracts/ERC721PresetMinterPauserAutoId.sol), this has been moved from the already created smart contracts by open zeppelin.
+ Nethereum code generation for all the services is already in place, if you want to alter your contract, open vscode solidity to compile and auto-generate your project.
+ Overall sample (Integration test)

This is based on the original and great sample by Andrew B Coathup @abcoathup from Open Zeppelin, please refer it for extra and detailed information https://forum.openzeppelin.com/t/create-an-nft-and-deploy-to-a-public-testnet-using-truffle/2961

and you will be able to achieve something like this https://testnets.opensea.io/assets/0xe4ac2f6980d7c5be4e4d14d381670648b0687398/0

```csharp

            //Example based on https://forum.openzeppelin.com/t/create-an-nft-and-deploy-to-a-public-testnet-using-truffle/ by Andrew B Coathup @abcoathup

            //Using rinkeby to demo opensea, if we dont want to use the configured client
            var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Rinkeby);
            //var web3 = _ethereumClientIntegrationFixture.GetWeb3();

            //using https://my-json-server.typicode.com/ to host the metadata database, this creates and auto json server api based on data hosted in github
            //see https://github.com/juanfranblanco/samplenftdb/blob/main/db.json as an example
            var erc721PresetMinter = new ERC721PresetMinterPauserAutoIdDeployment() {
                BaseURI = "https://my-json-server.typicode.com/juanfranblanco/samplenftdb/tokens/", 
                Name = "NFTArt", 
                Symbol = "NFA"};
            
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

```

