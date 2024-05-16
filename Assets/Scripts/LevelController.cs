using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private static LevelController instance;
    public static LevelController Instance { get { return instance; } private set { instance = value; } }
    public int levelCount;
    public AsyncOperation ao;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        

    }

    private void OnEnable()
    {
        
    }
    private void Start()
    {
        levelCount = SceneManager.GetActiveScene().buildIndex;
        levelCount++;
        if (levelCount < SceneManager.sceneCountInBuildSettings)
        {
            //SceneSettings
            print(levelCount);

            ao = SceneManager.LoadSceneAsync(levelCount);
            ao.allowSceneActivation = false;
            print(ao.allowSceneActivation);
        }
    }

    public void ToNextLevel(bool value)
    {
        if (value)
        {
            print(value);
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel()
    {
        print("load");
        print(ao.allowSceneActivation);
        yield return new WaitForSeconds(0.1f);
        ao.allowSceneActivation = true;
    }
}
