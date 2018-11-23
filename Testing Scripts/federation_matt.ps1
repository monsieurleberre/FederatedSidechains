
###############################
#    UPDATE THESE 5 VALUES    #
###############################
$root_datadir = "C:\Users\Matthieu\AppData\Roaming\StratisNode\federation" 
$path_to_federationgatewayd = "C:\Users\Matthieu\source\repos\_FederatedSidechains\src\Stratis.FederationGatewayD"
$path_to_sidechaind = "C:\Users\Matthieu\source\repos\_FederatedSidechains\src\Stratis.SidechainD"
$path_to_stratisd = "C:\Users\Matthieu\source\repos\StratisBitcoinFullNode\src\Stratis.StratisD"
$path_to_stratis_wallet_with_funds = "C:\Users\Matthieu\AppData\Roaming\StratisNode\stratis\StratisTest\walletTest1.wallet.json"

New-Item -ItemType directory -Force -Path $root_datadir
New-Item -ItemType directory -Force -Path $root_datadir\gateway1\stratis\StratisTest
New-Item -ItemType directory -Force -Path $root_datadir\gateway2\stratis\StratisTest
New-Item -ItemType directory -Force -Path $root_datadir\gateway3\stratis\StratisTest
New-Item -ItemType directory -Force -Path $root_datadir\MainchainUser\stratis\StratisTest
New-Item -ItemType directory -Force -Path $root_datadir\MiningNode
New-Item -ItemType directory -Force -Path $root_datadir\SidechainUser

If ((Test-Path $env:APPDATA\StratisNode\stratis\StratisTest) -And -Not (Test-Path $root_datadir\gateway1\stratis\StratisTest\blocks)) 
{
	$destinations = "$root_datadir\gateway1\stratis\StratisTest","$root_datadir\gateway2\stratis\StratisTest","$root_datadir\gateway3\stratis\StratisTest","$root_datadir\MainchainUser\stratis\StratisTest"
	$destinations | % {Copy-Item $env:APPDATA\StratisNode\stratis\StratisTest\blocks -Recurse -Destination $_}
	$destinations | % {Copy-Item $env:APPDATA\StratisNode\stratis\StratisTest\chain -Recurse -Destination $_}
	$destinations | % {Copy-Item $env:APPDATA\StratisNode\stratis\StratisTest\coinview -Recurse -Destination $_}
    Copy-Item -Path $path_to_stratis_wallet_with_funds -Destination $root_datadir\MainchainUser\stratis\StratisTest
}

# FEDERATION DETAILS
# Member1 mnemonic: ensure feel swift crucial bridge charge cloud tell hobby twenty people mandate
# Member1 public key: 02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65
# Member2 mnemonic: quiz sunset vote alley draw turkey hill scrap lumber game differ fiction
# Member1 public key: 02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65
# Member3 mnemonic: exchange rent bronze pole post hurry oppose drama eternal voice client state
# Member1 public key: 02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65
# Redeem script: 2 02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65 027ce19209dd1212a6a4fc2b7ddf678c6dea40b596457f934f73f3dcc5d0d9ee55 03093239d5344ddb4c69c46c75bd629519e0b68d2cfc1a86cd63115fd068f202ba 3 OP_CHECKMULTISIG
# Sidechan P2SH: OP_HASH160 42938bb61378468a38629c4ffa1521759d028357 OP_EQUAL
# Sidechain Multisig address: pBcbZ2NwMZHzwQ9SmpZYqfa1Lx8aprZ6H9
# Mainchain P2SH: OP_HASH160 42938bb61378468a38629c4ffa1521759d028357 OP_EQUAL
# Mainchain Multisig address: 2MyKFLbvhSouDYeAHhxsj9a5A4oV71j7SPR
$mainchain_federationips = "127.0.0.1:36011,127.0.0.1:36021,127.0.0.1:36031"
$sidechain_federationips = "127.0.0.1:36012,127.0.0.1:36022,127.0.0.1:36032"
$redeemscript = "2 02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65 027ce19209dd1212a6a4fc2b7ddf678c6dea40b596457f934f73f3dcc5d0d9ee55 03093239d5344ddb4c69c46c75bd629519e0b68d2cfc1a86cd63115fd068f202ba 3 OP_CHECKMULTISIG"
$sidechain_multisig_address = "pBcbZ2NwMZHzwQ9SmpZYqfa1Lx8aprZ6H9"
$gateway1_public_key = "02eef7619de25578c9717a289d08c61d4598b2bd81d2ee5db3072a07fa2d121e65"
$gateway2_public_key = "027ce19209dd1212a6a4fc2b7ddf678c6dea40b596457f934f73f3dcc5d0d9ee55"
$gateway3_public_key = "03093239d5344ddb4c69c46c75bd629519e0b68d2cfc1a86cd63115fd068f202ba"

$color_gateway1 = "0E" # light yellow on black
$color_gateway2 = "0A" # light green on black
$color_gateway3 = "09" # light blue on black
$color_miner    = "0C" # light red on black
$color_wallets  = "0D" # light purple on black

# The interval between starting the networks run, in seconds.
$interval_time = 5
$long_interval_time = 10 


cd $path_to_federationgatewayd

# Federation member 1 main and side
start-process cmd -ArgumentList "/k color $color_gateway1 && dotnet run -mainchain -agentprefix=fed1main -datadir=$root_datadir\gateway1 -port=36011 -apiport=38011 -counterchainapiport=38012 -federationips=$mainchain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway1_public_key"
timeout $long_interval_time
start-process cmd -ArgumentList "/k color $color_gateway1 && dotnet run -sidechain -agentprefix=fed1side -datadir=$root_datadir\gateway1 mine=1 mineaddress=$sidechain_multisig_address -port=36012 -apiport=38012 -counterchainapiport=38011 -txindex=1 -federationips=$sidechain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway1_public_key"
timeout $interval_time


# Federation member 2 main and side
start-process cmd -ArgumentList "/k color $color_gateway2 && dotnet run -mainchain -agentprefix=fed2main -datadir=$root_datadir\gateway2 -port=36021 -apiport=38021 -counterchainapiport=38022 -federationips=$mainchain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway2_public_key"
timeout $long_interval_time
start-process cmd -ArgumentList "/k color $color_gateway2 && dotnet run -sidechain -agentprefix=fed2side -datadir=$root_datadir\gateway2 mine=1 mineaddress=$sidechain_multisig_address -port=36022 -apiport=38022 -counterchainapiport=38021 -txindex=1 -federationips=$sidechain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway2_public_key"
timeout $interval_time


# Federation member 3 main and side
start-process cmd -ArgumentList "/k color $color_gateway3 && dotnet run -mainchain -agentprefix=fed3main -datadir=$root_datadir\gateway3 -port=36031 -apiport=38031 -counterchainapiport=38032 -federationips=$mainchain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway3_public_key"
timeout $long_interval_time
start-process cmd -ArgumentList "/k color $color_gateway3 && dotnet run -sidechain -agentprefix=fed3side -datadir=$root_datadir\gateway3 mine=1 mineaddress=$sidechain_multisig_address -port=36032 -apiport=38032 -counterchainapiport=38031 -txindex=1 -federationips=$sidechain_federationips -redeemscript=""$redeemscript"" -publickey=$gateway3_public_key"
timeout $interval_time


cd $path_to_stratisd

# MainchainUser
start-process cmd -ArgumentList "/k color $color_wallets && dotnet run -testnet -port=36178 -apiport=38221 -agentprefix=mainuser -datadir=$root_datadir\MainchainUser"
timeout $interval_time


cd $path_to_sidechaind

# SidechainUser
start-process cmd -ArgumentList "/k color $color_wallets && dotnet run -port=26179 -apiport=38225 -agentprefix=sideuser -datadir=$root_datadir\SidechainUser agentprefix=sc_user -addnode=127.0.0.1:36012 -addnode=127.0.0.1:36022 -addnode=127.0.0.1:36032"
timeout $interval_time
