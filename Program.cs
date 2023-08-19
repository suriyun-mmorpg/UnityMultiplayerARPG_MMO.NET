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

List<string> spawningMapByNames = new List<string>();
List<SpawnAllocateMapByNameData> spawningAllocateMaps = new List<SpawnAllocateMapByNameData>();

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

configFilePath = configFolder + "/socialSystemSetting.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    string dataAsJson = File.ReadAllText(configFilePath);
    SocialSystemSetting replacingConfig = JsonConvert.DeserializeObject<SocialSystemSetting>(dataAsJson);
    if (replacingConfig.GuildMemberRoles != null)
        DatabaseNetworkManager.GuildMemberRoles = replacingConfig.GuildMemberRoles;
    if (replacingConfig.GuildExpTree != null)
        DatabaseNetworkManager.GuildExpTree = replacingConfig.GuildExpTree;
}
else
{
    Logging.Log($"Can't find config file: {configFilePath}");
}

// Prepare server instances
DatabaseNetworkManager databaseNetworkManager = new DatabaseNetworkManager();
CentralNetworkManager centralNetworkManager = new CentralNetworkManager();
MapSpawnNetworkManager mapSpawnNetworkManager = new MapSpawnNetworkManager();

// Database option index
bool useCustomDatabaseClient = false;
ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT, out useCustomDatabaseClient, false);
if (ConfigReader.IsArgsProvided(args, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT))
{
    useCustomDatabaseClient = true;
}
serverConfig[ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT] = useCustomDatabaseClient;

int dbOptionIndex;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0))
{
    if (!useCustomDatabaseClient)
    {
        switch (dbOptionIndex)
        {
            case 0:
                databaseNetworkManager.Database = new SQLiteDatabase(new DefaultDatabaseUserLogin(new DefaultDatabaseUserLoginConfig()));
                break;
            case 1:
                databaseNetworkManager.Database = new MySQLDatabase(new DefaultDatabaseUserLogin(new DefaultDatabaseUserLoginConfig()));
                break;
        }
    }
}
serverConfig[ProcessArguments.CONFIG_DATABASE_OPTION_INDEX] = dbOptionIndex;

// Database disable cache reading or not?
bool disableDatabaseCaching = false;
#pragma warning disable CS0618 // Type or member is obsolete
ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DATABASE_DISABLE_CACHE_READING, out disableDatabaseCaching, false);
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DATABASE_DISABLE_CACHE_READING))
{
    disableDatabaseCaching = true;
}
serverConfig[ProcessArguments.CONFIG_DATABASE_DISABLE_CACHE_READING] = disableDatabaseCaching;
#pragma warning restore CS0618 // Type or member is obsolete
ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DISABLE_DATABASE_CACHING, out disableDatabaseCaching, false);
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DISABLE_DATABASE_CACHING))
{
    disableDatabaseCaching = true;
}
serverConfig[ProcessArguments.CONFIG_DISABLE_DATABASE_CACHING] = disableDatabaseCaching;

// Use Websocket or not?
bool useWebSocket = false;
ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_USE_WEB_SOCKET, out useWebSocket, false);
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_USE_WEB_SOCKET))
{
    useWebSocket = true;
}
serverConfig[ProcessArguments.CONFIG_USE_WEB_SOCKET] = useWebSocket;

// Is websocket running in secure mode or not?
bool webSocketSecure = false;
ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_WEB_SOCKET_SECURE, out webSocketSecure, false);
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_WEB_SOCKET_SECURE))
{
    webSocketSecure = true;
}
serverConfig[ProcessArguments.CONFIG_WEB_SOCKET_SECURE] = webSocketSecure;

// Where is the certification file path?
string webSocketCertPath;
if (!ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, string.Empty))
{
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, string.Empty);
}
serverConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH] = webSocketCertPath;

// What is the certification password?
string webSocketCertPassword;
if (!ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, string.Empty))
{
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, string.Empty);
}
serverConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD] = webSocketCertPassword;

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

// Central map spawn timeout (milliseconds)
int mapSpawnMillisecondsTimeout;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_MILLISECONDS_TIMEOUT, out mapSpawnMillisecondsTimeout, centralNetworkManager.mapSpawnMillisecondsTimeout) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_MAP_SPAWN_MILLISECONDS_TIMEOUT, out mapSpawnMillisecondsTimeout, centralNetworkManager.mapSpawnMillisecondsTimeout))
{
    centralNetworkManager.mapSpawnMillisecondsTimeout = mapSpawnMillisecondsTimeout;
}
serverConfig[ProcessArguments.CONFIG_MAP_SPAWN_MILLISECONDS_TIMEOUT] = mapSpawnMillisecondsTimeout;

// Central - default channels max connections
int defaultChannelMaxConnections;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DEFAULT_CHANNEL_MAX_CONNECTIONS, out defaultChannelMaxConnections, centralNetworkManager.defaultChannelMaxConnections) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DEFAULT_CHANNEL_MAX_CONNECTIONS, out defaultChannelMaxConnections, centralNetworkManager.defaultChannelMaxConnections))
{
    centralNetworkManager.defaultChannelMaxConnections = defaultChannelMaxConnections;
}
serverConfig[ProcessArguments.CONFIG_DEFAULT_CHANNEL_MAX_CONNECTIONS] = defaultChannelMaxConnections;

// Central - channels
List<ChannelData> channels;
if (ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_CHANNELS, out channels, centralNetworkManager.channels))
{
    centralNetworkManager.channels = channels;
}
serverConfig[ProcessArguments.CONFIG_CHANNELS] = channels;

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
if (!File.Exists(spawnExePath))
{
    spawnExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
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

// Spawn channels
List<string> spawnChannels;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_CHANNELS, out spawnChannels, mapSpawnNetworkManager.spawningChannelIds) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_CHANNELS, out spawnChannels, mapSpawnNetworkManager.spawningChannelIds))
{
    mapSpawnNetworkManager.spawningChannelIds = spawnChannels;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_CHANNELS] = spawnChannels;

// Spawn maps
List<string> defaultSpawnMapIds = new List<string>();
List<string> spawnMapIds;
if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds) ||
    ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds))
{
    spawningMapByNames = spawnMapIds;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_MAPS] = spawnMapIds;

// Spawn allocate maps
List<SpawnAllocateMapByNameData> defaultSpawnAllocateMaps = new List<SpawnAllocateMapByNameData>();
List<SpawnAllocateMapByNameData> spawnAllocateMaps;
if (ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_SPAWN_ALLOCATE_MAPS, out spawnAllocateMaps, defaultSpawnAllocateMaps))
{
    spawningAllocateMaps = spawnAllocateMaps;
}
serverConfig[ProcessArguments.CONFIG_SPAWN_ALLOCATE_MAPS] = spawnAllocateMaps;

if (!useCustomDatabaseClient)
{
    // Database network address
    string databaseNetworkAddress;
    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress) ||
        ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress))
    {
        databaseNetworkManager.networkAddress = databaseNetworkAddress;
    }
    serverConfig[ProcessArguments.CONFIG_DATABASE_ADDRESS] = databaseNetworkAddress;

    // Database network port
    int databaseNetworkPort;
    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort) ||
        ConfigReader.ReadConfigs(serverConfig, ProcessArguments.CONFIG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort))
    {
        databaseNetworkManager.networkPort = databaseNetworkPort;
    }
    serverConfig[ProcessArguments.CONFIG_DATABASE_PORT] = databaseNetworkPort;
}

if (!configFileFound)
{
    // Write config file
    Logging.Log("Not found server config file, creating a new one");
    if (!Directory.Exists(configFolder))
        Directory.CreateDirectory(configFolder);
    File.WriteAllText(configFilePath, JsonConvert.SerializeObject(serverConfig, Formatting.Indented));
}

// Read sever start args
bool startingDatabaseServer = false;
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_DATABASE_SERVER))
{
    startingDatabaseServer = true;
}
bool startingCentralServer = false;
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_CENTRAL_SERVER))
{
    startingCentralServer = true;
}
bool startingMapSpawnServer = false;
if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SPAWN_SERVER))
{
    startingMapSpawnServer = true;
}

// Setup process
const int targetFps = 60;
using LogicLooper looper = new LogicLooper(targetFps);
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

// Start database server
if (useCustomDatabaseClient)
    startingDatabaseServer = false;
if (startingDatabaseServer)
{
    databaseNetworkManager.DisableDatabaseCaching = disableDatabaseCaching;
    databaseNetworkManager.DatabaseCache = new LocalDatabaseCache();
    databaseNetworkManager.StartServer();
}

// Start central server
if (startingCentralServer)
{
    centralNetworkManager.useWebSocket = useWebSocket;
    centralNetworkManager.webSocketSecure = webSocketSecure;
    centralNetworkManager.webSocketCertificateFilePath = webSocketCertPath;
    centralNetworkManager.webSocketCertificatePassword = webSocketCertPassword;
    centralNetworkManager.DataManager = new CentralServerDataManager();
    centralNetworkManager.DbServiceClient = useCustomDatabaseClient ? new RestDatabaseClient() : databaseNetworkManager;
    if (!useCustomDatabaseClient)
        databaseNetworkManager.StartClient();
    centralNetworkManager.StartServer();
}

// Start map spawn server
if (startingMapSpawnServer)
{
    mapSpawnNetworkManager.spawningMapByNames = spawningMapByNames;
    mapSpawnNetworkManager.spawningAllocateMapByNames = spawningAllocateMaps;
    mapSpawnNetworkManager.StartServer();
}

// Register a action to the looper and wait for completion.
await looper.RegisterActionAsync((in LogicLooperActionContext ctx) =>
{
    if (databaseNetworkManager != null && databaseNetworkManager.IsServer)
        databaseNetworkManager.ProcessUpdate();

    if (centralNetworkManager != null && centralNetworkManager.IsServer)
        centralNetworkManager.ProcessUpdate();

    if (mapSpawnNetworkManager != null && mapSpawnNetworkManager.IsServer)
        mapSpawnNetworkManager.ProcessUpdate();

    // Return false to stop the loop
    return true;
});

void CurrentDomain_ProcessExit(object sender, EventArgs e)
{
    if (centralNetworkManager != null)
        centralNetworkManager.StopHost();

    if (mapSpawnNetworkManager != null)
        mapSpawnNetworkManager.StopHost();

    if (databaseNetworkManager != null)
        databaseNetworkManager.StopHost();
}