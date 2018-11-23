using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NBitcoin;
using NBitcoin.DataEncoders;

using Stratis.Bitcoin.Features.PoA;
using Stratis.Bitcoin.Networks;
using Stratis.Bitcoin.Utilities;
using Stratis.Sidechains.Networks;

using Xunit;
using Xunit.Abstractions;

namespace FedKeyPairGen
{
    public class MultisigAddressCreator
    {
        private readonly ITestOutputHelper output;

        public MultisigAddressCreator(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        //[Fact(Skip = "This is not a test, it is meant to be run upon creating a network")]
        public void Run_CreateMultisigAddresses()
        {
            var mainchainNetwork = Networks.Stratis.Testnet();
            var sidechainNetwork = FederatedPegNetwork.NetworksSelector.Testnet();

            this.CreateMultisigAddresses(mainchainNetwork, sidechainNetwork);
        }

        [Fact]
        //[Fact(Skip = "This is not a test, it is meant to be run upon creating a network")]
        public void Recalculate_RegTest_MultisigAddresses()
        {
            var mainchainNetwork = Networks.Stratis.Regtest();
            var sidechainNetwork = (FederatedPegRegTest)FederatedPegNetwork.NetworksSelector.Regtest();

            var mnemonics = sidechainNetwork.FederationMnemonics;

            Generate_Multisig_From_Mnemonic(mainchainNetwork, sidechainNetwork, mnemonics);
        }
        
        [Fact]
        //[Fact(Skip = "This is not a test, it is meant to be run upon creating a network")]
        public void Recalculate_Testnet_MultisigAddresses()
        {
            var mainchainNetwork = Networks.Stratis.Regtest();
            var sidechainNetwork = (FederatedPegTest)FederatedPegNetwork.NetworksSelector.Testnet();

            var mnemonics = sidechainNetwork.FederationMnemonics;

            Generate_Multisig_From_Mnemonic(mainchainNetwork, sidechainNetwork, mnemonics);
        }

        [Fact]
        public void Generate_PS1_Fragment()
        {
            var mainchainNetwork = Networks.Stratis.Regtest();
            var sidechainNetwork = (FederatedPegTest)FederatedPegNetwork.NetworksSelector.Testnet();

            var mnemonics = sidechainNetwork.FederationMnemonics;
            var pubKeysByMnemonic = mnemonics.ToDictionary(m => m, m => m.DeriveExtKey().PrivateKey.PubKey);

            var scriptAndAddresses = GenerateScriptAndAddresses(mainchainNetwork, sidechainNetwork, 2, pubKeysByMnemonic);

            var builder = new StringBuilder();
            builder.AppendLine("# FEDERATION DETAILS");
            Enumerable.Range(0, pubKeysByMnemonic.Count).ToList().ForEach(
                i =>
                    {
                        builder.AppendLine($"# Member{i + 1} mnemonic: {mnemonics[i]}");
                        builder.AppendLine($"# Member1 public key: {pubKeysByMnemonic[mnemonics[0]]}");
                    });

            builder.AppendLine($"# Redeem script: {scriptAndAddresses.payToMultiSig}");
            builder.AppendLine($"# Sidechan P2SH: {scriptAndAddresses.sidechainMultisigAddress.ScriptPubKey}");
            builder.AppendLine($"# Sidechain Multisig address: {scriptAndAddresses.sidechainMultisigAddress}");
            builder.AppendLine($"# Mainchain P2SH: {scriptAndAddresses.mainchainMultisigAddress.ScriptPubKey}");
            builder.AppendLine($"# Mainchain Multisig address: {scriptAndAddresses.mainchainMultisigAddress}");


            builder.AppendLine($"$mainchain_federationips = \"127.0.0.1:36011,127.0.0.1:36021,127.0.0.1:36031\"");
            builder.AppendLine($"$sidechain_federationips = \"127.0.0.1:36012,127.0.0.1:36022,127.0.0.1:36032\"");
            builder.AppendLine($"$redeemscript = \"{scriptAndAddresses.payToMultiSig}\"");
            builder.AppendLine($"$sidechain_multisig_address = \"{scriptAndAddresses.sidechainMultisigAddress}\"");
            Enumerable.Range(0, pubKeysByMnemonic.Count).ToList().ForEach(
                i =>
                    {
                        builder.AppendLine($"$gateway{i+1}_public_key = \"{pubKeysByMnemonic[mnemonics[i]]}\"");
                    });
            this.output.WriteLine(builder.ToString());
        }

        public void Generate_Multisig_From_Mnemonic(
            Network mainchainNetwork,
            Network sidechainNetwork,
            IList<Mnemonic> mnemonics)
        {
            Guard.Assert(mnemonics.Count > 0 && mnemonics.Count % 2 == 1);
            int quorum = mnemonics.Count / 2 + 1;
            PrintScriptAndAddresses(mainchainNetwork, sidechainNetwork, quorum, mnemonics);
        }

        public void CreateMultisigAddresses(Network mainchainNetwork, Network sidechainNetwork, int quorum = 2, int keysCount = 3)
        {
            var mnemonics = GenerateMnemonics(keysCount);

            PrintScriptAndAddresses(mainchainNetwork, sidechainNetwork, quorum, mnemonics);
        }

        private IList<Mnemonic> GenerateMnemonics(int keyCount)
        {
            return Enumerable.Range(0, keyCount)
                .Select(k => new Mnemonic(Wordlist.English, WordCount.Twelve))
                .ToList();
        }

        private void PrintScriptAndAddresses(
            Network mainchainNetwork,
            Network sidechainNetwork,
            int quorum,
            IList<Mnemonic> mnemonics)
        {
            var pubKeysByMnemonic = mnemonics.ToDictionary(m => m, m => m.DeriveExtKey().PrivateKey.PubKey);
            pubKeysByMnemonic.ToList().ForEach(
                m =>
                    {
                        this.output.WriteLine($"Mnemonic - Please note the following 12 words down in a secure place:");
                        this.output.WriteLine(string.Join(" ", m.Key.Words));
                        this.output.WriteLine($"PubKey   - Please share the following public key with the person responsible for the sidechain generation:");
                        this.output.WriteLine(Encoders.Hex.EncodeData((m.Value).ToBytes(false)));
                        this.output.WriteLine(Environment.NewLine);
                    });

            var (payToMultiSig, sidechainMultisigAddress, mainchainMultisigAddress) = GenerateScriptAndAddresses(mainchainNetwork, sidechainNetwork, quorum, pubKeysByMnemonic);
            PrintOutput(payToMultiSig, sidechainMultisigAddress, mainchainMultisigAddress);
        }

        private void PrintOutput(Script payToMultiSig, BitcoinAddress sidechainMultisigAddress, BitcoinAddress mainchainMultisigAddress)
        {
            this.output.WriteLine("Redeem script: " + payToMultiSig);
            this.output.WriteLine("Sidechan P2SH: " + sidechainMultisigAddress.ScriptPubKey);
            this.output.WriteLine("Sidechain Multisig address: " + sidechainMultisigAddress);
            this.output.WriteLine("Mainchain P2SH: " + mainchainMultisigAddress.ScriptPubKey);
            this.output.WriteLine("Mainchain Multisig address: " + mainchainMultisigAddress);
        }

        private (Script payToMultiSig, BitcoinAddress sidechainMultisigAddress, BitcoinAddress mainchainMultisigAddress)
            GenerateScriptAndAddresses(Network mainchainNetwork, Network sidechainNetwork, int quorum, Dictionary<Mnemonic, PubKey> pubKeysByMnemonic)
        {
            Script payToMultiSig = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(quorum, pubKeysByMnemonic.Values.ToArray());
            BitcoinAddress sidechainMultisigAddress = payToMultiSig.Hash.GetAddress(sidechainNetwork);
            BitcoinAddress mainchainMultisigAddress = payToMultiSig.Hash.GetAddress(mainchainNetwork);
            return (payToMultiSig, sidechainMultisigAddress, mainchainMultisigAddress);
        }
    }
}