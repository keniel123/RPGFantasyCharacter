using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGController
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        public GesturesManager gesturesManager;

        public float lerpSpeed = 1f;
        public Slider healthSlider;
        public Slider healthSlider_Visualizer;

        public Slider manaSlider;
        public Slider manaSlider_Visualizer;

        public Slider staminaSlider;
        public Slider staminaSlider_Visualizer;

        public Text soulsTxt;
        int currentSouls;

        public float sizeMultiplier = 3;

        private void Start()
        {
            gesturesManager = GesturesManager.Instance;
        }

        public void InitSouls(int value) {

            currentSouls = value;
        }

        public void InitSlider(StatSliderType sliderType, float value)
        {
            Slider slider = null;
            Slider visualizer = null;

            switch (sliderType)
            {
                case StatSliderType.Health:
                    slider = healthSlider;
                    visualizer = healthSlider_Visualizer; 
                    break;
                case StatSliderType.Mana:
                    slider = manaSlider;
                    visualizer = manaSlider_Visualizer;
                    break;
                case StatSliderType.Stamina:
                    slider = staminaSlider;
                    visualizer = staminaSlider_Visualizer;
                    break;
                default:
                    break;
            }

            slider.maxValue = value;
            visualizer.maxValue = value;

            RectTransform target = slider.GetComponent<RectTransform>();
            RectTransform targetVisualizer = visualizer.GetComponent<RectTransform>();

            float valueActual = value * sizeMultiplier;
            valueActual = Mathf.Clamp(valueActual, 0, 1000);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, valueActual);
            targetVisualizer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, valueActual);
        }

        public void Tick(CharacterStats charStats, float delta) {
            GameUI(charStats, delta);
        }

        void GameUI(CharacterStats charStats, float delta) {

            healthSlider.value = Mathf.Lerp(healthSlider.value, charStats.currentHealth, delta * lerpSpeed * 2);
            manaSlider.value = Mathf.Lerp(manaSlider.value, charStats.currentMana, delta * lerpSpeed * 2);
            staminaSlider.value = charStats.currentStamina;
            soulsTxt.text = charStats.currentSouls.ToString();


            currentSouls = Mathf.RoundToInt(Mathf.Lerp(currentSouls, charStats.currentSouls, delta * lerpSpeed));
            soulsTxt.text = currentSouls.ToString();

            healthSlider_Visualizer.value = Mathf.Lerp(healthSlider_Visualizer.value, charStats.currentHealth, delta * lerpSpeed);
            manaSlider_Visualizer.value = Mathf.Lerp(manaSlider_Visualizer.value, charStats.currentMana, delta * lerpSpeed);
            staminaSlider_Visualizer.value = Mathf.Lerp(staminaSlider_Visualizer.value, charStats.currentStamina, delta * lerpSpeed);
        }

        public void EffectAll(int health, int mana, int stamina) {
            InitSlider(StatSliderType.Health, health);
            InitSlider(StatSliderType.Mana, mana);
            InitSlider(StatSliderType.Stamina, stamina);
        }
        
        public enum StatSliderType {

            Health, Mana, Stamina
        }

        void Awake() {
            Instance = this;
        }
    }
}