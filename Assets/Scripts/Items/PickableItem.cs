using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class PickableItem : MonoBehaviour
    {
        public PickItemContainer[] items;
    }

    [System.Serializable]
    public class PickItemContainer {
        public string itemID;
        public Itemtype itemType;
    }
}