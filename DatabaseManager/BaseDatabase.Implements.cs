using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class BaseDatabase
    {
        public BaseDatabase(IDatabaseUserLogin userLoginManager)
        {
            _userLoginManager = userLoginManager;
            Initialize();
        }

        public void LogInformation(string tag, string msg)
        {
            Logging.Log(tag, msg);
        }

        public void LogWarning(string tag, string msg)
        {
            Logging.LogWarning(tag, msg);
        }

        public void LogError(string tag, string msg)
        {
            Logging.LogError(tag, msg);
        }

        public void LogException(string tag, Exception ex)
        {
            Logging.LogException(tag, ex);
        }
    }
}
