using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GlobalSceneManager : MonoBehaviour
{
    public string nextScene;
    private string nowScene;
    private static GlobalSceneManager instance;
    public static GlobalSceneManager GetInstance()
    {
        if (instance == null)
        {
            instance = new GlobalSceneManager();
        }
        return instance;
    }
}
