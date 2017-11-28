using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance;
        public InventoryUI inventoryUI;
        public ResourcesManager resourcesManager;

        [Header("Equipped Items")]
        public List<string> rightHand_Weapons_Equipped = new List<string>();
        public List<string> leftHand_Weapons_Equipped = new List<string>();
        public List<string> consumables_Equipped = new List<string>();
        public List<string> spells_Equipped = new List<string>();
        
        //Lists to store item IDs
        [HideInInspector]
        public List<int> _equipped_RightHand = new List<int>();
        [HideInInspector]
        public List<int> _equipped_LeftHand = new List<int>();
        [HideInInspector]
        public List<int> _equipped_Consumables = new List<int>();


        [Header(" In Inventory")]
        public List<string> weaponItemList = new List<string>();
        public List<string> consumableItemList = new List<string>();
        public List<string> spellItemList = new List<string>();

        int max_Weapon_Item_Index;
        int max_Consumable_Item_Index;

        public List<ItemInventoryInstance> _weapon_items = new List<ItemInventoryInstance>();
        public List<ItemInventoryInstance> _consumable_items = new List<ItemInventoryInstance>();
        ItemInventoryInstance unarmedItem;
        ItemInventoryInstance emptyItem;

        Dictionary<string, int> events_IDs = new Dictionary<string, int>();

        public List<ItemInventoryInstance> GetItemInstanceList(Itemtype itemtype) {
            switch (itemtype)
            {
                case Itemtype.Weapon:
                    return _weapon_items;
                case Itemtype.Spell:
                case Itemtype.Consumable:
                    return _consumable_items;
                    break;
                case Itemtype.Equipment:
                default:
                    return null;
            }
        }

        void InitEmptyItems() {

            unarmedItem = new ItemInventoryInstance();
            unarmedItem.itemId = "Unarmed";
            unarmedItem.uniqueId = -1;

            emptyItem = new ItemInventoryInstance();
            emptyItem.itemId = "empty";
            emptyItem.uniqueId = -1;

        }

        void Awake() {

            Instance = this;

            InitEmptyItems();

            resourcesManager.PreInit();
            inventoryUI.PreInit();
            
            for (int i = 0; i < rightHand_Weapons_Equipped.Count; i++)
            {
                weaponItemList.Add(rightHand_Weapons_Equipped[i]);
            }

            for (int i = 0; i < leftHand_Weapons_Equipped.Count; i++)
            {
                weaponItemList.Add(leftHand_Weapons_Equipped[i]);
            }

            for (int i = 0; i < consumables_Equipped.Count; i++)
            {
                consumableItemList.Add(consumables_Equipped[i]);
            }

            for (int i = 0; i < weaponItemList.Count; i++)
            {
                ItemInventoryInstance invItem = new ItemInventoryInstance();
                invItem.itemId = weaponItemList[i];
                invItem.uniqueId = max_Weapon_Item_Index;
                max_Weapon_Item_Index++;
                _weapon_items.Add(invItem);
            }

            for (int i = 0; i < rightHand_Weapons_Equipped.Count; i++)
            {
                ItemInventoryInstance it = StringToInID(_weapon_items, rightHand_Weapons_Equipped[i]);
                _equipped_RightHand.Add(it.uniqueId);
                it.slot = inventoryUI.equipmentSlotsUI.GetWeaponSlot(i);
                it.equip_Index = i;
            }

            for (int i = 0; i < leftHand_Weapons_Equipped.Count; i++)
            {
                ItemInventoryInstance it = StringToInID(_weapon_items, leftHand_Weapons_Equipped[i]);
                _equipped_LeftHand.Add(it.uniqueId);

                int targetIndex = i + 3;
                it.slot = inventoryUI.equipmentSlotsUI.GetWeaponSlot(targetIndex);
                it.equip_Index = targetIndex;
            }

            for (int i = 0; i < consumableItemList.Count; i++)
            {
                ItemInventoryInstance invItem = new ItemInventoryInstance();
                invItem.itemId = consumableItemList[i];
                invItem.uniqueId = max_Consumable_Item_Index;
                max_Consumable_Item_Index++;
                _consumable_items.Add(invItem);
            }

            for (int i = 0; i < consumables_Equipped.Count; i++)
            {
                ItemInventoryInstance it = StringToInID(_consumable_items, consumables_Equipped[i]);
                _equipped_Consumables.Add(it.uniqueId);
                it.slot = inventoryUI.equipmentSlotsUI.GetConsumableSlot(i);
                it.equip_Index = i;
            }
        }

        void AddEvents() {
            events_IDs.Add("scale", 0);
        }

        public void PlayEvent(string eventID) {
            int index = -1;
            events_IDs.TryGetValue(eventID, out index);

            switch (index)
            {
                case -1:
                    return;
                case 0:
                    Debug.Log("Doing some special shit!");

                    Transform t = InputHandler.Instance.transform;
                    Vector3 s = Vector3.one * 0.3f;
                    t.transform.localScale = s;
                    break;
                default:
                    break;
            }
        }

        public List<Item> GetItemsAsList(Itemtype type) {
            switch (type)
            {
                case Itemtype.Weapon:
                    return ResourcesManager.Instance.GetAllItemsFromList(weaponItemList, Itemtype.Weapon);
                case Itemtype.Spell:
                    return ResourcesManager.Instance.GetAllItemsFromList(spellItemList, Itemtype.Spell);
                case Itemtype.Consumable:
                    return ResourcesManager.Instance.GetAllItemsFromList(consumableItemList, Itemtype.Consumable);
                case Itemtype.Equipment:
                    break;
                default:
                    return null;
            }

            return null;
        }
        

        public ItemInventoryInstance GetWeaponItem(int uniqueId)
        {
            if (uniqueId == -1)
            {
                Debug.Log("Get unarmed");
                return unarmedItem;
            }

            return GetItem(_weapon_items, uniqueId);
    }

        public ItemInventoryInstance GetConsumableItem(int uniqueId)
        {
            if (uniqueId == -1)
            {
                return emptyItem;
            }

            return GetItem(_consumable_items, uniqueId);
        }

        public ItemInventoryInstance GetItem(List<ItemInventoryInstance> list, int uniqueId)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].uniqueId == uniqueId)
                {
                    return list[i];
                }
            }

            return null;
        }

        public ItemInventoryInstance StringToInID(List<ItemInventoryInstance> list, string s) {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].itemId, s))
                    return list[i];
            }
            
            return null;
        }
        
        private void AddArmorItem(string itemID)
        {
            throw new NotImplementedException();
        }

        public void AddItem(string itemID, Itemtype type) {
            switch (type)
            {
                case Itemtype.Weapon:
                    weaponItemList.Add(itemID);
                    ItemInventoryInstance invItem = new ItemInventoryInstance();
                    invItem.itemId = itemID;
                    invItem.uniqueId = max_Weapon_Item_Index;
                    max_Weapon_Item_Index++;
                    _weapon_items.Add(invItem);
                    break;
                case Itemtype.Spell:
                    break;
                case Itemtype.Consumable:
                    ItemInventoryInstance consumableItem = new ItemInventoryInstance();
                    consumableItem.itemId = itemID;
                    consumableItem.uniqueId = max_Consumable_Item_Index;
                    max_Consumable_Item_Index++;
                    _consumable_items.Add(consumableItem);
                    break;
                case Itemtype.Equipment:
                    AddArmorItem(itemID);
                    break;
                default:
                    break;
            }

            Item item = resourcesManager.GetItem(itemID, type);
            UIManager.Instance.AddAnnounceCard(item);
        }

    }

    [System.Serializable]
    public class ItemInventoryInstance {
        public int uniqueId;
        public int equip_Index;
        public string itemId;
        public UI.EquipmentSlot slot;
    }
}