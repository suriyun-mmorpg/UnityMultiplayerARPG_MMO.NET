namespace MultiplayerARPG
{
    public partial struct CharacterItem
    {
        public MinimalItem GetItem()
        {
            if (!DataManager.Items.TryGetValue(dataId, out MinimalItem item))
                return null;
            return item;
        }

        public MinimalItem GetEquipmentItem()
        {
            if (!DataManager.Items.TryGetValue(dataId, out MinimalItem item) || !item.IsEquipment())
                return null;
            return item;
        }

        public MinimalItem GetWeaponItem()
        {
            if (!DataManager.Items.TryGetValue(dataId, out MinimalItem item) || !item.IsWeapon())
                return null;
            return item;
        }

        public MinimalItem GetPetItem()
        {
            if (!DataManager.Items.TryGetValue(dataId, out MinimalItem item) || !item.IsPet())
                return null;
            return item;
        }

        public bool IsEmpty()
        {
            return Equals(Empty);
        }

        public bool IsEmptySlot()
        {
            return IsEmpty() || GetItem() == null || amount <= 0;
        }

        public bool NotEmptySlot()
        {
            return !IsEmptySlot();
        }
    }
}
