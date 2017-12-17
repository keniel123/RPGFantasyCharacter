using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class RuntimeConsumableItem : MonoBehaviour
    {
        public bool isEmpty;
        public int itemCount = 2;
        public bool unlimitedCount;
        public Consumable Instance;
        public GameObject consumableModel;
    }
}
