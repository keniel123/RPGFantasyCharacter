using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ItemsScriptableObject : ScriptableObject
    {
        public List<Item> consumable_Items = new List<Item>();
        public List<Item> weapon_Items = new List<Item>();
        public List<Item> spell_Items = new List<Item>();
    }
}