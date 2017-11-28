using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class PickableItemsManager : MonoBehaviour
    {
        public PickableItemsManager Instance;

        public List<WorldInteraction> worldInteractions = new List<WorldInteraction>();
        public List<PickableItem> pickableItems = new List<PickableItem>();
        public PickableItem itemCandidate;
        public WorldInteraction worldInterCandidate;

        int frameCount;
        public int frameCheck = 15;

        public void Tick() {
            if (frameCount <frameCheck)
            {
                frameCount++;
                return;
            }

            frameCount = 0;

            //Pickable Items
            for (int i = 0; i < pickableItems.Count; i++)
            {
                float distance = Vector3.Distance(pickableItems[i].transform.position, transform.position);

                if (distance < 2) {
                    itemCandidate = pickableItems[i];
                }
                else
                {
                    if (itemCandidate == pickableItems[i])
                    {
                        itemCandidate = null;
                    }
                }
            }


            //World Interactions
            for (int i = 0; i < worldInteractions.Count; i++)
            {
                float distance = Vector3.Distance(worldInteractions[i].transform.position, transform.position);

                if (distance < 2)
                {
                    worldInterCandidate = worldInteractions[i];
                }
                else
                {
                    if (worldInterCandidate == worldInteractions[i])
                    {
                        worldInterCandidate = null;
                    }
                }
            }
        }

        public void PickCandidate() {
            if (itemCandidate == null)
            {
                return;
            }

            SessionManager sessionManager = SessionManager.Instance;
            for (int i = 0; i < itemCandidate.items.Length; i++)
            {
                PickItemContainer container = itemCandidate.items[i];
                sessionManager.AddItem(container.itemID, container.itemType);
            }

            if (pickableItems.Contains(itemCandidate))
            {
                pickableItems.Remove(itemCandidate);
            }

            Destroy(itemCandidate.gameObject);
            itemCandidate = null;
        }
    }
    
}