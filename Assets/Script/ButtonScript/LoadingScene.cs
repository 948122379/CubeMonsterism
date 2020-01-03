using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadingScene : MonoBehaviour
{
    public Slider m_Slider;
    public Text m_Text;
    private int LevelMax;
    private ParticleSystem particle;
    private AsyncOperation op =null;
    private int startShowProgess=5;
    private GameObject LevelRecordCreater;
    void Awake()
    {
        Application.backgroundLoadingPriority=ThreadPriority.BelowNormal;
        print(Application.backgroundLoadingPriority);

        particle=GameObject.Find("Particle").GetComponent<ParticleSystem>();
        if(particle)
        {
            particle.Play();
        }
    }
    void Start()
    {
        SetLoadingPercentage(startShowProgess);
        
        LevelRecordCreater=GameObject.Find("LevelRecordCreater");
        if(LevelRecordCreater) 
        {
            LevelMax=LevelRecordCreater.GetComponent<levelUpdata>().LevelMax;
            StartCoroutine(loadScene());                    //该协程会导致加载卡顿
        }
        else LoadingError();
    }
    string GetLoadName()
    {
        string loadName="";
        if (LevelRecordCreater)
        {
            int nowlevel=LevelRecordCreater.GetComponent<levelUpdata>().level;
            if (nowlevel>=1&&nowlevel<=LevelMax)
            {
                loadName = "level"+nowlevel.ToString();
            }
        }
        return loadName;
    }
    IEnumerator loadScene()
    {
        yield return new WaitForEndOfFrame();               //加上这么一句就可以先显示加载画面然后再进行加载
        int displayProgress = startShowProgess;
        int toProgress = (int)(Random.value*88);            //假随机卡在某个数
        //假进度前88%
        while (displayProgress < toProgress)
        {
            SetLoadingPercentage(++displayProgress);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(3);                 //把卡顿推后,假装是进度中的,防止一开始玩家就跑了<过半理论>
        op = SceneManager.LoadSceneAsync(GetLoadName());    //这个创建还是会导致卡顿
        op.allowSceneActivation = false;
        /*while(!op.isDone)
        {
            yield return null;
        }*/
        //真进度后88%~90%
        while (op.progress < 0.9f)                          //op.allowSceneActivation = true至前,op.progress最大值为0.9
        {
            toProgress = (int)op.progress * 100;
            while (displayProgress < toProgress)
            {
                SetLoadingPercentage(++displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }
        //假进度后90%~100%
        toProgress = 100;
        while (displayProgress < toProgress)
        {
            SetLoadingPercentage(++displayProgress);
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = true;
    }
    void SetLoadingPercentage(int sliderProgress)
    {
        m_Slider.value = sliderProgress * 0.01f;
        m_Text.text = "加载资源 "+sliderProgress.ToString() + "%";
    }
    void LoadingError()
    {
        m_Text.text = "加载资源出错";
        print("缺少场景控制物体LevelRecordCreater");
    }
}
