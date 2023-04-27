namespace MultiplayerARPG.MMO
{
    public class CentralServerDataManager : ICentralServerDataManager
    {
        public string GenerateCharacterId()
        {
            return GenericUtils.GetUniqueId();
        }

        public string GenerateMapSpawnRequestId()
        {
            return GenericUtils.GetUniqueId();
        }

        public bool CanCreateCharacter(int dataId, int entityId, int factionId)
        {
            return DataManager.CharacterCreationData.CanCreateCharacter(dataId, entityId, factionId);
        }

        public void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId)
        {
            DataManager.CharacterCreationData.SetCreateCharacterData(playerCharacterData, playerCharacterData.Id, playerCharacterData.UserId, characterName, dataId, entityId, factionId);
        }
    }
}
