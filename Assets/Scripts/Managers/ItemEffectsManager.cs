using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class ItemEffectsManager : MonoBehaviour
    {
        public static ItemEffectsManager Instance;

        Dictionary<string, int> effects = new Dictionary<string, int>();

        void Awake()
        {
            Instance = this;
            InitEffectsId();
        }
        
        public void InitEffectsId()
        {
            effects.Add("Bestus", 0);
            effects.Add("Bestus_Mana", 1);
            effects.Add("Souls", 2);
        }

        public void CastEffect(string effectId, StateManager states)
        {

            int i = GetIntFromId(effectId);
            if (i < 0)
            {
                Debug.Log(effectId + "effect doesn't exist!");
                return;
            }

            switch (i)
            {
                case 0: //Health
                    AddHealth(states);
                    break;
                case 1: //Mana
                    AddMana(states);
                    break;
                case 2: //Souls
                    AddSouls(states);
                    break;
                default:
                    break;
            }
        }

        #region Effects Actual
        void AddHealth(StateManager states) {

            states.characterStats.currentHealth += states.characterStats.healthRecoverValue;
        }

        void AddMana(StateManager states)
        {
            states.characterStats.currentMana += states.characterStats.manaRecoverValue;
        }

        void AddSouls(StateManager states)
        {
            states.characterStats.currentSouls += 100;

        }
        #endregion

        int GetIntFromId(string id)
        {
            int index = -1;
            if (effects.TryGetValue(id, out index))
            {
                return index;
            }

            return index;
        }
    }
}