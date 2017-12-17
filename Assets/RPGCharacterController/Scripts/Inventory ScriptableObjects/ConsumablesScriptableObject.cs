using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ConsumablesScriptableObject : ScriptableObject
    {
        public List<Consumable> consumables = new List<Consumable>();
    }
}