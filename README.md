# ERC721ContractLibrary.Template
Template to get started creating ERC721 NFTs using OpenZeppelin

For more information on ERC721 check the spec https://eips.ethereum.org/EIPS/eip-721

This template provides an example of creating an NFT using the preset ERC721 smart contract provided by OpenZeppelin "ERC721PresetMinterPauserAutoId", an ERC721 standard with minting, pausing and auto id (evertime you mint an nft the id automatically created sequentially).

It also provides a development environment to work with OpenZeppelin smart contracts and Nethereum.

# Instructions 
Temporary instructions until this is created as a full template

+ Clone this repo
+ Npm install to download all the open zeppelin libraries

# The integration example

+ The preset contract can be found [here](contracts/ERC721EnumerableUriStorage.sol), this is an extension of the default ERC721URIStorage open zeppelin.
+ Nethereum code generation for all the services is already in place, if you want to alter your contract, open vscode solidity to compile and auto-generate your project.
+ You will need to "npm install" at root, this is necesary to retrieve the "openzeppelin" contracts 
+ Overall sample (Integration test)

```csharp

            //Using rinkeby to demo opensea, if we dont want to use the configured client
            //please input your infura id in appsettings.test.json
            var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Rinkeby);
            //var web3 = _ethereumClientIntegrationFixture.GetWeb3(); //if you want to use your local node (ie geth, uncomment this, see appsettings.test.json for further info)
            //example of configuration as legacy (not eip1559) to work on L2s
            web3.Eth.TransactionManager.UseLegacyAsDefault = true;

            //creating our deployment information (this includes the bytecode already)
            //This example creates an NFT Property (Real state) registry
            var erc721Deployment = new ERC721EnumerableUriStorageDeployment() { Name = "Property Registry", Symbol = "PR" };

            //Deploy the erc721Minter
            var deploymentReceipt = await ERC721EnumerableUriStorageService.DeployContractAndWaitForReceiptAsync(web3, erc721Deployment);

            //creating a new service with the new contract address
            var erc721Service = new ERC721EnumerableUriStorageService(web3, deploymentReceipt.ContractAddress);

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
            var mintReceipt = await erc721Service.MintRequestAndWaitForReceiptAsync(
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
```

