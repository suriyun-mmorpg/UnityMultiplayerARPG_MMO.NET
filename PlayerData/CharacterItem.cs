namespace MultiplayerARPG
{
#nullable disable
    public partial class CharacterItem
    {
        [System.NonSerialized]
        private MinimalItem _cacheItem;
        [System.NonSerialized]
        private MinimalItem _cacheUsableItem;
        [System.NonSerialized]
        private MinimalItem _cacheEquipmentItem;
        [System.NonSerialized]
        private MinimalItem _cacheDefendItem;
        [System.NonSerialized]
        private MinimalItem _cacheArmorItem;
        [System.NonSerialized]
        private MinimalItem _cacheWeaponItem;
        [System.NonSerialized]
        private MinimalItem _cacheShieldItem;
        [System.NonSerialized]
        private MinimalItem _cachePotionItem;
        [System.NonSerialized]
        private MinimalItem _cacheAmmoItem;
        [System.NonSerialized]
        private MinimalItem _cacheBuildingItem;
        [System.NonSerialized]
        private MinimalItem _cachePetItem;
        [System.NonSerialized]
        private MinimalItem _cacheSocketEnhancerItem;
        [System.NonSerialized]
        private MinimalItem _cacheMountItem;
        [System.NonSerialized]
        private MinimalItem _cacheSkillItem;

        void MakeCache()
        {
            _cacheItem = null;
            _cacheUsableItem = null;
            _cacheEquipmentItem = null;
            _cacheDefendItem = null;
            _cacheArmorItem = null;
            _cacheWeaponItem = null;
            _cacheShieldItem = null;
            _cachePotionItem = null;
            _cacheAmmoItem = null;
            _cacheBuildingItem = null;
            _cachePetItem = null;
            _cacheSocketEnhancerItem = null;
            _cacheMountItem = null;
            _cacheSkillItem = null;
            if (DataManager.Items.TryGetValue(dataId, out _cacheItem) && _cacheItem != null)
            {
                if (_cacheItem.IsUsable())
                    _cacheUsableItem = _cacheItem;
                if (_cacheItem.IsEquipment())
                    _cacheEquipmentItem = _cacheItem;
                if (_cacheItem.IsDefendEquipment())
                    _cacheDefendItem = _cacheItem;
                if (_cacheItem.IsArmor())
                    _cacheArmorItem = _cacheItem;
                if (_cacheItem.IsWeapon())
                    _cacheWeaponItem = _cacheItem;
                if (_cacheItem.IsShield())
                    _cacheShieldItem = _cacheItem;
                if (_cacheItem.IsPotion())
                    _cachePotionItem = _cacheItem;
                if (_cacheItem.IsAmmo())
                    _cacheAmmoItem = _cacheItem;
                if (_cacheItem.IsBuilding())
                    _cacheBuildingItem = _cacheItem;
                if (_cacheItem.IsPet())
                    _cachePetItem = _cacheItem;
                if (_cacheItem.IsSocketEnhancer())
                    _cacheSocketEnhancerItem = _cacheItem;
                if (_cacheItem.IsMount())
                    _cacheMountItem = _cacheItem;
                if (_cacheItem.IsSkill())
                    _cacheSkillItem = _cacheItem;
            }
        }
    }
}
