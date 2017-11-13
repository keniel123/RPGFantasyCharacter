using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGController.UI
{
    public class QuickSlot : MonoBehaviour
    {
        public List<QSlots> slots = new List<QSlots>();

        public void Init() {
            //Disable all icons initially
            ClearIcons();
        }

        public void ClearIcons() {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].itemIcon.gameObject.SetActive(false);
            }
        }

        public void UpdateSlot(QSlotType slotType, Sprite iconSprite) {

            QSlots slot = GetSlot(slotType);
            slot.itemIcon.sprite = iconSprite;
            slot.itemIcon.gameObject.SetActive(true);
        }

        public QSlots GetSlot(QSlotType t) {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].type == t)
                {
                    return slots[i];
                }
            }

            return null;
        }

        public static QuickSlot Instance;
        private void Awake()
        {
            Instance = this;
        }
    }

    public enum QSlotType {
        rightHand, leftHand, item, spell
    }

    [System.Serializable]
    public class QSlots {
        public Image itemIcon;
        public QSlotType type;
    }
}