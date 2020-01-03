using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExplainSceneButton : MonoBehaviour {
    private float width;
    private float moveTime;
    private int explainPageBum=4;
    private GameObject[] explainPages;
    void Start () {
        width = 1080;
        moveTime = 0.5f;
        explainPages = new GameObject[explainPageBum];
        for(int i = 0;i<explainPageBum;i++)
        {
            explainPages[i]=GameObject.Find("No"+(i+1).ToString());
        }
    }

    public void ClickNextOrLast()
    {
        string name=EventSystem.current.currentSelectedGameObject.name;
        int i;
        int.TryParse(EventSystem.current.currentSelectedGameObject.transform.parent.name.Substring(2,1),out i);
        //i是实际页数,比数组索引大1
        if(name=="Next")
        {
            explainPages[i-1].transform.DOLocalMoveX(-width, moveTime);
            explainPages[i].transform.DOLocalMoveX(0, moveTime);     
        }
        else if(name=="Last")
        {
            explainPages[i-1].transform.DOLocalMoveX(width, moveTime);
            explainPages[i-2].transform.DOLocalMoveX(0, moveTime);
        }
    }
}