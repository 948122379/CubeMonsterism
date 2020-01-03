using UnityEngine;
using System.Collections;

public class GameTipsLogic : MonoBehaviour
{
    //暂停
    public bool isAllPauseUpdate = false;
    private GameObject pauseButton;

    private GameObject box_CreatingPlace;//出生点
    private GameObject topPlaceTips;//GameOver层提示盒子
    private GameObject FloorTips_nowFloor;//现在活动的方块所处的层数表示图片
    //目标位置方块(一层的)
    public GameObject box_aim;//底部影子目标和的预制体
    private GameObject[, ,] aimPlaneGameObject_arr;
    private int aimFloor = 1;//底部影子目标位置层数 "k" or "y"
    //下落方块底部目标位置映射相关变量(最下面一层目标位置)
    private Vector3 lastCorrespondAimPlane_ikj;
    public Texture correspondAimPlane_activeTexture;

    private float[] FloorTipsImage_y = {0,0,0,0,0,0,0};
    private Transform FloorTips;
    //调整转动摄像机的屏幕区域提示
    //在TouchLogic中

    //外部变量
    //from GameLogic//
    private float wight;//两个方块之间中心点与中心点的距离
    private int countleght;//x、z方向上的方块数
    private int canHandleFloor = 7;//y方向上的方块数
    private int nowActiveBox_count = 0;//目前方块数量nowActiveBox_count-1
    private GameObject[] boxGameObject_arr;//下落方块数组
    //VictoryCondition//
    private int level;
    private int aimMayAppearFloor;
    private bool gameover;//游戏结束（包括胜利）

    void Start()
    {
        //初始化外部数据
        wight = GetComponent<GameLogic>().wight;
        countleght = GetComponent<GameLogic>().countleght;
        canHandleFloor = GetComponent<GameLogic>().canHandleFloor;
        gameover = GetComponent<GameLogic>().gameover;
        //VictoryCondition在此脚本后面运行，因此下面在start()中数据还未初始化
        level = GetComponent<GameVictoryCondition>().level;
        aimMayAppearFloor = GetComponent<GameVictoryCondition>().aimMayAppearFloor;
        //初始化内部变量
        aimPlaneGameObject_arr = new GameObject[countleght, canHandleFloor, countleght];
        lastCorrespondAimPlane_ikj = new Vector3(1, 0, 1);
        FloorTips_nowFloor=GameObject.Find("Canvas/FloorTips/FloorTips_nowFloor");
        pauseButton=GameObject.Find("Canvas/Play");
        box_CreatingPlace=GameObject.Find("LevelsTipsObj/Cube_CreatingPlace");//出生点
        topPlaceTips=GameObject.Find("LevelsTipsObj/TopPlaceTips");

        //透明包围盒&Gameover提示层,用到未初始化的VictoryCondition中的aimMayAppearFloor，所以下面在Updata中在改变(取消碰撞器)
        topPlaceTips.transform.localScale = new Vector3(0.33f, wight, 0.33f);
        topPlaceTips.transform.position = new Vector3(wight, 6f, wight);
        //出生点
        box_CreatingPlace.transform.position = new Vector3(wight, wight * canHandleFloor, wight);

        //增加底部影子位置指示方块
        if (box_aim)
        {
            for (int i = 0; i < countleght; i++)
            {
                for (int j = 0; j < countleght; j++)
                {
                    for (int k = 0; k < aimFloor; k++)
                    {
                        aimPlaneGameObject_arr[i, k, j] = Instantiate(box_aim, new Vector3(i * wight, k * wight - wight / wight, j * wight), new Quaternion(0, 0, 0, 0)) as GameObject;//create every box of map
                        aimPlaneGameObject_arr[i, k, j].name = i.ToString() + k.ToString() + j.ToString();//set box name by "i" and "j" and "k"
                        aimPlaneGameObject_arr[i, k, j].tag = "0_aimPlane";
                        aimPlaneGameObject_arr[i, k, j].layer = 8;// "0_aimPlane"层
                    }
                }
            }
        }

        //增加层数坐标位置
        FloorTips=GameObject.Find("Canvas/FloorTips").transform;
        for(int i=0;i<7;i++)
        {
            FloorTipsImage_y[i]=GameObject.Find("Canvas/FloorTips/"+(i+1).ToString()).transform.position.y;
        } 
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isAllPauseUpdate == false && gameover == false)
        {
            //更新外部数据
            nowActiveBox_count = GetComponent<GameLogic>().nowActiveBox_count;
            boxGameObject_arr = GetComponent<GameLogic>().boxGameObject_arr;
            gameover = GetComponent<GameLogic>().gameover;

            //更新层数位置数组信息(以防变换屏幕比例)
            if(FloorTipsImage_y[0]!=GameObject.Find("Canvas/FloorTips/1").transform.position.y)
            {
                for(int i=0;i<7;i++)
                {
                    FloorTipsImage_y[i]=GameObject.Find("Canvas/FloorTips/"+(i+1).ToString()).transform.position.y;
                } 
            }
            //活动的方块所处的层数提示
            if (nowActiveBox_count > 0)
            {
                if (boxGameObject_arr[nowActiveBox_count - 1])
                {
                    int floor=(int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.y / wight);
                    if (floor < 7)
                    {
                        FloorTips_nowFloor.transform.position = new Vector3(FloorTips.position.x, FloorTipsImage_y[floor], FloorTips_nowFloor.transform.position.z);
                    }
                }
            }
            //底部阴影位置映射（更换texture）
            if (nowActiveBox_count>0){
                if (boxGameObject_arr[nowActiveBox_count - 1])
                {
                    if (nowActiveBox_count >= 1)
                    {
                        int correspondAimPlane_i = (int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight);
                        int correspondAimPlane_j = (int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight);

                        for (int i = 0; i < countleght ; i++)
                        {
                            for (int j = 0; j < countleght ; j++)
                            {
                                if (i > correspondAimPlane_i - 0.1
                                    && i < correspondAimPlane_i + 0.1
                                    && j > correspondAimPlane_j - 0.1
                                    && j < correspondAimPlane_j + 0.1)
                                {
                                    aimPlaneGameObject_arr[i, 0, j].GetComponent<Renderer>().material.mainTexture = correspondAimPlane_activeTexture;
                                }
                                else
                                {
                                    //print(i+" "+j);
                                    if (aimPlaneGameObject_arr[i, 0, j])
                                    {
                                        aimPlaneGameObject_arr[i, 0, j].GetComponent<Renderer>().material.mainTexture = null;
                                    }
                                    else
                                    {
                                        print(i+",0,"+j);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
