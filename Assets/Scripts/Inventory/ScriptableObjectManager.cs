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


        //Editor extension to create inventory
        [MenuItem("Assets/Inventory/Create Inventory List")]
        public static void CreateInventory() {


        }

        //Editor extension to create spell items
        [MenuItem("Assets/Inventory/Create Spell Item List")]
        public static void CreateSpell()
        {
            CreateAsset<SpellItemScriptableObject>();

        }

        //Editor extension to weapon list for inventory
        [MenuItem("Assets/Inventory/Create Weapon List")]
        public static void CreateWeaponList()
        {

            CreateAsset<WeaponScriptableObject>();
        }
    }
}