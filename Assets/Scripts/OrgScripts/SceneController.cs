using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private string ActualSceneName;
    private List<Scene> scenes;

    void Awake()
    {
        ActualSceneName = SceneManager.GetActiveScene().name;
    }

    public IEnumerator LoadSceneAdditive(string sceneName)
    {
        print(sceneName);
        if (SceneManager.GetSceneByName(sceneName).IsValid())
            yield break; // уже загружена

        
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = true;

        // Ждём завершения без allocation
        while (!op.isDone)
            yield return null;
    }

    public IEnumerator SetActiveSceneAdditive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        while (!scene.isLoaded)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(scene);
    }

    public IEnumerator LoadAndActivateScene(string sceneName)
    {
        // Проверяем, загружена ли уже сцена
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            Debug.Log($"Сцена {sceneName} уже загружена.");
            SceneManager.SetActiveScene(scene);
            yield break;
        }

        // Загружаем асинхронно
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = true;

        // Ждём завершения загрузки
        while (!op.isDone)
            yield return null;

        // Получаем сцену и делаем её активной
        scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
            Debug.Log($"Сцена {sceneName} активирована.");

            Debug.Log($"🔍 Ищем объект QUEST в сцене {sceneName}...");

            GameObject quest = GameObject.Find("----------QUESTwin----------");
            if (quest != null)
            {
                quest.SetActive(true);
                Debug.Log($"✅ QUEST активирован в сцене {sceneName}");
            }
            else
            {
                Debug.LogError($"❌ QUEST не найден в сцене {sceneName}");
            }
        }
        else
        {
            Debug.LogError($"Не удалось активировать сцену: {sceneName}");
        }
    }
    
    public void LoadScene(string SceneName)
    {
        StartCoroutine(LoadAndActivateScene(SceneName));
    }
}
