using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class StartSceneButton : MonoBehaviour {

    private float moveTime;
    private Image CreatorBG;

    void Start() {
        Time.timeScale = 1;
        moveTime = 0.5f;
        CreatorBG = (GameObject.Find("GreatorBG") as GameObject).GetComponent<Image>();
    }

    public void ClickStart() {
        SceneManager.LoadScene("SelectLevel");
    }

    public void ClickCreator() {
        ControlParticleSystem(false);
        CreatorBG.transform.SetAsLastSibling();     //移动到渲染最下层
        CreatorBG.transform.DOLocalMoveX(0, moveTime);
    }

    public void ClickCreatorBack()
    {
        CreatorBG.transform.DOLocalMoveX(1197, moveTime);
        ControlParticleSystem(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void ControlParticleSystem(bool isPlay)
    {
        ParticleSystem[] particles=GameObject.FindObjectsOfType<ParticleSystem>();
        for(int i=0;i<particles.Length;i++)
        {
            if (isPlay == true) particles[i].Play();
            else particles[i].Stop();
        }
    }
}
