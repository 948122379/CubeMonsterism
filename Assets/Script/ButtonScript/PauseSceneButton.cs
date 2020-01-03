using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSceneButton: MonoBehaviour {
    public bool isAllPauseUpdate;
    private bool isPauseing=false;
	// Use this for initialization
    private GameObject LevelRecordCreater;
    private GameObject Slider;
	void Awake () {
        isPauseing = false;
        //Time.timeScale = 1;
        isAllPauseUpdate=false;

        LevelRecordCreater=GameObject.Find("LevelRecordCreater");
        Slider=GameObject.Find("Canvas/Slider");
        if(LevelRecordCreater&&Slider)
        {
            Slider.GetComponent<Slider>().value=LevelRecordCreater.GetComponent<levelUpdata>().voiceVelue;
        }
	}
    public void OnValueChanged(float i)
    {
        if (LevelRecordCreater&&Slider)
        {
            LevelRecordCreater.GetComponent<levelUpdata>().voiceVelue = Slider.GetComponent<Slider>().value;
        }
    }
    public void BackToStart()
    {
        SceneManager.LoadScene("Start"); //回到开始界面
    }
    public void isPauseOrGoOn()
    {
        if (isPauseing==false)
        {
            isPauseing = true;
            PauseGame();
        }
        else if (isPauseing == true)
        {
            isPauseing = false;
            StartGameFromPause();
        }
    }

    //Time.realtimeSinceStartup设置不受影响的物体
    void PauseGame(){
        Time.timeScale = 0;//暂停FixUpdata
        print("暂停");
        isAllPauseUpdate = true;//暂停Updata
        GetComponent<GameTouchLogic>().isAllPauseUpdate = GetComponent<GameTipsLogic>().isAllPauseUpdate = GetComponent<GameVictoryCondition>().isAllPauseUpdate = isAllPauseUpdate;
        SceneManager.LoadScene("Pause", LoadSceneMode.Additive); //增加暂停场景
    }
    void StartGameFromPause()
    {
        print("继续");
        Time.timeScale = 1;//继续FixUpdata
        isAllPauseUpdate = false;//继续Updata
        GetComponent<GameTouchLogic>().isAllPauseUpdate = GetComponent<GameTipsLogic>().isAllPauseUpdate = GetComponent<GameVictoryCondition>().isAllPauseUpdate = isAllPauseUpdate;
        SceneManager.UnloadScene("Pause");
    }
}
