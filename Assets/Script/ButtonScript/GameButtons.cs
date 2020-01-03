using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

//关卡内的按钮功能集合,把按钮功能整合放在Canvas上,方便预制体内按钮的函数不丢失
public class GameButtons : MonoBehaviour
{
    private GameObject MainCamera;
    private PauseSceneButton PauseSceneButton;
    void Start()
    {
        MainCamera=GameObject.Find("Camera Axis/Main Camera");
        PauseSceneButton=MainCamera.GetComponent<PauseSceneButton>();
    }
    public void BackToStart()
    {
        SceneManager.LoadScene("Start"); //回到开始界面
    }
    public void isPauseOrGoOn()
    {
        PauseSceneButton.isPauseOrGoOn();
    }
    public void ClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void NextLevel()
    {
        int i;
        int.TryParse(SceneManager.GetActiveScene().name.Substring(5,1),out i);
        print("当前Level是第"+i.ToString());
        if (i>=1&&i<=2)
        {
            print("NextLevel:Level"+i.ToString());
            GameObject.Find("LevelRecordCreater").GetComponent<levelUpdata>().level = i+1;
        }
        SceneManager.LoadScene("LoadingScene");
    }
}
