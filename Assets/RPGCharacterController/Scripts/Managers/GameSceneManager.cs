using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGController
{
    public class GameSceneManager : MonoBehaviour
    {
        public GameScenes[] gameScenes;
        public static GameSceneManager Instance;
        public GameObject menuCamera;

        Dictionary<string, int> levelsDict = new Dictionary<string, int>();

        public string referencesScene = "References";
        public string startSceneName = "DemoLevel";

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < gameScenes.Length; i++)
            {
                if (!levelsDict.ContainsKey(gameScenes[i].primaryScene))
                {
                    levelsDict.Add(gameScenes[i].primaryScene, i);
                }
            }

            StartCoroutine(LoadScene(referencesScene, LoadSceneMode.Additive));
        }

        int StringToInt(string Id) {
            int index = -1;

            levelsDict.TryGetValue(Id, out index);

            return index;
        }

        public void PressStartGame() {

            StartCoroutine("StartGameRoutine");
        }

        IEnumerator StartGameRoutine() {

            UIManager.Instance.mainMenu.SetActive(false);
            yield return new WaitForSeconds(0.2f);

            menuCamera.SetActive(false);

            GameScenes scene = GetGameScene(startSceneName);
            scene.isLoaded = true;

            yield return LoadScene(scene.lightData, LoadSceneMode.Additive, true);
            yield return LoadScene(scene.primaryScene, LoadSceneMode.Additive, false);

            //Load forward scene if exists
            if (!string.IsNullOrEmpty(scene.forward))
            {
                yield return LoadScene(scene.forward, LoadSceneMode.Additive, false);
                GameScenes forwardScene = GetGameScene(scene.forward);
                forwardScene.isLoaded = true;
            }

            //Load backward scene if exists
            if (!string.IsNullOrEmpty(scene.backwards))
            {
                yield return LoadScene(scene.backwards, LoadSceneMode.Additive, false);
                GameScenes backwardScene = GetGameScene(scene.backwards);
                backwardScene.isLoaded = true;
            }

            yield return new WaitForSeconds(0.5f);

            SessionManager.Instance.InitGame();
        }

        IEnumerator LoadScene(string targetSceneName, LoadSceneMode mode, bool isActiveScene = false) {
            yield return SceneManager.LoadSceneAsync(targetSceneName, mode);
            if (isActiveScene)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetSceneName));
            }
        }

        public void LoadScene(string targetScene) {

            GameScenes scene = GetGameScene(targetScene);

            if (scene.isLoaded)
            {
                return;
            }

            StartCoroutine(LoadScene(targetScene, LoadSceneMode.Additive, false));
            scene.isLoaded = true;
        }

        public void UnloadScene(string targetScene) {
            GameScenes scene = GetGameScene(targetScene);
            if (scene.isLoaded)
            {
                scene.isLoaded = false;
            }
            else
            {
                return;
            }

            SceneManager.UnloadSceneAsync(targetScene);
        }

        GameScenes GetGameScene(string id) {
            int index = StringToInt(id);
            return gameScenes[index];
        }
    }

    [System.Serializable]
    public class GameScenes {

        public string primaryScene;
        public bool isLoaded;
        public string forward;
        public string backwards;
        public string lightData;
    }
}