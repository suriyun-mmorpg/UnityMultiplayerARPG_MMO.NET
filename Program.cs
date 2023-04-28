using Cysharp.Threading;
using LiteNetLibManager;
using MultiplayerARPG;
using MultiplayerARPG.MMO;
using Newtonsoft.Json;

// Read config files
string configFolder = "./Config";
string configFilePath;

bool configFileFound = false;
Dictionary<string, object> serverConfig = new Dictionary<string, object>();
List<string> spawningMapIds = new List<string>();
configFilePath = configFolder + "/serverConfig.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    configFileFound = true;
    string dataAsJson = File.ReadAllText(configFilePath);
    serverConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataAsJson);
}
else
{
    Logging.Log($"Can't find config file: {configFilePath}");
}

configFilePath = configFolder + "/items.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    string dataAsJson = File.ReadAllText(configFilePath);
    DataManager.Items = JsonConvert.DeserializeObject<Dictionary<int, MinimalItem>>(dataAsJson);
}
else
{
    Logging.Log($"Can't find config file: {configFilePath}");
}

configFilePath = configFolder + "/characterCreationData.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    string dataAsJson = File.ReadAllText(configFilePath);
    DataManager.CharacterCreationData = JsonConvert.DeserializeObject<CharacterCreationData>(dataAsJson);
}
else
{
    Logging.Log($"Can't find config file: {configFilePath}");
}

// Prepare server instances
CentralNetworkManager centralNetworkManager = new CentralNetworkManager();
MapSpawnNetworkManager mapSpawnNetworkManager = new MapSpawnNetworkManager();

// Central network address
string centralNetworkAddress;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress))
{
    mapSpawnNetworkManager.clusterServerAddress = centralNetworkAddress;
}
serverConfig[ProcessArguments.CONFIG_CENTRAL_ADDRESS] = centralNetworkAddress;

// Central network port
int centralNetworkPort;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort))
{
    centralNetworkManager.networkPort = centralNetworkPort;
}
serverConfig[ProcessArguments.CONFIG_CENTRAL_PORT] = centralNetworkPort;

// Central max connections
int centralMaxConnections;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections))
{
    centralNetworkManager.maxConnections = centralMaxConnections;
}
serverConfig[ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS] = centralMaxConnections;

// Central network port
int clusterNetworkPort;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort))
{
    centralNetworkManager.clusterServerPort = clusterNetworkPort;
    mapSpawnNetworkManager.clusterServerPort = clusterNetworkPort;
}
serverConfig[ProcessArguments.CONFIG_CLUSTER_PORT] = clusterNetworkPort;

// Machine network address, will be set to map spawn / map / chat
string machineNetworkAddress;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress))
{
    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
}
serverConfig[ProcessArguments.CONFIG_MACHINE_ADDRESS] = machineNetworkAddress;

// Map spawn network port
int mapSpawnNetworkPort;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort))
{
    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
}
serverConfig[ProcessArguments.CONFIG_MAP_SPAWN_PORT] = mapSpawnNetworkPort;

// Map spawn exe path
string spawnExePath;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath))
{
    mapSpawnNetworkManager.exePath = spawnExePath;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_EXE_PATH] = spawnExePath;

// Map spawn in batch mode
bool notSpawnInBatchMode = mapSpawnNetworkManager.notSpawnInBatchMode = false;
if (ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE, out notSpawnInBatchMode, mapSpawnNetworkManager.notSpawnInBatchMode))
{
    mapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode;
}
else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_NOT_SPAWN_IN_BATCH_MODE))
{
    mapSpawnNetworkManager.notSpawnInBatchMode = true;
}
serverConfig[ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE] = notSpawnInBatchMode;

// Map spawn start port
int spawnStartPort;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort))
{
    mapSpawnNetworkManager.startPort = spawnStartPort;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_START_PORT] = spawnStartPort;

// Spawn maps
List<string> defaultSpawnMapIds = new List<string>();
List<string> spawnMapIds;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds))
{
    spawningMapIds = spawnMapIds;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_MAPS] = spawnMapIds;

if (!configFileFound)
{
    // Write config file
    Logging.Log("Not found server config file, creating a new one");
    if (!Directory.Exists(configFolder))
        Directory.CreateDirectory(configFolder);
    File.WriteAllText(configFilePath, JsonConvert.SerializeObject(serverConfig, Formatting.Indented));
}

// Setup process
const int targetFps = 60;
using LogicLooper looper = new LogicLooper(targetFps);
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

// Start central server
centralNetworkManager.DbServiceClient = new RestDatabaseClient();
centralNetworkManager.DataManager = new CentralServerDataManager();
centralNetworkManager.StartServer();

// Start map spawn server
mapSpawnNetworkManager.spawningMapIds = spawningMapIds;
mapSpawnNetworkManager.StartServer();

// Register a action to the looper and wait for completion.
await looper.RegisterActionAsync((in LogicLooperActionContext ctx) =>
{
    if (centralNetworkManager != null && centralNetworkManager.IsServer)
        centralNetworkManager.ProcessUpdate();

    if (mapSpawnNetworkManager != null && mapSpawnNetworkManager.IsServer)
        mapSpawnNetworkManager.ProcessUpdate();

    // Return false to stop the loop
    return true;
});

async void CurrentDomain_ProcessExit(object sender, EventArgs e)
{
    if (centralNetworkManager != null)
        centralNetworkManager.StopServer();

    if (mapSpawnNetworkManager != null)
        mapSpawnNetworkManager.StopServer();
}