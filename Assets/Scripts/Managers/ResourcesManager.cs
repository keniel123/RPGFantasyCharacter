using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ResourcesManager : MonoBehaviour
    {
        public static ResourcesManager Instance;
        Dictionary<string, int> weapon_IDs = new Dictionary<string, int>();
        Dictionary<string, int> spell_IDs = new Dictionary<string, int>();


        void Awake() {
            Instance = this;
            LoadWeaponIDs();
            LoadSpellIDs();
        }

        void LoadSpellIDs() {
            SpellItemScriptableObject obj = Resources.Load(StaticStrings.SpellScriptableObject_FileName) as SpellItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("Couldn't load spell item from: " + StaticStrings.SpellScriptableObject_FileName);

            }

            for (int i = 0; i < obj.spellItems.Count; i++)
            {
                if (spell_IDs.ContainsKey(obj.spellItems[i].itemName))
                {
                    Debug.Log("Spell Item already exists in the resource manager dictionary!");
                }
                else
                {
                    spell_IDs.Add(obj.spellItems[i].itemName, i);
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
                if (weapon_IDs.ContainsKey(weaponObj.weaponsAll[i].itemName))
                {
                    Debug.Log("Weapon Item already exists in the resource manager dictionary!");
                }
                else
                {
                    weapon_IDs.Add(weaponObj.weaponsAll[i].itemName, i);
                }
            }
        }

        int GetWeaponIDFromString(string id) {

            int index = -1;

            if (weapon_IDs.TryGetValue(id, out index))
            {
                return index;
            }

            return -1;
        }

        public Weapon GetWeapon(string weaponID) {

            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;
            if (weaponObj == null)
            {
                Debug.Log(StaticStrings.WeaponScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetWeaponIDFromString(weaponID);

            if (index == -1)
            {
                return null;
            }
            
            return weaponObj.weaponsAll[index];
          
        }

        public int GetSpellIDFromString(string spellID) {
            int index = -1;
            if (spell_IDs.TryGetValue(spellID, out index))
            {
                return index;
            }

            return index;
        }

        public Spell GetSpell(string spellID) {
            SpellItemScriptableObject obj = Resources.Load(StaticStrings.SpellScriptableObject_FileName) as SpellItemScriptableObject;
            if (obj ==null)
            {
                Debug.Log(StaticStrings.SpellScriptableObject_FileName + "cant be laoded");
                return null;
            }

            int index = GetSpellIDFromString(spellID);

            if (index == -1)
            {
                Debug.Log("Cant find spell");
                return null;
            }

            return obj.spellItems[index];
        }
    }
}