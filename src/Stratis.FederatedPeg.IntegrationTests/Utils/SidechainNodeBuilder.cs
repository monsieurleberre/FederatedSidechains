using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NBitcoin;

using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Features.PoA;
using Stratis.Bitcoin.Features.PoA.IntegrationTests.Common;
using Stratis.Bitcoin.IntegrationTests.Common.EnvironmentMockUpHelpers;
using Stratis.Bitcoin.Tests.Common;
using Stratis.FederatedPeg.Features.FederationGateway;
using Stratis.Sidechains.Networks;

namespace Stratis.FederatedPeg.IntegrationTests.Utils
{
    public class SidechainNodeBuilder : NodeBuilder
    {
        public EditableTimeProvider TimeProvider { get; }

        private SidechainNodeBuilder(string rootFolder) : base(rootFolder)
        {
            this.TimeProvider = new EditableTimeProvider();
        }

        public static SidechainNodeBuilder CreatePoANodeBuilder(object caller, [CallerMemberName] string callingMethod = null)
        {
            string testFolderPath = TestBase.CreateTestDir(caller, callingMethod);
            SidechainNodeBuilder builder = new SidechainNodeBuilder(testFolderPath);
            builder.WithLogsDisabled();

            return builder;
        }

        public CoreNode CreateSidechainNode(PoANetwork network)
        {
            return this.CreateNode(new PoANodeRunner(this.GetNextDataFolderName(), network, this.TimeProvider), "poa.conf");
        }

        public CoreNode CreateSidechainNode(FederatedPegRegTest network, Key key)
        {
            string dataFolder = this.GetNextDataFolderName();
            CoreNode node = this.CreateNode(new PoANodeRunner(dataFolder, network, this.TimeProvider), "poa.conf");
            var quorum = network.FederationKeys.Count / 2 + 1;
            var pubKeys = network.FederationKeys.Select(k => k.PubKey).ToArray();
            var multisigRedeemScript = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(quorum, pubKeys);

            var settings = new NodeSettings(network, args: new string[]
               {
                   "-conf=poa.conf",
                   "-datadir=" + dataFolder,
                   $"-{FederationGatewaySettings.RedeemScriptParam}={multisigRedeemScript}"
               });
            var tool = new KeyTool(settings.DataFolder);
            tool.SavePrivateKey(key);

            return node;
        }
    }
}
