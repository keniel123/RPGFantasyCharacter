using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace RPGController
{
    public class GesturesManager : MonoBehaviour
    {
        public static GesturesManager Instance;
        public List<GestureContainer> gestures = new List<GestureContainer>();
        Dictionary<string, int> gesturesDictionary = new Dictionary<string, int>();

        public GameObject gesturesGrid;
        public GameObject gestureIconTemplate;
        public RectTransform gestureSelector;

        public int gestureIndex;
        public string currentGestureAnim;
        public bool closeWeapons;

        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < gestures.Count; i++)
            {
                if (gesturesDictionary.ContainsKey(gestures[i].targetAnim))
                {
                    Debug.Log(gestures[i].targetAnim + " is a duplicate");
                }
                else
                {
                    gesturesDictionary.Add(gestures[i].targetAnim, i);
                }
            }
        }

        private void Start()
        {
            CreateGestureUI();
        }

        public void SelectGesture(bool pos) {

            if (pos)
            {
                gestureIndex++;
            }
            else
            {
                gestureIndex--;
            }

            if (gestureIndex < 0)
            {
                gestureIndex = gestures.Count - 1;
            }

            if (gestureIndex>gestures.Count-1)
            {
                gestureIndex = 0;
            }

            if (gestures[gestureIndex] == null)
            {
                Debug.Log("No such gesture found @ index: " + gestureIndex);
            }

            IconBase iconBase = gestures[gestureIndex].iconBase;
            gestureSelector.transform.SetParent(iconBase.transform);
            gestureSelector.anchoredPosition = Vector2.zero;

            currentGestureAnim = gestures[gestureIndex].targetAnim;
            closeWeapons = gestures[gestureIndex].closeWeapons;
        }

        public void HandleGestures(bool isOpen)
        {
            if (isOpen)
            {
                Debug.Log("Closing gesture menu...");
                if (gesturesGrid.activeInHierarchy == false)
                {
                    gesturesGrid.SetActive(true);
                    gestureSelector.gameObject.SetActive(true);
                }
            }
            else
            {
                if (gesturesGrid.activeInHierarchy)
                {
                    Debug.Log("Opening gesture menu...");
                    gesturesGrid.SetActive(false);
                    gestureSelector.gameObject.SetActive(false);
                }
            }
        }

        public GestureContainer GetGesture(string id) {
            int index = -1;
            if (gesturesDictionary.TryGetValue(id, out index))
            {
                return gestures[index];
            }

            return null;
        }

        void CreateGestureUI() {
            for (int i = 0; i < gestures.Count; i++)
            {
                GameObject go = Instantiate(gestureIconTemplate) as GameObject;
                go.transform.SetParent(gesturesGrid.transform);
                go.transform.localScale = Vector3.one;
                go.SetActive(true);

                IconBase iconBase = go.GetComponent<IconBase>();
                iconBase.icon.sprite = gestures[i].iconSprite;
                gestures[i].iconBase = iconBase;
            }

            gesturesGrid.SetActive(false);
            gestureSelector.gameObject.SetActive(false);

            //By default, the first gesture in the list is selected
            gestureIndex = 1;
            SelectGesture(false);
        }
    }

    [System.Serializable]
    public class GestureContainer{
        public Sprite iconSprite;
        public string targetAnim;
        public IconBase iconBase;
        public bool closeWeapons;
    }
}