using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance;

        public List<string> weaponItemList = new List<string>();
        public List<string> consumableItemList = new List<string>();
        public List<string> spellItemList = new List<string>();

        void Awake() {
            Instance = this;
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
    }
}