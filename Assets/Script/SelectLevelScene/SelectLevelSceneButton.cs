using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SelectLevelSceneButton : MonoBehaviour {
    private bool openExplain = false;
    public void OpenOrCloseExplain()
    {
        if (openExplain == false)
        {
            openExplain = true;
            SceneManager.LoadScene("Explain", LoadSceneMode.Additive);
            
        }
        else if (openExplain == true)
        {
            openExplain = false;
            SceneManager.UnloadScene("Explain");
        }
	}
    public void BackStart()
    {
        SceneManager.LoadScene("Start"); //回到开始界面
    }
}
