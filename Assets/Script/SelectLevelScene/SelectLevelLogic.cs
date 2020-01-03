using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SelectLevelLogic : MonoBehaviour {
	// Use this for initialization
	private AudioSource audio;
    private levelUpdata levelcode;
    private int LevelMax;
    private bool enteringLevel=false;
    void Start () 
    {
        audio=GameObject.Find("Canvas").GetComponent<AudioSource>();
        levelcode=GameObject.Find("LevelRecordCreater").GetComponent<levelUpdata>();
        LevelMax=levelcode.LevelMax;
	}
	
	// Update is called once per frame
    void FixedUpdate()
    {
        if(!enteringLevel)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);//划出射线，只有在scene视图中才能看到
                }

                if (hitInfo.collider)
                {
                    string mid = hitInfo.collider.gameObject.name.Substring(0, 6);
                    if (mid == "level_")
                    {
                        audio.Play();
                        enteringLevel=true;
                        int i;
                        int.TryParse(hitInfo.collider.gameObject.name.Substring(6),out i);
                        if (i>=1&&i<=LevelMax)
                        {
                            levelcode.level  = i;
                        }
                        SceneManager.LoadScene("LoadingScene");
                    }
                }
            }
        }
    }
}
