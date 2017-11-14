using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ResourcesManager : MonoBehaviour
    {
        public static ResourcesManager Instance;
        Dictionary<string, int> item_IDs = new Dictionary<string, int>();

        void Awake() {
            Instance = this;
            LoadItemIDs();
        }

        void LoadItemIDs() {
            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;

            for (int i = 0; i < weaponObj.weaponsAll.Count; i++)
            {
                if (item_IDs.ContainsKey(weaponObj.weaponsAll[i].itemName))
                {
                    Debug.Log("Item already exists in the resource manager dictionary!");
                }
                else
                {
                    item_IDs.Add(weaponObj.weaponsAll[i].itemName, i);
                }
            }
        }

        int GetItemIDFromString(string id) {

            int index = -1;

            if (item_IDs.TryGetValue(id, out index))
            {
                return index;
            }

            return -1;
        }

        public Weapon GetWeapon(string weaponID) {

            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;

            int index = GetItemIDFromString(weaponID);

            if (index == -1)
            {
                return null;
            }
            
            return weaponObj.weaponsAll[index];
          
        }
    }
}