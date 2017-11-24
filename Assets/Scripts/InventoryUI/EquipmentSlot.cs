using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController.UI
{
    public class EquipmentSlot : MonoBehaviour
    {
        public string slotName;
        public IconBase iconBase;
        public EquipmentSlotType eqSlotType;
        public Vector2 slotPos;
        public int itemPosition;

        public void Init(InventoryUI inventoryUI)
        {
            iconBase = GetComponent<IconBase>();
            inventoryUI.equipmentSlotsUI.AddSlotOnList(this);
        }
    }
}