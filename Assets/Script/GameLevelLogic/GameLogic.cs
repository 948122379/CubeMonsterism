using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameLogic : MonoBehaviour
{
    //暂停，这个函数里FixedUpdata不听话了
    public bool isAllPauseUpdate = false;

    public GameObject box_active;//掉落方块的预制体

    public float wight = 1.1f;//两个方块之间中心点与中心点的距离(TouchLogic/AimShadowLogic引用)
    public int countleght = 3;//x、z方向上的方块数(TouchLogic/AimShadowLogic引用)
    public int canHandleFloor = 7;//y方向上的方块数

    //元素种类
    private int type_num = 5;
    public GameObject fire_cube;
    public GameObject gold_cube;
    public GameObject soil_cube;
    public GameObject water_cube;
    public GameObject wood_cube;

    //下落方块数组(TouchLogic引用)
    public GameObject[] boxGameObject_arr;
    public int nowActiveBox_count = 0;

    //方块下落计时器
    private float timer_boxfall = 0;
    public bool needFloor = true;//(TouchLogic引用)

    //已下落方块每个格子已叠最大层数的数组(TouchLogic引用)
    public int[] hadFloorCubeFloor_arr;
    private bool  isNeedChangeBack=true;

    //暂时存储已下落的方块的位置的数组，实际上是存储boxGameObject_arr的i，用来搜索可合并的同属性方块
    public int[, ,] placeOfCube_arr;
    private ArrayList needBeEliminateCube_lis;//暂时存储需要消除的方块的数组，实际上是存储boxGameObject_arr的i，
    private ArrayList XofNeedBeEliminate_lis;//暂时存储需要消除的方块的数组的x，用于上面的方块自然下落
    private ArrayList YofNeedBeEliminate_lis;//暂时存储需要消除的方块的数组的y，用于上面的方块自然下落
    private ArrayList ZofNeedBeEliminate_lis;//暂时存储需要消除的方块的数组的z，用于上面的方块自然下落
    private bool needEliminateCube = false;//现在是否需要马上消除方块的开关

    //消除需要的方块数量
    private int minNumEliminate = 2;
    //是否需要在VictoryCondition播放放在正确位置的音效
    public bool isPlayRight = false;

    //递归的次数,防止死循环
    private int seekCount = 0;

    //连消暂停开关
    private bool isNeedConditionEliminate = false;

    //消除状态，是否一套的消除动作已经结束，防止死循环
    private string nowActive = "noEliminate";

    //失败的音乐
    public AudioClip music_gameover;

    //游戏结束(TouchLogic引用)
    public bool gameover = false;

    void Awake()
    {
        Time.timeScale = 1;

        //初始化各个数组
        boxGameObject_arr = new GameObject[countleght * countleght * canHandleFloor];
        hadFloorCubeFloor_arr = new int[countleght * countleght];
        placeOfCube_arr = new int[countleght, canHandleFloor, countleght];

        //初始化队列
        needBeEliminateCube_lis = new ArrayList();
        XofNeedBeEliminate_lis = new ArrayList();
        YofNeedBeEliminate_lis = new ArrayList();
        ZofNeedBeEliminate_lis = new ArrayList();

        //设hadFloorCubeFloor_arr初始值为-1*wight，目标下落位置是每个格子最高处的那个方块+wight,若初始值为0则都会开始落在第二层
        for (int i = 0; i < countleght * countleght; i++)
        {
            hadFloorCubeFloor_arr[i] = -1;
        }

        //设placeOfCube_arr初始值为-1，若初始值为0,与第一个记录的boxGameObject_arr的i冲突
        for (int i = 0; i < countleght; i++)
        {
            for (int j = 0; j < canHandleFloor; j++)
            {
                for (int k = 0; k < countleght; k++)
                {
                    placeOfCube_arr[i, j, k] = -1;
                }
            }
        }
    }

    void FixedUpdate()
    {
        //print(isAllPauseUpdate);
        //if (isAllPauseUpdate == false)
        //{
            if (nowActiveBox_count > 0 && boxGameObject_arr[nowActiveBox_count - 1])
            {
                StartCoroutine(UpdataHadFloorCubeFloor_arr());//更新HadFloorCubeFloor_arr
                //print(hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight) + boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight)]);
            }
            //增加下落方块，并隔一段时间下落一格
            if (gameover == false)
            {
                if (nowActiveBox_count == 0)//增加第一个方块
                {
                    isPlayRight = true;//打开放置正确位置的音效
                    playSoundEffect("SoundEffect/create_SoundEffect");
                    boxGameObject_arr[nowActiveBox_count] = Instantiate(RandomCubeProperty(), new Vector3(1 * wight, canHandleFloor * wight, 1 * wight), new Quaternion(0, 0, 0, 0)) as GameObject;
                    boxGameObject_arr[nowActiveBox_count].name = nowActiveBox_count.ToString();//set box ID name by "nowActiveBox_count"
                    boxGameObject_arr[nowActiveBox_count].layer = 9;// "1_activeBox"层

                    needFloor = true;
                    timer_boxfall = Time.time;
                    nowActiveBox_count++;
                }
                else//下落移动
                {
                    if (needFloor == true)
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > 0.1)
                        {
                            if (nowActiveBox_count >= 2)//场景中有两个或以上的方块时判断是否要落在之前的方块上面,判断是否要摞在上面
                            {
                                for (int i = 0; i < nowActiveBox_count - 1 && boxGameObject_arr[nowActiveBox_count - 1] != null && boxGameObject_arr[i] != null; i++)
                                {
                                    if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > boxGameObject_arr[i].transform.position.x - 0.1
                                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < boxGameObject_arr[i].transform.position.x + 0.1
                                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > boxGameObject_arr[i].transform.position.z - 0.1
                                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < boxGameObject_arr[i].transform.position.z + 0.1
                                        && ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < (boxGameObject_arr[i].transform.position.y + wight + 0.1)) && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > (boxGameObject_arr[i].transform.position.y + wight - 0.1)))
                                        )
                                    {
                                        //检查失败
                                        if ((boxGameObject_arr[i].transform.position.y < 5 * wight + 0.1) && (boxGameObject_arr[i].transform.position.y > 5 * wight - 0.1))
                                        {
                                            //if (Time.time - timer_boxfall >= 0.9)
                                            //{
                                            print("检查是否可消除、合并，若不能则gameover");
                                            gameover = true;
                                            //SeekCubeCombine("floorCube",boxGameObject_arr[nowActiveBox_count - 1], true, true, true, true, true, true);
                                            GameObject.Find("Canvas/Restart").transform.Rotate(new Vector3(0, -90, 0));
                                            GameObject.Find("Canvas/Play").transform.Rotate(new Vector3(0, 90, 0));
                                            GameObject.Find("Canvas/Restart _actScene").transform.Rotate(new Vector3(0, 90, 0));
                                            float overBgNeedMove=GameObject.Find("Canvas").transform.position.y;
                                            print(overBgNeedMove);
                                            GameObject.Find("Canvas/OverBackground").transform.DOMoveY(overBgNeedMove,1);
                                            GameObject.Find("MusicMiner").GetComponent<AudioSource>().clip = music_gameover;
                                            if (!GameObject.Find("MusicMiner").GetComponent<AudioSource>().isPlaying)
                                            {
                                                GameObject.Find("MusicMiner").GetComponent<AudioSource>().Play();
                                            }
                                            if (GameObject.Find("Canvas/Pause"))
                                            {
                                                GameObject.Find("Canvas/Pause").SetActive(false);
                                            }
                                            gameover = true;
                                            StopAllCoroutines();//停止所有协程
                                            //}
                                        }
                                        else//下方有障碍物
                                        {
                                            needFloor = false;
                                            break;
                                        }
                                    }
                                    else//第二个及之后的方块移动
                                    {
                                        if (Time.time - timer_boxfall >= 1)
                                        {
                                            boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(0, -1 * wight, 0);
                                            timer_boxfall = Time.time;
                                        }
                                    }
                                }
                            }
                            else//第一个下落方块移动
                            {
                                if (Time.time - timer_boxfall >= 1)
                                {
                                    boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(0, -1 * wight, 0);
                                    timer_boxfall = Time.time;
                                }
                            }
                        }
                        else//达到底部
                        {
                            needFloor = false;
                        }
                    }

                    else if (needFloor == false)
                    {
                        //矫正刚落下完毕的方块的位置
                        if (boxGameObject_arr[nowActiveBox_count - 1])
                        {
                            if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < 0.5)//落在底层
                            {
                                boxGameObject_arr[nowActiveBox_count - 1].transform.position = new Vector3(boxGameObject_arr[nowActiveBox_count - 1].transform.position.x, 0, boxGameObject_arr[nowActiveBox_count - 1].transform.position.z);
                            }
                            else//落在2层及以上
                            {
                                boxGameObject_arr[nowActiveBox_count - 1].transform.position = new Vector3(boxGameObject_arr[nowActiveBox_count - 1].transform.position.x, boxGameObject_arr[nowActiveBox_count - 1].transform.position.y, boxGameObject_arr[nowActiveBox_count - 1].transform.position.z);
                            }
                        }

                        //把刚落下的这个方块的位置记录到数组中

                        //寻找是否可以消除该方块，把tag种类传进去检查
                        if (nowActive == "noEliminate")
                        {

                            //播放落下音效
                            playSoundEffect("SoundEffect/land_SoundEffect");

                            nowActive = "underEliminate";
                            needEliminateCube = true;
                            placeOfCube_arr[(int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight), (int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.y / wight), (int)(boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight)] = nowActiveBox_count - 1;
                            StartCoroutine(SeekEliminationFall(boxGameObject_arr[nowActiveBox_count - 1]));
                        }

                        //创造下一个方块,如果已经消除完毕
                        if (nowActive == "createNext" && needEliminateCube == false)
                        {
                            isPlayRight = true;//在VictoryCondition里播放放到正确位置音效
                            //print("创造方块");
                            playSoundEffect("SoundEffect/create_SoundEffect");
                            boxGameObject_arr[nowActiveBox_count] = Instantiate(RandomCubeProperty(), new Vector3(1 * wight, canHandleFloor * wight, 1 * wight), new Quaternion(0, 0, 0, 0)) as GameObject;
                            boxGameObject_arr[nowActiveBox_count].name = nowActiveBox_count.ToString();//set box ID name by "nowActiveBox_count"
                            boxGameObject_arr[nowActiveBox_count].layer = 9;// "1_activeBox"层


                            //矫正时间、方块数量、下落开关
                            timer_boxfall = Time.time;
                            nowActiveBox_count++;
                            needFloor = true;
                            nowActive = "noEliminate";
                        }
                    }
                }
            }
        //}
    }

    //单个方块搜索删除下落汇总和方块的连消方块搜索
    IEnumerator SeekEliminationFall(GameObject cubeFloored)
    {
        //把刚落下的这个方块的位置记录到数组中，然后寻找是否可以消除该方块，把tag种类传进去检查
        //print(cubeFloored);
        if (cubeFloored && placeOfCube_arr[(int)(cubeFloored.transform.position.x / wight), (int)(cubeFloored.transform.position.y / wight), (int)(cubeFloored.transform.position.z / wight)] != -1)
        {
            needBeEliminateCube_lis.Add(placeOfCube_arr[(int)(cubeFloored.transform.position.x / wight), (int)(cubeFloored.transform.position.y / wight), (int)(cubeFloored.transform.position.z / wight)]);
            yield return StartCoroutine(SeekCubeCombine("floorCube", cubeFloored, true, true, true, true, true, true));

            //print("删除数组长度：" + needBeEliminateCube_lis.Count );
            //print("删除");
            yield return StartCoroutine(EliminationTheList());//删除搜到可消除的方块
            seekCount = 0;

            yield return StartCoroutine(EliminateCondition());//搜索连消
            yield return StartCoroutine(WaitSomeFrameAndJudgeVictory(30));//判断是否成功

            //print("连消完毕");
            needEliminateCube = false;//所有连消完毕，创造下一个方块
            nowActive = "createNext";
            //print("消除完毕");
        }
        else
        {
            yield return null;
        }
    }

    //随机新创建的方块的属性
    GameObject RandomCubeProperty()
    {
        GameObject obj;
        float type_i = Random.value * 100;
        if (type_i < 100 / type_num)
        {
            obj = fire_cube;
        }
        else if (type_i < 2 * (100 / type_num))
        {
            obj = gold_cube;
        }
        else if (type_i < 3 * (100 / type_num))
        {
            obj = soil_cube;
        }
        else if (type_i < 4 * (100 / type_num))
        {
            obj = water_cube;
        }
        else
        {
            obj = wood_cube;
        }

        return obj;
    }

    //用递归寻找连着的方块
    IEnumerator SeekCubeCombine(string cubetype,GameObject obj, bool x1, bool x2, bool y1, bool y2, bool z1, bool z2)
    {
        //设置好需要搜索的面的开关
        if (obj.transform.position.x / wight == 0)
        {
             x1 = false;
        }
        else if (obj.transform.position.x / wight == countleght - 1)
        {
             x2 = false;
        }
        if (obj.transform.position.y / wight == 0)
        {
             y1 = false;
        }
        else if (obj.transform.position.y / wight == canHandleFloor - 1)
        {
            y2 = false;
        }
        if (obj.transform.position.z / wight == 0)
        {
            z1 = false;
        }
        else if (obj.transform.position.z / wight == countleght - 1)
        {
            z2 = false;
        }
        //print("box:"+obj.name +"  "+ seekCount + "次递归" + x1 + " " + x2 + " " + y1 + " " + y2 + " " + z1 + " " + z2);
        //如果是刚下落的方块，搜索周围的方块是否有相同的进行消除
        if (cubetype=="floorCube")
        {
            
            if (seekCount <= 6 && (x1 != false || y1 != false || z1 != false || x2 != false || y2 != false || z2 != false))
            {
                //判断所有需要判断的相邻的方块是否需要加入可合并数组
                if (x1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight - 1), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight)];
                    //如果boxGameObject_arr在i位置有物体且标签相同
                    //print(seekCount +"  i:"+i+"  check 左1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 左1");
                        //如果boxGameObject_arr在i位置的物体还未加入需要消除的队列，则加入队列
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 左2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 左2");
                        }
                        //递归
                        seekCount++;
                    }
                }
                if (y1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight - 1), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 下1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 下1");
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 下2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 下2");
                        }
                        seekCount++;
                    }
                }
                if (z1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight - 1)];
                    //print(seekCount + "  i:" + i + "  check 近1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 近1");
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 近2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 近2");
                        }
                        seekCount++;
                    }
                }
                if (x2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight + 1), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 右1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 右1");
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 右2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 右2");
                        }
                        seekCount++;
                    }
                }
                if (y2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight + 1), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 上1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 上1");
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 上2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 上2");
                        }
                        seekCount++;
                    }
                }
                if (z2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight + 1)];
                    //print(seekCount + "  i:" + i + "  check 远1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        //print("began 远1");
                        bool addNeedBeEliminatelis = true;
                        for (int n = 0; n < needBeEliminateCube_lis.Count; n++)//搜索是否已在数组中
                        {
                            if ((int)needBeEliminateCube_lis[n] == i)
                            {
                                addNeedBeEliminatelis = false;
                                //print("had 远2");
                            }
                        }
                        if (addNeedBeEliminatelis)
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            //print("add 远2");
                        }
                        seekCount++;
                    }
                }
            }
        }
        //如果是需要连消搜索的剩余所有方块，搜索周围是否有相同的方块，若果有则将被搜索的方块加入消除数组
        else if (cubetype == "conditionCube")
        {
            if ((x1 != false || y1 != false || z1 != false || x2 != false || y2 != false || z2 != false))
            {
                //记录上下左右前后的方块是否为同元素
                bool left_x1 = false;
                bool down_y1 = false;
                bool near_z1 = false;
                bool right_x2 = false;
                bool up_y2 = false;
                bool far_z2 = false;

                //判断所有需要判断的相邻的方块是否为相同元素
                if (x1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight - 1), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight)];            
                    //如果boxGameObject_arr在i位置有物体且标签相同
                    //print(seekCount +"  i:"+i+"  check 左1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        left_x1 = true;
                    }
                }
                if (y1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight - 1), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 下1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        down_y1 = true;
                    }
                }
                if (z1)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight - 1)];
                    //print(seekCount + "  i:" + i + "  check 近1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        near_z1 = true;
                    }
                }
                if (x2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight + 1), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 右1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        right_x2 = true;
                    }
                }
                if (y2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight + 1), (int)(obj.transform.position.z / wight)];
                    //print(seekCount + "  i:" + i + "  check 上1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        up_y2 = true;
                    }
                }
                if (z2)
                {
                    int i = placeOfCube_arr[(int)(obj.transform.position.x / wight), (int)(obj.transform.position.y / wight), (int)(obj.transform.position.z / wight + 1)];
                    //print(seekCount + "  i:" + i + "  check 远1");
                    if (i != -1 && boxGameObject_arr[i] && boxGameObject_arr[i].tag == obj.tag)
                    {
                        far_z2 = true;
                    }
                }

                if (left_x1 != false || down_y1 != false || near_z1 != false || right_x2 != false || up_y2 != false || far_z2 != false)
                {
                    for (int i = 0; i < nowActiveBox_count - 1; i++)
                    {
                        if (obj == boxGameObject_arr[i])
                        {
                            needBeEliminateCube_lis.Add(i);
                            XofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.x);
                            YofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.y);
                            ZofNeedBeEliminate_lis.Add(boxGameObject_arr[i].transform.position.z);
                            break;
                        }
                    }
                }
            }
        }
        yield return null;
    }

    //删除搜到可消除的方块
    IEnumerator EliminationTheList()
    {
        //print(minNumEliminate);
        //print(needBeEliminateCube_lis.Count);
        if (needBeEliminateCube_lis.Count >= minNumEliminate && needEliminateCube == true)
        {
            isPlayRight = false;//因为最后落下的方块会被消除，所以不用播放其放置正确位置的音效
            //播放删除音效
            GameObject.Find("SoundEffect/eliminate_SoundEffect").transform.position = boxGameObject_arr[nowActiveBox_count - 1].transform.position;
            playSoundEffect("SoundEffect/eliminate_SoundEffect");

            for (int i = 0; i < needBeEliminateCube_lis.Count; i++)
            {
                //print(i+"hadFloorCubeFloor_arr减完的层数" + hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[(int)needBeEliminateCube_lis[i]].transform.position.x / wight) + boxGameObject_arr[(int)needBeEliminateCube_lis[i]].transform.position.z / wight)]);
                //销毁需要删除的列表中的方块
                if (boxGameObject_arr[(int)needBeEliminateCube_lis[i]] != null)
                {
                    placeOfCube_arr[(int)(boxGameObject_arr[(int)needBeEliminateCube_lis[i]].transform.position.x / wight), (int)(boxGameObject_arr[(int)needBeEliminateCube_lis[i]].transform.position.y / wight), (int)(boxGameObject_arr[(int)needBeEliminateCube_lis[i]].transform.position.z / wight)] = -1;
                    GameObject.Find(boxGameObject_arr[(int)needBeEliminateCube_lis[i]].name).GetComponent<CloneCube>().enabled = true;
                    //Destroy(GameObject.Find(boxGameObject_arr[(int)needBeEliminateCube_lis[i]].name));
                    boxGameObject_arr[(int)needBeEliminateCube_lis[i]] = null;
                }
            }

            //补上boxGameObject_arr的空位
            yield return StartCoroutine(PadTheBoxArryBlank());

            //计数减3
            nowActiveBox_count -= needBeEliminateCube_lis.Count;
            yield return StartCoroutine(FloorAfterElimination());
            //needEliminateCube = true;
            print("allOver");
        }
        needBeEliminateCube_lis.Clear();
        XofNeedBeEliminate_lis.Clear();
        YofNeedBeEliminate_lis.Clear();
        ZofNeedBeEliminate_lis.Clear();
        //print("ClearList");
        yield return null;
    }

    //补上boxGameObject_arr的空位
    IEnumerator PadTheBoxArryBlank()
    {
        for (int i = 0; i < nowActiveBox_count + needBeEliminateCube_lis.Count && i < boxGameObject_arr.Length; i++)
        {
            if (boxGameObject_arr[i] == null)
            {
                for (int j = i; j < nowActiveBox_count + needBeEliminateCube_lis.Count && j < boxGameObject_arr.Length; j++)
                {
                    if (boxGameObject_arr[j] != null)
                    {
                        for (int k = i; k < nowActiveBox_count + needBeEliminateCube_lis.Count && k < boxGameObject_arr.Length; k++)
                        {
                            boxGameObject_arr[k] = boxGameObject_arr[k + j - i];
                            if (boxGameObject_arr[k + j - i] != null)
                            {
                                boxGameObject_arr[k].name = k.ToString();
                                //print(k);
                                //print((int)(boxGameObject_arr[k].transform.position.x / wight)+","+ (int)(boxGameObject_arr[k].transform.position.y / wight)+","+ (int)(boxGameObject_arr[k].transform.position.z / wight));
                                placeOfCube_arr[(int)(boxGameObject_arr[k].transform.position.x / wight), (int)(boxGameObject_arr[k].transform.position.y / wight), (int)(boxGameObject_arr[k].transform.position.z / wight)] = k;
                                //print("k:" + k +"  boxGameObject_arr[k]:" + boxGameObject_arr[k]);
                                //print("payOver");
                            }
                        }
                        break;
                    }
                }
            }
            //nowActive = "noEliminate";
        }
        yield return null;
    }

    //消除之后，若消除的是底层方块，上面的方块会落下来
    IEnumerator FloorAfterElimination()
    {
        //print("掉落");
        isNeedConditionEliminate = false;
        for (int i = 0; i < YofNeedBeEliminate_lis.Count; i++)
        {
            //用非空数组长度来计算减少计算量
            for (int j = 0; j < nowActiveBox_count; j++)
            {
                if (boxGameObject_arr[j] != null)
                {
                    isNeedConditionEliminate = true;
                    if (boxGameObject_arr[j].transform.position.x > (float)XofNeedBeEliminate_lis[i] - 0.1
                        && boxGameObject_arr[j].transform.position.x < (float)XofNeedBeEliminate_lis[i] + 0.1
                        && boxGameObject_arr[j].transform.position.z > (float)ZofNeedBeEliminate_lis[i] - 0.1
                        && boxGameObject_arr[j].transform.position.z < (float)ZofNeedBeEliminate_lis[i] + 0.1
                        && boxGameObject_arr[j].transform.position.y > (float)YofNeedBeEliminate_lis[i])
                    {
                        placeOfCube_arr[(int)(boxGameObject_arr[j].transform.position.x / wight), (int)(boxGameObject_arr[j].transform.position.y / wight), (int)(boxGameObject_arr[j].transform.position.z / wight)] = -1;
                        boxGameObject_arr[j].transform.Translate(new Vector3(0, -wight, 0));
                        placeOfCube_arr[(int)(boxGameObject_arr[j].transform.position.x / wight), (int)(boxGameObject_arr[j].transform.position.y / wight), (int)(boxGameObject_arr[j].transform.position.z / wight)] = j;

                        //print("floorOver");
                    }
                }
            }
        }
        yield return null;
    }

    //VictoryCondition脚本利用needJudgeVictory开关判断是否判断胜利
    IEnumerator WaitSomeFrameAndJudgeVictory(int frames)
    {
        GetComponent<GameVictoryCondition>().needJudgeVictory = true;
        yield return frames;
    }

    //音效播放
    void playSoundEffect(string gameObjectName)
    {
        //if (!GameObject.Find(gameObjectName).GetComponent<AudioSource>().isPlaying)
        //{
        //print(gameObjectName);
        GameObject.Find(gameObjectName).GetComponent<AudioSource>().Play();
        //}
    }

    //连消
    IEnumerator EliminateCondition()
    {
        isNeedConditionEliminate = false;
        yield return StartCoroutine(EliminateCondition2());//连消搜索
        yield return StartCoroutine(EliminationTheList());//删除搜到的需要连消的方块
        if (isNeedConditionEliminate == false)
        {
            yield return null;
        }
        else
        {
            yield return StartCoroutine(EliminateCondition());
        }
    }
    //连消搜索
    IEnumerator EliminateCondition2()
    {
        for (int i=0; i < nowActiveBox_count - 1; i++)
        {
            yield return StartCoroutine(SeekCubeCombine("conditionCube", boxGameObject_arr[i], true, true, true, true, true, true));
        }
    }

    IEnumerator UpdataHadFloorCubeFloor_arr()
    {
        //print(nowActiveBox_count - 1 + "已摞层数:" + hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight) + boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight)]); 
        isNeedChangeBack = true;
        //已下落方块每个格子已叠最大层数,countleght * countleght个格子的数据（从第2个方块出现后开始计算）
        if (nowActiveBox_count > 0 && boxGameObject_arr[nowActiveBox_count - 1] != null)
        {
            int count_i = 0;
            for (int i = 0; i < nowActiveBox_count - 1 && boxGameObject_arr[i] != null; i++)
            {
                if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > boxGameObject_arr[i].transform.position.x - 0.01
                    && boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < boxGameObject_arr[i].transform.position.x + 0.01
                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > boxGameObject_arr[i].transform.position.z - 0.01
                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < boxGameObject_arr[i].transform.position.z + 0.01)
                {
                    //print("下面的方块:"+i);
                    isNeedChangeBack = false;
                    if ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > (boxGameObject_arr[i].transform.position.y + wight - 0.01)))
                    {
                        if (count_i==0||hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[i].transform.position.x / wight) + boxGameObject_arr[i].transform.position.z / wight)] <= boxGameObject_arr[i].transform.position.y / wight)//由二维转化成一维数表示0~countleght * countleght
                        {
                            hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[i].transform.position.x / wight) + boxGameObject_arr[i].transform.position.z / wight)] = (int)(boxGameObject_arr[i].transform.position.y / wight);
                            count_i++;
                        }
                    }
                }
            }
        }
        if (isNeedChangeBack == true)
        {
            hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight) + boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight)] = -1;
        }
        yield return null;
    }

}

