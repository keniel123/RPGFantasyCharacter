using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace RPGController
{
    public static class ScriptableObjectManager
    {
        public static void CreateAsset<T>() where T: ScriptableObject{

            T asset = ScriptableObject.CreateInstance<T>();

            //If we dont have scriptable object with that name, create it
            if (Resources.Load(typeof(T).ToString()) == null)
            {
                string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + typeof(T).ToString() + ".asset");

                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
            //If the asset already exists
            else
            {
                Debug.Log(typeof(T).ToString() + " already created!");
            }
        }

        //Editor extension to create scriptableobject file for all Items
        [MenuItem("Assets/Inventory/Create Items Item List")]
        public static void CreateItems()
        {
            CreateAsset<ItemsScriptableObject>();
        }

        //Editor extension to create scriptableobject file for Consumables
        [MenuItem("Assets/Inventory/Create Consumable Item List")]
        public static void CreateConsumables()
        {
            CreateAsset<ConsumablesScriptableObject>();
        }

        //Editor extension to create scriptableobject file for Spells
        [MenuItem("Assets/Inventory/Create Spell Item List")]
        public static void CreateSpell()
        {
            CreateAsset<SpellItemScriptableObject>();
        }

        //Editor extension to create scriptableobject file for Weapons
        [MenuItem("Assets/Inventory/Create Weapon List")]
        public static void CreateWeaponList()
        {
            CreateAsset<WeaponScriptableObject>();
        }

        //Editor extension to create scriptableobject file for Interactions
        [MenuItem("Assets/Inventory/Create Interaction List")]
        public static void CreateInteractionList()
        {
            CreateAsset<InteractionsScriptableObject>();
        }


        //Editor extension to create scriptableobject file for NPC Dialogs
        [MenuItem("Assets/Inventory/Create NPC ScriptableObject List")]
        public static void CreateNPCDialogScriptableObject()
        {
            CreateAsset<NPCScriptableObject>();
        }
    }
}