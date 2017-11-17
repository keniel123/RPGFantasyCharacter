using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class SpellEffectManager : MonoBehaviour
    {
        //Singleton reference
        public static SpellEffectManager Instance;

        //Each spell has an uniqur ID pointed by its name
        Dictionary<string, int> spellEffects = new Dictionary<string, int>();

        private void Awake()
        {
            Instance = this;

            spellEffects.Add("Firebreath", 0);
            spellEffects.Add("DarkShield", 1);
            spellEffects.Add("HealingSmall", 2);
            spellEffects.Add("Fireball", 3);
            spellEffects.Add("OnFire", 4);

        }


        public void UseSpellEffect(string spellEffectID, StateManager stateManager, EnemyStates enemy = null)
        {
            int index = GetEffect(spellEffectID);

            if (index == -1)
            {
                Debug.Log("Spell effect doesn't exist!!");
                return;
            }

            switch (index)
            {
                case 0:
                    FireBreath(stateManager);
                    break;
                case 1:
                    DarkShield(stateManager);
                    break;
                case 2:
                    HealingSmall(stateManager);
                    break;
                case 3:
                    Fireball(stateManager);
                    break;
                case 4:
                    OnFire(stateManager, enemy);
                    break;
                default:
                    break;
            }

        }

        int GetEffect(string id) {
            int index = -1;

            if (spellEffects.TryGetValue(id, out index));
            {
                return index;
            }

            return index;
        }

        void FireBreath(StateManager states) {
            states.spellCastStart = states.inventoryManager.OpenBreathCollider;
            states.spellCastLoop = states.inventoryManager.EmitSpellParticle;
            states.spellCastStop = states.inventoryManager.CloseBreathCollider;
        }

        void DarkShield(StateManager states) {
            states.spellCastStart = states.inventoryManager.OpenBlockCollider;
            states.spellCastLoop = states.inventoryManager.EmitSpellParticle;
            states.spellCastStop = states.inventoryManager.CloseBlockCollider;
        }

        void HealingSmall(StateManager states) {

            states.spellCastLoop = states.AddHealth;
        }

        void Fireball(StateManager states) {
            states.spellCastLoop = states.inventoryManager.EmitSpellParticle;
        }

        void OnFire(StateManager state, EnemyStates enemy) {
            if (state != null)
            {

            }

            if (enemy != null)
            {
                enemy.spellEffectLoop = enemy.OnFire;
            }
        }
    }
}