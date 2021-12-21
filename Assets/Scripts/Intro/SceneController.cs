using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    private static SceneController sceneCtrl = null;

    public static SceneController Instance()
    {
        if(sceneCtrl == null)
        {
            sceneCtrl = GameObject.FindObjectOfType(typeof(SceneController)) as SceneController;
            Debug.Log("NEW SCENECONTROLLER");
            if(sceneCtrl == null)
            {
                sceneCtrl = new GameObject("SceneController").AddComponent<SceneController>();
            }
        }
        return sceneCtrl;
    }


    public AsyncOperation stageScene = null;

    private AsyncOperation loadingScene = null;

    private GameManager gameManager = null;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoadScene(int _sceneNum)
    {
        if(_sceneNum == 0)
        {
            StartCoroutine(LoadIntroScene());
        }
        else if(_sceneNum ==1)
        {
            StartCoroutine(LoadLobbyScene());
        }
        else if(_sceneNum ==2)
        {
            StartCoroutine(LoadStageScene());
        }
        else
        {
            LoadEndingScene();
        }
    }

    private IEnumerator LoadIntroScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadSceneAsync(0);
    }

    private IEnumerator LoadLobbyScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadSceneAsync(1);

    }
    private IEnumerator LoadStageScene()
    {
        LoadLoadingScene();
        yield return new WaitUntil(() => loadingScene.isDone);
        stageScene = SceneManager.LoadSceneAsync(1);
        LoadLoadingScene();
        yield return new WaitUntil(() => loadingScene.isDone);
        stageScene = SceneManager.LoadSceneAsync(2);
    }
    private void LoadEndingScene()
    {
        SceneManager.LoadSceneAsync(4);
    }


    private void LoadLoadingScene()
    {
        loadingScene = SceneManager.LoadSceneAsync(3);
    }

}
