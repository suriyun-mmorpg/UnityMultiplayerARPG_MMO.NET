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

        public bool CanCreateCharacter(int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            return DataManager.CharacterCreationData.CanCreateCharacter(dataId, entityId, factionId, publicBools, publicInts, publicFloats);
        }

        public void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            DataManager.CharacterCreationData.SetCreateCharacterData(playerCharacterData, playerCharacterData.Id, playerCharacterData.UserId, characterName, dataId, entityId, factionId, publicBools, publicInts, publicFloats);
        }
    }
}
