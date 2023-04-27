using Cysharp.Threading;
using LiteNetLibManager;
using MultiplayerARPG;
using MultiplayerARPG.MMO;
using Newtonsoft.Json;

string configFolder = "./Config";
string configFilePath;
configFilePath = configFolder + "/items.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    string dataAsJson = File.ReadAllText(configFilePath);
    DataManager.Items = JsonConvert.DeserializeObject<Dictionary<int, MinimalItem>>(dataAsJson);
}
configFilePath = configFolder + "/characterCreationData.json";
if (File.Exists(configFilePath))
{
    Logging.Log($"Found config file: {configFilePath}");
    string dataAsJson = File.ReadAllText(configFilePath);
    DataManager.CharacterCreationData = JsonConvert.DeserializeObject<CharacterCreationData>(dataAsJson);
}

const int targetFps = 60;
using var looper = new LogicLooper(targetFps);

CentralNetworkManager centralNetworkManager;
centralNetworkManager = new CentralNetworkManager();
centralNetworkManager.DbServiceClient = new RestDatabaseClient();
centralNetworkManager.DataManager = new CentralServerDataManager();
centralNetworkManager.StartServer();

MapSpawnNetworkManager mapSpawnNetworkManager;
mapSpawnNetworkManager = new MapSpawnNetworkManager();
mapSpawnNetworkManager.StartServer();

// Register a action to the looper and wait for completion.
await looper.RegisterActionAsync((in LogicLooperActionContext ctx) =>
{
    if (centralNetworkManager != null)
        centralNetworkManager.ProcessUpdate();
    if (mapSpawnNetworkManager != null)
        mapSpawnNetworkManager.ProcessUpdate();
    // Return false to stop the loop
    return true;
});

if (centralNetworkManager != null)
    centralNetworkManager.StopServer();

if (mapSpawnNetworkManager != null)
    mapSpawnNetworkManager.StopServer();