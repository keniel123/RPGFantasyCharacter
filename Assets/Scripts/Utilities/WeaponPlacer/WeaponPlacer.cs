using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    [ExecuteInEditMode]
    public class WeaponPlacer : MonoBehaviour
    {
        public string weaponID;
        public GameObject weaponModel;

        public bool leftHand;
        public bool saveWeapon;

        // Update is called once per frame
        void Update()
        {
            if (!saveWeapon)
            {
                return;
            }

            saveWeapon = false;

            if (weaponModel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(weaponID))
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
                if (weaponObj.weaponsAll[i].itemName == weaponID)
                {
                    Weapon w = weaponObj.weaponsAll[i];

                    //Save either left or right hand weapon transforms
                    if (leftHand)
                    {
                        w.left_model_eulerRot = weaponModel.transform.localEulerAngles;
                        w.left_model_pos = weaponModel.transform.localPosition;
                        w.left_model_scale = weaponModel.transform.localScale;
                    }
                    else
                    {
                        w.right_model_eulerRot = weaponModel.transform.localEulerAngles;
                        w.right_model_pos = weaponModel.transform.localPosition;
                        w.right_model_scale = weaponModel.transform.localScale;
                    }
                    return;
                }
            }

            Debug.Log(weaponID + " is NOT found in inventory!");
        }
    }
}