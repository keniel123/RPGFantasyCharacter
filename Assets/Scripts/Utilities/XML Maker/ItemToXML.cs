using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGController;
using System.IO;
using UnityEditor;

namespace RPGController.Utilities
{
    public static class ItemToXML
    {

        [MenuItem("Assets/Inventory/Backup/Make Weapons Database Backup")]
        public static void CreateInventory()
        {
            MakeXMLFromWeaponItems();
        }

        static void MakeXMLFromWeaponItems()
        {
       
            WeaponScriptableObject obj = Resources.Load("SA.WeaponScriptableObject") as WeaponScriptableObject;

            if(obj == null)
            {
                Debug.Log("No WeaponScriptableObject found! Aborting operation");
                return;
            }

            string xml = "<?xml version = \"1.0\" encoding = \"UTF-8\" ?>"; 
            xml += "\b";
            xml += "<root>";

            foreach (Weapon w in obj.weaponsAll)
            {      
                xml += "<weapon>" + "\n";

                xml += "<itemName>" + w.itemName + "</itemName>" + "\n";
                xml += "<itemDescription>" + w.itemDescription + "</itemDescription>" + "\n";
                xml += "<oh_idle>" + w.oh_idle + "</oh_idle>" + "\n";
                    xml += "<th_idle>" + w.th_idle + "</th_idle>" + "\n";

                    xml += ActionListToString(w.actions, "actions");
                    xml += ActionListToString(w.twoHandedActions, "two_handed");

                    xml += "<parryMultiplier>" + w.parryMultiplier + "</parryMultiplier>" + "\n";
                    xml += "<backstabMultiplier>" + w.backstabMultiplier + "</backstabMultiplier>" + "\n";
                    xml += "<LeftHandMirror>" + w.LeftHandMirror + "</LeftHandMirror>" + "\n";

                //xml += VectorToXml(w.r_model_pos, "rmp");
                //xml += VectorToXml(w.r_model_eulers, "rme");
                //xml += VectorToXml(w.l_model_pos, "lmp");
                //xml += VectorToXml(w.l_model_eulers, "lme");
                //xml += VectorToXml(w.model_scale, "ms");

                xml += "</weapon>" + "\n";

            }

            xml += "</root>";

            string path = null;
            //string path = StaticStrings.SaveLocation() + StaticStrings.itemFolder;
            
            path += "weapons_database.xml";

            File.WriteAllText(path, xml);
            Debug.Log("weapons_database.xml created!");
        }

        static string VectorToXml(Vector3 v, string nodePrefix)
        {
            string xml = null;

            xml = "<" + nodePrefix + "_x>" + v.x + "</" + nodePrefix + "_x>" + "\n";
            xml += "<" + nodePrefix + "_y>" + v.y + "</" + nodePrefix + "_y>" + "\n";
            xml += "<" + nodePrefix + "_z>" + v.z + "</" + nodePrefix + "_z>" + "\n";

            return xml;
        }


        static string ActionListToString(List<Action> l, string nodeName)
        {
            string xml = null;

            foreach (Action a in l)
            {
                xml += "<" + nodeName + ">" + "\n";
                xml += "<ActionInput>" + a.input.ToString() + "</ActionInput>" + "\n";
                xml += "<ActionType>" + a.type.ToString() + "</ActionType>" + "\n";
                xml += "<targetAnim>" + a.targetAnim + "</targetAnim>" + "\n";
                xml += "<mirror>" + a.mirror + "</mirror>" + "\n";
                xml += "<canBeParried>" + a.canBeParried + "</canBeParried>" + "\n";
                xml += "<changeSpeed>" + a.chageSpeed + "</changeSpeed>" + "\n";
                xml += "<animSpeed>" + a.animSpeed.ToString() + "</animSpeed>" + "\n";
                xml += "<canParry>" + a.canParry + "</canParry>" + "\n";
                xml += "<canBackStab>" + a.canBackStab + "</canBackStab>" + "\n";
                xml += "<ovverideDamageAnim>" + a.overrideDamageAnimation + "</ovverideDamageAnim>" + "\n";
                xml += "<damageAnim>" + a.damageAnim + "</damageAnim>" + "\n";

                WeaponStats s = a.weaponStats;

                xml += "<physical>" + s.physicalDamage + "</physical>" + "\n";
                xml += "<strike>" + s.strikeDamage + "</strike>" + "\n";
                xml += "<slash>" + s.slashDamage + "</slash>" + "\n";
                xml += "<thrust>" + s.thrustDamage + "</thrust>" + "\n";
                xml += "<magic>" + s.magicDamage + "</magic>" + "\n";
                xml += "<fire>" + s.fireDamage + "</fire>" + "\n";
                xml += "<lighting>" + s.lightningDamage + "</lighting>" + "\n";
                xml += "<dark>" + s.darkDamage + "</dark>" + "\n";

                xml += "</" + nodeName + ">" + "\n";
            }

            return xml;
   
        }
    }
}
