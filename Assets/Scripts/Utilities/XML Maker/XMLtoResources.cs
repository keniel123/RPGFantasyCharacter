using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using RPGController;
using System;
using UnityEditor;

namespace RPGController.Utilities
{
    public static class XMLtoResources
    {

        [MenuItem("Assets/Inventory/Backup/Read Weapons Database From Xml")]
        public static void ReadXmlForWeapons()
        {
            LoadWeaponData();
        }

        public static void LoadWeaponData()
        {
            string filePath = StaticStrings.SaveLocation() + StaticStrings.itemFolder;
            filePath += "weapons_database.xml";

            if(!File.Exists(filePath))
            {
                Debug.Log("weapons_database.xml doesnt exist! Aborting.");
                return;
            }

            WeaponScriptableObject obj = Resources.Load(StaticStrings.WeaponScriptableObject_FileName) as WeaponScriptableObject;

            if(obj == null)
            {
                Debug.Log("WeaponScriptableObject doesn't exist! Aborting.");
                return;
            }

            
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode w in doc.DocumentElement.SelectNodes("//weapon")) 
            {
                Weapon _w = new Weapon();
                _w.actions = new List<Action>();
                _w.twoHandedActions = new List<Action>();

                XmlNode itemName = w.SelectSingleNode("itemName");
                _w.itemName = itemName.InnerText;

                XmlNode itemDescription = w.SelectSingleNode("itemDescription");
                _w.itemDescription = itemDescription.InnerText;

                XmlNode oh_idle = w.SelectSingleNode("oh_idle");
                _w.oh_idle = oh_idle.InnerText;
                XmlNode th_idle = w.SelectSingleNode("th_idle");
                _w.th_idle = th_idle.InnerText;

                XmlNode parryMultiplier = w.SelectSingleNode("parryMultiplier"); 
                float.TryParse(parryMultiplier.InnerText, out _w.parryMultiplier);
                XmlNode backstabMultiplier = w.SelectSingleNode("backstabMultiplier");
                float.TryParse(backstabMultiplier.InnerText, out _w.backstabMultiplier);

                XmlToActions(doc, "actions", ref _w);
                XmlToActions(doc, "two_handed", ref _w);

                XmlNode LeftHandMirror = w.SelectSingleNode("LeftHandMirror");
                _w.LeftHandMirror = (LeftHandMirror.InnerText == "True");

                _w.right_model_pos = XmlToVector(w, "rmp");
                _w.right_model_eulerRot = XmlToVector(w, "rme");
                _w.left_model_pos = XmlToVector(w, "lmp");
                _w.left_model_eulerRot = XmlToVector(w, "lme");
                _w.right_model_scale = XmlToVector(w, "rms");
                _w.left_model_scale = XmlToVector(w, "lms");


                obj.weaponsAll.Add(_w);
            }
        }

        static Vector3 XmlToVector(XmlNode w, string prefix)
        {
            XmlNode _x = w.SelectSingleNode(prefix + "_x");
            float x = 0;
            float.TryParse(_x.InnerText, out x);

            XmlNode _y = w.SelectSingleNode(prefix + "_y");
            float y = 0;
            float.TryParse(_y.InnerText, out y);

            XmlNode _z = w.SelectSingleNode(prefix + "_z");
            float z = 0;
            float.TryParse(_z.InnerText, out z);

            return new Vector3(x, y, z);
        }

        static void XmlToActions(XmlDocument doc,string nodeName, ref Weapon _w)
        {
            foreach (XmlNode a in doc.DocumentElement.SelectNodes("//" + nodeName))
            {
                Action _a = new Action();

                XmlNode actionInput = a.SelectSingleNode("ActionInput");
                _a.input = (ActionInput) Enum.Parse( typeof(ActionInput),actionInput.InnerText);

                XmlNode actionType = a.SelectSingleNode("ActionType");
                _a.actionType = (ActionType)Enum.Parse(typeof(ActionType), actionType.InnerText);

                XmlNode targetAnim = a.SelectSingleNode("targetAnim");
                _a.targetAnim = targetAnim.InnerText;

                XmlNode mirror = a.SelectSingleNode("mirror");
                _a.mirror = (mirror.InnerText == "True");
                XmlNode canBeParried = a.SelectSingleNode("canBeParried");
                _a.canBeParried = (canBeParried.InnerText == "True");
                XmlNode changeSpeed = a.SelectSingleNode("changeSpeed");
                _a.chageSpeed = (changeSpeed.InnerText == "True");

                XmlNode animSpeed = a.SelectSingleNode("animSpeed");
                float.TryParse(animSpeed.InnerText, out _a.animSpeed);

                XmlNode canParry = a.SelectSingleNode("canParry");
                _a.canParry = (canParry.InnerText == "True");
                XmlNode canBackStab = a.SelectSingleNode("canBackStab");
                _a.canBackStab = (canBackStab.InnerText == "True");
                XmlNode ovverideDamageAnim = a.SelectSingleNode("ovverideDamageAnim");
                _a.overrideDamageAnimation = (ovverideDamageAnim.InnerText == "True");

                XmlNode damageAnim = a.SelectSingleNode("damageAnim");
                _a.damageAnim = damageAnim.InnerText;

                _a.weaponStats = new WeaponStats();

                XmlNode physical = a.SelectSingleNode("physical");
                int.TryParse(physical.InnerText, out _a.weaponStats.physicalDamage);
                XmlNode strike = a.SelectSingleNode("strike");
                int.TryParse(strike.InnerText, out _a.weaponStats.strikeDamage);
                XmlNode slash = a.SelectSingleNode("slash");
                int.TryParse(slash.InnerText, out _a.weaponStats.slashDamage);
                XmlNode thrust = a.SelectSingleNode("thrust");
                int.TryParse(thrust.InnerText, out _a.weaponStats.thrustDamage);

                XmlNode magic = a.SelectSingleNode("magic");
                int.TryParse(magic.InnerText, out _a.weaponStats.magicDamage);
                XmlNode fire = a.SelectSingleNode("fire");
                int.TryParse(fire.InnerText, out _a.weaponStats.fireDamage);
                XmlNode lighting = a.SelectSingleNode("lighting");
                int.TryParse(lighting.InnerText, out _a.weaponStats.lightningDamage);
                XmlNode dark = a.SelectSingleNode("dark");
                int.TryParse(dark.InnerText, out _a.weaponStats.darkDamage);

                if(nodeName == "actions")
                {
                    _w.actions.Add(_a);
                }
                else
                {
                    _w.twoHandedActions.Add(_a);
                }
                
            }
        }
    }
}
