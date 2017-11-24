using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public enum Itemtype
    {
        Weapon,
        Spell,
        Consumable,
        Equipment
    }

    public class ResourcesManager : MonoBehaviour
    {
        public static ResourcesManager Instance;

        Dictionary<string, int> item_Spells = new Dictionary<string, int>();
        Dictionary<string, int> item_Weapons = new Dictionary<string, int>();
        Dictionary<string, int> item_Consumables = new Dictionary<string, int>();
        
        Dictionary<string, int> weapon_IDs = new Dictionary<string, int>();
        Dictionary<string, int> spell_IDs = new Dictionary<string, int>();
        Dictionary<string, int> weaponStat_IDs = new Dictionary<string, int>();
        Dictionary<string, int> consumable_IDs = new Dictionary<string, int>();
        
        void Awake() {
            Instance = this;
            LoadItems();
            LoadWeaponIDs();
            LoadSpellIDs();
            LoadConsumables();
        }

        #region Loading
        void LoadSpellIDs() {
            SpellItemScriptableObject obj = Resources.Load(StaticStrings.SpellScriptableObject_FileName) as SpellItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("Couldn't load spell item from: " + StaticStrings.SpellScriptableObject_FileName);

            }

            for (int i = 0; i < obj.spellItems.Count; i++)
            {
                if (spell_IDs.ContainsKey(obj.spellItems[i].item_id))
                {
                    Debug.Log("Spell Item already exists in the resource manager dictionary!");
                }
                else
                {
                    spell_IDs.Add(obj.spellItems[i].item_id, i);
                }
            }
        }

        void LoadWeaponIDs() {
            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;

            if (weaponObj == null)
            {
                Debug.Log("Couldn't load weapon item from: " + StaticStrings.WeaponScriptableObject_FileName);
                return;
            }

            for (int i = 0; i < weaponObj.weaponsAll.Count; i++)
            {
                if (weapon_IDs.ContainsKey(weaponObj.weaponsAll[i].item_id))
                {
                    Debug.Log("Weapon Item already exists in the resource manager dictionary!");
                }
                else
                {
                    weapon_IDs.Add(weaponObj.weaponsAll[i].item_id, i);
                }
            }

            //Load weapon stats
            for (int i = 0; i < weaponObj.weaponStats.Count; i++)
            {
                if (weaponStat_IDs.ContainsKey(weaponObj.weaponStats[i].weaponID))
                {
                    Debug.Log("Weapon stat for " + weaponObj.weaponStats[i].weaponID + "  already exists in the resource manager dictionary!");
                }
                else
                {
                    //Debug.Log("Adding: " + weaponObj.weaponStats[i].weaponID + " @ index: " + i);
                    weaponStat_IDs.Add(weaponObj.weaponStats[i].weaponID, i);
                }
            }
        }

        void LoadConsumables()
        {
            ConsumablesScriptableObject obj = Resources.Load(StaticStrings.ConsumableScriptableObject_FileName) as ConsumablesScriptableObject;

            if (obj == null)
            {
                Debug.Log("Couldn't load spell item from: " + StaticStrings.ConsumableScriptableObject_FileName);

            }

            for (int i = 0; i < obj.consumables.Count; i++)
            {
                if (consumable_IDs.ContainsKey(obj.consumables[i].item_id))
                {
                    Debug.Log("Consumable Item already exists in the resource manager dictionary!");
                }
                else
                {
                    consumable_IDs.Add(obj.consumables[i].item_id, i);
                }
            }
        }

        void LoadItems() {
            ItemsScriptableObject obj = Resources.Load(StaticStrings.ItemsScriptableObject_FileName) as ItemsScriptableObject;

            if (obj == null)
            {
                Debug.Log("Couldn't load spell item from: " + StaticStrings.ItemsScriptableObject_FileName);

            }

            //Weapon Items
            for (int i = 0; i < obj.weapon_Items.Count; i++)
            {
                if (item_Weapons.ContainsKey(obj.weapon_Items[i].item_id))
                {
                    Debug.Log("Weapon Item already exists in the resource manager dictionary!");
                }
                else
                {
                    item_Weapons.Add(obj.weapon_Items[i].item_id, i);
                }
            }

            //Spell Items
            for (int i = 0; i < obj.spell_Items.Count; i++)
            {
                if (item_Spells.ContainsKey(obj.spell_Items[i].item_id))
                {
                    Debug.Log("Spell Item already exists in the resource manager dictionary!");
                }
                else
                {
                    item_Spells.Add(obj.spell_Items[i].item_id, i);
                }
            }

            //Consumable Items
            for (int i = 0; i < obj.consumable_Items.Count; i++)
            {
                if (item_Consumables.ContainsKey(obj.consumable_Items[i].item_id))
                {
                    Debug.Log("Consumable Item already exists in the resource manager dictionary!");
                }
                else
                {
                    item_Consumables.Add(obj.consumable_Items[i].item_id, i);
                }
            }
        }
        #endregion

        #region Get Accessors
        int GetIndexFromString(Dictionary<string,int> dictionary, string id) {
            int index = -1;
            dictionary.TryGetValue(id, out index);
            return index;
        }

        public Item GetItem(string id, Itemtype itemType) {
            ItemsScriptableObject obj = Resources.Load(StaticStrings.ItemsScriptableObject_FileName) as ItemsScriptableObject;

            if (obj == null)
            {
                Debug.Log("Couldn't find the file: " + StaticStrings.ItemsScriptableObject_FileName + " under Resources!");
            }

            Dictionary<string, int> dict = null;
            List<Item> listItem = null;

            switch (itemType)
            {
                case Itemtype.Weapon:
                    dict = item_Weapons;
                    listItem = obj.weapon_Items;
                    break;
                case Itemtype.Spell:
                    dict = item_Spells;
                    listItem = obj.spell_Items;
                    break;
                case Itemtype.Consumable:
                    dict = item_Consumables;
                    listItem = obj.consumable_Items;
                    break;
                case Itemtype.Equipment:
                default:
                    return null;
            }

            if (dict==null)
            {
                return null;
            }
            if (listItem == null)
            {
                return null;
            }
            
            int index = GetIndexFromString(dict, id);
            if (index == -1)
            {
                return null;
            }

            return listItem[index];
        }

        public Weapon GetWeapon(string weaponID) {

            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;
            if (weaponObj == null)
            {
                Debug.Log(StaticStrings.WeaponScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetIndexFromString(weapon_IDs, weaponID);

            if (index == -1)
            {
                return null;
            }

            return weaponObj.weaponsAll[index];
        }

        public List<Item> GetAllItemsFromList(List<string> listOfItems, Itemtype itemType) {
            List<Item> tmp = new List<Item>();
            for (int i = 0; i < listOfItems.Count; i++)
            {
                Item item = GetItem(listOfItems[i], itemType);
                tmp.Add(item);
            }

            return tmp;
        }
        #endregion

        #region WeaponStats

        public WeaponStats GetWeaponStats(string weaponID)
        {
            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;
            if (weaponObj == null)
            {
                Debug.Log(StaticStrings.WeaponScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetIndexFromString(weaponStat_IDs, weaponID);

            if (index == -1)
            {
                return null;
            }

            return weaponObj.weaponStats[index];

        }
#endregion

        #region Spells
        public Spell GetSpell(string spellID) {
            SpellItemScriptableObject obj = Resources.Load(StaticStrings.SpellScriptableObject_FileName) as SpellItemScriptableObject;
            if (obj ==null)
            {
                Debug.Log(StaticStrings.SpellScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetIndexFromString(spell_IDs, spellID);

            if (index == -1)
            {
                Debug.Log("Cant find spell");
                return null;
            }

            return obj.spellItems[index];
        }

        #endregion

        #region Consumables

        public Consumable GetConsumable(string consumableID)
        {
            ConsumablesScriptableObject obj = Resources.Load(StaticStrings.ConsumableScriptableObject_FileName) as ConsumablesScriptableObject;
            if (obj == null)
            {
                Debug.Log(StaticStrings.ConsumableScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetIndexFromString(consumable_IDs, consumableID);

            if (index == -1)
            {
                Debug.Log("Cant find spell");
                return null;
            }

            return obj.consumables[index];
        }
        #endregion
    }

}