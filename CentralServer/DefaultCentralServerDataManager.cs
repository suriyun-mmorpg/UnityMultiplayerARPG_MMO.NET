using System.Text;

namespace MultiplayerARPG.MMO
{
    public class DefaultCentralServerDataManager : ICentralServerDataManager
    {
        public string GenerateCharacterId()
        {
            return GenericUtils.GetUniqueId();
        }

        public string GenerateMapSpawnInstanceId()
        {
            return GenericUtils.GetUniqueId();
        }

        public bool CanCreateCharacter(ref int dataId, ref int entityId, ref int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats, out UITextKeys errorMessage)
        {
            errorMessage = UITextKeys.NONE;
            return DataManager.CharacterCreationData.CanCreateCharacter(dataId, entityId, factionId, publicBools, publicInts, publicFloats);
        }

        public void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            DataManager.CharacterCreationData.SetCreateCharacterData(playerCharacterData, playerCharacterData.Id, playerCharacterData.UserId, characterName, dataId, entityId, factionId, publicBools, publicInts, publicFloats);
        }

        public string GenerateAccessToken(string userId)
        {
            string str = $"{userId}_{DateTime.Now.ToLongDateString()}";
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        public string GetUserIdFromAccessToken(string accessToken)
        {
            string str = Encoding.ASCII.GetString(System.Convert.FromBase64String(accessToken));
            string[] splitedStr = str.Split('_');
            if (splitedStr.Length > 0)
                return splitedStr[0];
            return string.Empty;
        }
    }
}
