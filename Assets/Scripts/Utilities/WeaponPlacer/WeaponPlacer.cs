using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    [ExecuteInEditMode]
    public class WeaponPlacer : MonoBehaviour
    {
        public string itemID;
        public GameObject targetModel;

        public bool leftHand;
        public bool saveItem;
        public ItemType saveType;

        public enum ItemType
        {
            Weapon, Consumable
        }

        // Update is called once per frame
        void Update()
        {
            if (!saveItem)
            {
                return;
            }

            saveItem = false;

            switch (saveType)
            {
                case ItemType.Weapon:
                    SaveWeapon();
                    break;
                case ItemType.Consumable:
                    SaveConsumable();
                    break;
                default:
                    break;
            }
        }

        public void SaveWeapon()
        {
            if (targetModel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(itemID))
            {
                return;
            }

            WeaponScriptableObject weaponObj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;

            if (weaponObj == null)
            {
                return;
            }

            for (int i = 0; i < weaponObj.weaponsAll.Count; i++)
            {
                //Check weapon id
                if (weaponObj.weaponsAll[i].itemName == itemID)
                {
                    Weapon w = weaponObj.weaponsAll[i];

                    //Save either left or right hand weapon transforms
                    if (leftHand)
                    {
                        w.left_model_eulerRot = targetModel.transform.localEulerAngles;
                        w.left_model_pos = targetModel.transform.localPosition;
                        w.left_model_scale = targetModel.transform.localScale;
                    }
                    else
                    {
                        w.right_model_eulerRot = targetModel.transform.localEulerAngles;
                        w.right_model_pos = targetModel.transform.localPosition;
                        w.right_model_scale = targetModel.transform.localScale;
                    }
                    return;
                }
            }
        }

        public void SaveConsumable()
        {
            if (targetModel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(itemID))
            {
                return;
            }

            ConsumablesScriptableObject consumableObj = Resources.Load(StaticStrings.ConsumableScriptableObject_FileName) as ConsumablesScriptableObject;

            if (consumableObj == null)
            {
                return;
            }

            for (int i = 0; i < consumableObj.consumables.Count; i++)
            {
                //Check weapon id
                if (consumableObj.consumables[i].itemName == itemID)
                {
                    Consumable w = consumableObj.consumables[i];

                    w.model_eulerRot = targetModel.transform.localEulerAngles;
                    w.model_pos = targetModel.transform.localPosition;
                    w.model_scale = targetModel.transform.localScale;
                    Debug.Log(w.itemName + " saved!");
                    return;
                }
            }
        }

    }
}