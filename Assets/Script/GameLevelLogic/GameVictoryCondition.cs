using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameVictoryCondition : MonoBehaviour
{
    //暂停
    public bool isAllPauseUpdate = false;

    private ArrayList aimVictoryCondition_lis;//记录其坐标
    private ArrayList aimVictoryType_lis;//记录其种类（与aimVictoryCondition_lis相对应）
    private GameObject[] aimVictoryCondition_arr;//存储其物体
    public GameObject victoryCube;//VictoryCondition预制体
    public GameObject victoryCubeS;//VictoryConditionS需合成的预制体
    private int[] victoryCubeColor_arr = {252, 255 ,9,37,188,87,0,242,234,255,28,2,62,41,5};//记录各个种类对应的RGB颜色
    private int victoryCubeAphla = 180;
    private ArrayList excludeNumbers_lis;
    public int level;//GameTipsLogic引用
    public int aimMayAppearFloor;//GameTipsLogic引用
    private int numCondition=0;
    private int numHadCondition = 0;
    private bool keepOneceSearch = true;
    private string Type_randoming = "";
    public AudioClip music_Victory;//过关的音乐
    //需合成的目标方块种类
    public Texture treeTexture;
    private int needCubeCombineNum = 3;
    private bool hadTreeCenter = false;
    private bool centerPlaceIsOk = true;

    //外部变量from GameLogic//
    private float wight;//两个方块之间中心点与中心点的距离
    private int countleght;//x、z方向上的方块数
    private int canHandleFloor;//y方向上的方块数
    private GameObject[] boxGameObject_arr;//下落方块数组
    private int nowActiveBox_count = 0;//目前方块数量nowActiveBox_count-1
    public bool needJudgeVictory = false;//判断是否胜利
    private bool gameover;//游戏结束（包括胜利）

	void Start () {

        //外部变量初始化
        wight = GetComponent<GameLogic>().wight;
        countleght = GetComponent<GameLogic>().countleght;
        canHandleFloor = GetComponent<GameLogic>().canHandleFloor;
        boxGameObject_arr = GetComponent<GameLogic>().boxGameObject_arr;
        nowActiveBox_count = GetComponent<GameLogic>().nowActiveBox_count;
        gameover = GetComponent<GameLogic>().gameover;

        aimVictoryCondition_lis = new ArrayList();
        aimVictoryType_lis = new ArrayList();
        
        aimVictoryCondition_arr = new GameObject[countleght *canHandleFloor * countleght];

        //按设置当前关卡数和目标
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            level = 1;
        }
        else
        {
            string str1 = SceneManager.GetActiveScene().name.Substring(5);
            int.TryParse(str1, out level);
        }
        if (level < countleght * canHandleFloor * countleght)
        {
            
            numCondition = level + 2;
            if (level == 1)
            {
                numCondition = level + 3;
            }
            //print(numCondition +"="+ level+" + 2");
            /*if (level > 2)
            {
                //numCondition = level + 2;
                numCondition = 2 + 2;
            }*/
        }//else 
        //aimMayAppearFloor = numCondition + 1;
        //if (aimMayAppearFloor > canHandleFloor)
        //{
            aimMayAppearFloor = canHandleFloor;
        //}
        if (level != 0)
        {
            StartCoroutine(ExcludeNumRandom());
            for (int i = 0; i < numCondition-1; i++)
            {
                aimVictoryCondition_arr[i] = Instantiate(victoryCube, (Vector3) aimVictoryCondition_lis[i]*wight, new Quaternion(0, 0, 0, 0)) as GameObject;
                aimVictoryCondition_arr[i].name = "VictoryCondition" + i.ToString();//set box ID name by "nowActiveBox_count"
                aimVictoryCondition_arr[i].layer = 13;// "VictoryCondition"层
                StartCoroutine(ChangeColorAndTagByType(i));
            }
            //两关以上的需要合并出的新方块
            ////创建合并所需的中间方块，由于位置计算所需时间较长，所以放在Updata中
            /*if (level >= 2)
            {
                aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 1] = Instantiate(victoryCubeS, (Vector3)aimVictoryCondition_lis[aimVictoryCondition_lis.Count - 1] * wight, new Quaternion(0, 0, 0, 0)) as GameObject;
                aimVictoryCondition_arr[aimVictoryCondition_lis.Count-1].name = "VictoryConditionS" + (aimVictoryCondition_lis.Count-1).ToString();//set box ID name by "nowActiveBox_count"
                aimVictoryCondition_arr[aimVictoryCondition_lis.Count-1].layer = 13;// "VictoryCondition"层
                StartCoroutine(ChangeColorAndTagByType(aimVictoryCondition_lis.Count - 1));
            }*/
        }
	}
	
	// Update is called once per frame
    void Update()
    {
        //print(numHadCondition + ">=" + numCondition);
        if (isAllPauseUpdate == false && gameover==false)
        {
            //创建合并所需的中间方块，由于位置计算所需时间较长，所以放在Updata中
            if (level >= 2 && (!aimVictoryCondition_arr[numCondition - 1]) && aimVictoryCondition_lis.Count == numCondition)
            {
                print("createcenter");
                aimVictoryCondition_arr[numCondition - 1] = Instantiate(victoryCubeS, (Vector3)aimVictoryCondition_lis[numCondition - 1] * wight, new Quaternion(0, 0, 0, 0)) as GameObject;
                aimVictoryCondition_arr[numCondition - 1].name = "VictoryConditionS" + (numCondition - 1).ToString();//set box ID name by "nowActiveBox_count"
                aimVictoryCondition_arr[numCondition - 1].layer = 13;// "VictoryCondition"层
                StartCoroutine(ChangeColorAndTagByType(numCondition - 1));
            }
            //外部变量更新
            boxGameObject_arr = GetComponent<GameLogic>().boxGameObject_arr;
            nowActiveBox_count = GetComponent<GameLogic>().nowActiveBox_count;
            gameover = GetComponent<GameLogic>().gameover;
            //判断胜利（待测试）
            if (needJudgeVictory == true)
            {
                needJudgeVictory = false;
                numHadCondition = 0;//搜索开始前清空上次搜索的数据，保证这次的搜索
                for (int i = 0; i < aimVictoryCondition_lis.Count; i++)
                {
                    for (int j = 0; j < boxGameObject_arr.Length; j++)
                    {
                        if (boxGameObject_arr[j] != null)
                        {
                            if (boxGameObject_arr[j].transform.position.x / wight > ((Vector3)aimVictoryCondition_lis[i]).x - 0.1
                                && boxGameObject_arr[j].transform.position.x / wight < ((Vector3)aimVictoryCondition_lis[i]).x + 0.1
                                && boxGameObject_arr[j].transform.position.y / wight > ((Vector3)aimVictoryCondition_lis[i]).y - 0.1
                                && boxGameObject_arr[j].transform.position.y / wight < ((Vector3)aimVictoryCondition_lis[i]).y + 0.1
                                && boxGameObject_arr[j].transform.position.z / wight > ((Vector3)aimVictoryCondition_lis[i]).z - 0.1
                                && boxGameObject_arr[j].transform.position.z / wight < ((Vector3)aimVictoryCondition_lis[i]).z + 0.1)
                            {
                                if (aimVictoryCondition_arr[i]&&aimVictoryCondition_arr[i].tag.Substring(0, 7) == "combine")
                                {
                                    if (aimVictoryCondition_arr[i].tag == "combine_wood" && boxGameObject_arr[j].transform.tag == "wood"
                                        || aimVictoryCondition_arr[i].tag == "combine_wate" && boxGameObject_arr[j].transform.tag == "wate"
                                        || aimVictoryCondition_arr[i].tag == "combine_soil" && boxGameObject_arr[j].transform.tag == "soil")
                                    {
                                        if (j == nowActiveBox_count - 1 && GetComponent<GameLogic>().isPlayRight == true)
                                        {
                                            //播放落入正确位置的音效
                                            GameObject.Find("SoundEffect/right_SoundEffect").GetComponent<AudioSource>().Play();
                                        }
                                        //print(aimVictoryCondition_arr[i].tag + " " + boxGameObject_arr[j].transform.tag);
                                        if (!aimVictoryCondition_arr[numCondition])//防止多建一次的bug
                                        {
                                            print("creat");
                                            StartCoroutine(findArroundTwoCubePlace((Vector3)aimVictoryCondition_lis[i]));//获得剩余两个合并的目标方块的位置
                                            hadTreeCenter = true;
                                        }
                                        //print(boxGameObject_arr[j].transform.tag + " " + aimVictoryType_lis[i]);
                                        numHadCondition++;
                                        //print("numHadCondition" + numHadCondition + "j" + j);
                                    }
                                }
                                else if (boxGameObject_arr[j].transform.tag == (string)aimVictoryType_lis[i])
                                {
                                    if (j == nowActiveBox_count - 1&&GetComponent<GameLogic>().isPlayRight==true)
                                    {
                                        //播放落入正确位置的音效
                                        GameObject.Find("SoundEffect/right_SoundEffect").GetComponent<AudioSource>().Play();
                                    }
                                    //print(boxGameObject_arr[j].transform.tag + " " + aimVictoryType_lis[i]);
                                    numHadCondition++;
                                    //print("numHadCondition" + numHadCondition + "j" + j);
                                    //print(((Vector3)aimVictoryCondition_lis[i]).x + " " + ((Vector3)aimVictoryCondition_lis[i]).y + " " + ((Vector3)aimVictoryCondition_lis[i]).z);
                                }
                            }
                        }
                        if (i == aimVictoryCondition_lis.Count - 1 && j == boxGameObject_arr.Length - 1)
                        {
                            //print(numHadCondition +">="+ numCondition);
                            if (numHadCondition >= numCondition-1)
                            {
                                if (level == 1)//因为无特殊方块，所以numCondition-1个
                                {
                                    print("你已经过关");
                                    GetComponent<GameLogic>().gameover = true;//胜利 
                                    GameObject.Find("Canvas/Restart").transform.Rotate(new Vector3(0, -90, 0));
                                    GameObject.Find("Canvas/NextLevel").transform.Rotate(new Vector3(0, -90, 0));
                                    GameObject.Find("Canvas/Play").transform.Rotate(new Vector3(0, 90, 0));
                                    GameObject.Find("Canvas/Restart _actScene").transform.Rotate(new Vector3(0, 90, 0));
                                    GameObject.Find("Canvas/VictoryBackground").transform.Rotate(new Vector3(0, -90, 0));
                                    GameObject.Find("MusicMiner").GetComponent<AudioSource>().clip = music_Victory;
                                    if (!GameObject.Find("MusicMiner").GetComponent<AudioSource>().isPlaying)
                                    {
                                        GameObject.Find("MusicMiner").GetComponent<AudioSource>().Play();
                                    }
                                }
                                else if (level >= 2)
                                {
                                    if (numHadCondition >= numCondition + 2)//因为有特殊方块，所以numCondition+2个
                                    {
                                        //2关以后对合并的方块的判断
                                        print("你已经过关");
                                        GetComponent<GameLogic>().gameover = true;//胜利 
                                        GameObject.Find("Canvas/Restart").transform.Rotate(new Vector3(0, -90, 0));
                                        GameObject.Find("Canvas/NextLevel").transform.Rotate(new Vector3(0, -90, 0));
                                        GameObject.Find("Canvas/Play").transform.Rotate(new Vector3(0, 90, 0));
                                        GameObject.Find("Canvas/Restart _actScene").transform.Rotate(new Vector3(0, 90, 0));
                                        GameObject.Find("Canvas/VictoryBackground").transform.Rotate(new Vector3(0, -90, 0));
                                        GameObject.Find("MusicMiner").GetComponent<AudioSource>().clip = music_Victory;
                                        if (!GameObject.Find("MusicMiner").GetComponent<AudioSource>().isPlaying)
                                        {
                                            GameObject.Find("MusicMiner").GetComponent<AudioSource>().Play();
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

    //创建随机的不重叠位置Vector3
    IEnumerator ExcludeNumRandom()
    {
        //print(aimVictoryCondition_lis.Count +"  "+ numCondition);
        if (aimVictoryCondition_lis.Count < numCondition)
        {
            //随机种类
            StartCoroutine(TypeRandom());

            //随机位置
            int point_x = (int)(Random.value * 100 % (countleght));
            int point_z = (int)(Random.value * 100 % (countleght));
            int point_y = (int)(Random.value * 100 % (canHandleFloor - 4));

            if (aimVictoryCondition_lis.Count == 0)
            {
                addConditionPoint(point_x, point_y, point_z);
                aimVictoryType_lis.Add(Type_randoming);
            }
            else if (aimVictoryCondition_lis.Count > 0)
            {
                bool isHadTheNum = false;
                //合并中心的目标方块的位置需要特殊搜索(必须放在“遍历数组查看是否已有此数”的前面，不然会和之前numCondition - 1个的方块重叠)
                if (aimVictoryCondition_lis.Count == numCondition - 1)
                {
                    yield return StartCoroutine(judgeCubeCenterPlace(new Vector3(point_x, point_y, point_z)));
                    //print("centerPlaceIsOk"+centerPlaceIsOk);
                    isHadTheNum = !centerPlaceIsOk;
                    centerPlaceIsOk = true;
                    //print("isHadTheNum"+isHadTheNum);
                }
                //所有位置都要遍历数组查看是否已有此数
                for (int i = 0; i < aimVictoryCondition_lis.Count; i++)
                {
                    if ((Vector3)aimVictoryCondition_lis[i] == new Vector3(point_x, point_y, point_z))
                    {
                        isHadTheNum = true;
                    }
                    else if ((((Vector3)aimVictoryCondition_lis[i]).x > point_x - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).x < point_x + 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).y > point_y - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).y < point_y + 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).z > point_z - 1.2
                        && ((Vector3)aimVictoryCondition_lis[i]).z < point_z + 1.2)

                        ||( ((Vector3)aimVictoryCondition_lis[i]).x > point_x - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).x < point_x + 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).y > point_y - 1.2
                        && ((Vector3)aimVictoryCondition_lis[i]).y < point_y + 1.2
                        && ((Vector3)aimVictoryCondition_lis[i]).z > point_z - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).z < point_z + 0.1)

                        ||( ((Vector3)aimVictoryCondition_lis[i]).x > point_x - 1.2
                        && ((Vector3)aimVictoryCondition_lis[i]).x < point_x + 1.2
                        && ((Vector3)aimVictoryCondition_lis[i]).y > point_y - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).y < point_y + 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).z > point_z - 0.1
                        && ((Vector3)aimVictoryCondition_lis[i]).z < point_z + 0.1)
                        )
                    {
                        isHadTheNum = true;
                    }
                }
                

                if (isHadTheNum == false)
                {
                    addConditionPoint(point_x, point_y, point_z);
                    aimVictoryType_lis.Add(Type_randoming);
                }
            }

            yield return StartCoroutine(ExcludeNumRandom());//递归
        }
        yield return null;
    }

    //把坐标加入列表
    void addConditionPoint(int x,int y,int z)
    {
        Vector3 conditionPoint = new Vector3(x, y, z);
        aimVictoryCondition_lis.Add(conditionPoint);
        //print(conditionPoint);
    }

    //随机种类
    IEnumerator TypeRandom()
    {
        //前numCondition个目标方块随机种类
        if (aimVictoryCondition_lis.Count < numCondition-1)
        {
            int type_Condition = (int)(Random.value * 100 % 5);
            switch (type_Condition)
            {
                case 0:
                    {
                        Type_randoming = "gold";
                        //aimVictoryType_lis.Add("gold");
                        //print("gold");
                        break;
                    }
                case 1:
                    {
                        Type_randoming = "wood";
                        //aimVictoryType_lis.Add("wood");
                        //print("wood");
                        break;
                    }
                case 2:
                    {
                        Type_randoming = "wate";
                        //aimVictoryType_lis.Add("wate");
                        //print("water");
                        break;
                    }
                case 3:
                    {
                        Type_randoming = "fire";
                        //aimVictoryType_lis.Add("fire");
                        //print("fire");
                        break;
                    }
                case 4:
                    {
                        Type_randoming = "soil";
                        //aimVictoryType_lis.Add("soil");
                        //print("soil");
                        break;
                    }
            }
        }
        else
        {
            Type_randoming = "tree";
        }
        yield return null;
    }

    //通过aimVictoryType_lis记录的种类改变相应胜利条件方块的颜色
    IEnumerator ChangeColorAndTagByType(int i)
    {
        int red_RGB = 0;
        if (aimVictoryType_lis[i] == "gold")
        {
            red_RGB = 0;
            aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
            aimVictoryCondition_arr[i].tag = "aim_gold";
            //print("chage gold");
        }
        else if (aimVictoryType_lis[i] == "wood")
        {
            red_RGB = 3;
            aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
            aimVictoryCondition_arr[i].tag = "aim_wood";
            //print("chage wood");
        }
        else if (aimVictoryType_lis[i] == "wate")
        {
            red_RGB = 6;
            aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
            aimVictoryCondition_arr[i].tag = "aim_wate";
            //print("chage wate");
        }
        else if (aimVictoryType_lis[i] == "fire")
        {
            red_RGB = 9;
            aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
            aimVictoryCondition_arr[i].tag = "aim_fire";
            //print("chage fire");
        }
        else if (aimVictoryType_lis[i] == "soil")
        {
            red_RGB = 12;
            aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
            aimVictoryCondition_arr[i].tag = "aim_soil";
            //print("chage soil");
        }
        else//特殊方块附Tag
        {

                if (aimVictoryType_lis[i] == "tree")
                {
                    if (i == numCondition)
                    {
                        red_RGB = 6;
                        aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
                        aimVictoryCondition_arr[i].tag = "combine_wate";
                    }
                    else if (i == numCondition+1)
                    {
                        red_RGB = 12;
                        aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
                        aimVictoryCondition_arr[i].tag = "combine_soil";
                    }
                    else
                    {
                        red_RGB = 3;
                        aimVictoryCondition_arr[i].GetComponent<Renderer>().material.color = new Color(victoryCubeColor_arr[red_RGB] / 255f, victoryCubeColor_arr[red_RGB + 1] / 255f, victoryCubeColor_arr[red_RGB + 2] / 255f, victoryCubeAphla / 255f);
                        aimVictoryCondition_arr[i].GetComponent<Renderer>().material.SetTexture("_MainTex", treeTexture);
                        aimVictoryCondition_arr[i].tag = "combine_wood";
                    }
            }
            //print("chage tree");
        }
        yield return null;
    }

    //搜索需要合并的中间方块周围的空位方块
    IEnumerator findArroundTwoCubePlace(Vector3 centerCubePosition)
    {
        //print("centerCubePosition:"+centerCubePosition);
        bool x1 = true;
        bool x2 = true;
        bool y1 = true;
        bool y2 = true;
        bool z1 = true;
        bool z2 = true;
        //设置好需要搜索的面的开关
        if ((int)centerCubePosition.x == 0)
        {
            x1 = false;
        }
        else if ((int)centerCubePosition.x == countleght - 1)
        {
            x2 = false;
        }
        if ((int)centerCubePosition.y == 0)
        {
            y1 = false;
        }
        else if ((int)centerCubePosition.y == canHandleFloor - 1)
        {
            y2 = false;
        }
        if ((int)centerCubePosition.z == 0)
        {
            z1 = false;
        }
        else if ((int)centerCubePosition.z == countleght - 1)
        {
            z2 = false;
        }

        if (x1)
        {
            Vector3 arroundPlace= new Vector3((int)(centerCubePosition.x-1), (int)(centerCubePosition.y), (int)(centerCubePosition.z));
            addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
            aimVictoryType_lis.Add(Type_randoming);
            //print("x1" + arroundPlace);
        }

        if (y1)
        {
            Vector3 arroundPlace = new Vector3((int)(centerCubePosition.x), (int)(centerCubePosition.y-1), (int)(centerCubePosition.z));
            addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
            aimVictoryType_lis.Add(Type_randoming);
            //print("y1" + arroundPlace);
        }

        if (z1)
        {
            Vector3 arroundPlace = new Vector3((int)(centerCubePosition.x), (int)(centerCubePosition.y), (int)(centerCubePosition.z -1));
            addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
            aimVictoryType_lis.Add(Type_randoming);
            //print("z1"+arroundPlace);
        }

        if (x2)
        {
            Vector3 arroundPlace = new Vector3((int)(centerCubePosition.x+1), (int)(centerCubePosition.y), (int)(centerCubePosition.z));
            /*for (int i = 0; i < aimVictoryCondition_lis.Count; i++)//遍历数组查看是否已有此数
            {
                if ((Vector3)aimVictoryCondition_lis[i] == arroundPlace)
                {
                    isHadConditionCube = true;
                }
            }
            yield return StartCoroutine(findArroundTwoCubePlace2(arroundPlace, x2_had));
            print("x2_had:" + x2_had);
            if (x2_had == false)
            {*/
                addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
                aimVictoryType_lis.Add(Type_randoming);
                //print("x2" + arroundPlace);
            //}
        }

        if (y2)
        {
            Vector3 arroundPlace = new Vector3((int)(centerCubePosition.x), (int)(centerCubePosition.y+1), (int)(centerCubePosition.z));
            addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
            aimVictoryType_lis.Add(Type_randoming);
            //print("y2" + arroundPlace);
        }

        if (z2)
        {
            Vector3 arroundPlace = new Vector3((int)(centerCubePosition.x), (int)(centerCubePosition.y), (int)(centerCubePosition.z+1));
            addConditionPoint((int)arroundPlace.x, (int)arroundPlace.y, (int)arroundPlace.z);
            aimVictoryType_lis.Add(Type_randoming);
            //print("z2" + arroundPlace);
        }

        //筛除周围有水或者土同样的目标方块的位置(加上restrat按钮（加上提示并自动重新加载场景）则可以删除该步骤，该步骤还需继续优化，比如删除该步骤，在前面先创建该需合成的中央方块，再创建其他目标方块)
        //yield return StartCoroutine(findArroundTwoCubePlace2());

        //随机选出2个位置留在列表里
        yield return StartCoroutine(findArroundTwoCubePlace2());
        //创建选出的2个方块
        if (!aimVictoryCondition_arr[numCondition + 2 - 1])//防止多建一次的bug
        {
            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 2] = Instantiate(victoryCubeS, (Vector3)aimVictoryCondition_lis[aimVictoryCondition_lis.Count - 2] * wight, new Quaternion(0, 0, 0, 0)) as GameObject;
            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 2].name = "VictoryConditionS" + (aimVictoryCondition_lis.Count - 2).ToString();//set box ID name by "nowActiveBox_count"
            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 2].layer = 13;// "VictoryCondition"层
            StartCoroutine(ChangeColorAndTagByType(aimVictoryCondition_lis.Count - 2));

            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 1] = Instantiate(victoryCubeS, (Vector3)aimVictoryCondition_lis[aimVictoryCondition_lis.Count - 1] * wight, new Quaternion(0, 0, 0, 0)) as GameObject;
            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 1].name = "VictoryConditionS" + (aimVictoryCondition_lis.Count - 1).ToString();//set box ID name by "nowActiveBox_count"
            aimVictoryCondition_arr[aimVictoryCondition_lis.Count - 1].layer = 13;// "VictoryCondition"层
            StartCoroutine(ChangeColorAndTagByType(aimVictoryCondition_lis.Count - 1));
        }


        yield return null;
    }
    //判断此位置是否可作为合并的中央方块的位置(需改善)
    IEnumerator judgeCubeCenterPlace(Vector3 centerPlace)
    {
        //ArrayList list_tempopary = new ArrayList();
        for (int j = 0; j < numCondition - 1; j++)
        {
            //print("aimVictoryCondition_lis[" + j + "]:" + aimVictoryCondition_lis[j]);
            //print("centerPlace:" + centerPlace);
            //OK
            if (aimVictoryType_lis[j] == "wate" || aimVictoryType_lis[j] == "soil")
            {
                if ((centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 2.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 2.1)))
                    ||(centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 2.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 2.1)))
                    ||(centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 2.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 2.1)))

                    //八个角
                    ||(centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z +1.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 1.1)))
                    || (centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 1.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 1.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1)))
                    || (centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 1.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 1.1)))
                    || (centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 1.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 1.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1)))

                    || (centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 1.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 1.1)))
                    || (centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 1.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 1.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1)))
                    || (centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 1.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 1.1)))
                    || (centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 1.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 1.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1)))

                    || (centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 1.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 1.1)))
                    || (centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 1.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 1.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1)))
                    || (centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 0.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 1.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 0.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 1.1)))
                    || (centerPlace.z < (((Vector3)aimVictoryCondition_lis[j]).z + 0.1) && (centerPlace.z > (((Vector3)aimVictoryCondition_lis[j]).z - 0.1))
                        && centerPlace.y < (((Vector3)aimVictoryCondition_lis[j]).y + 1.1) && (centerPlace.y > (((Vector3)aimVictoryCondition_lis[j]).y - 0.1))
                        && centerPlace.x < (((Vector3)aimVictoryCondition_lis[j]).x + 1.1) && (centerPlace.x > (((Vector3)aimVictoryCondition_lis[j]).x - 0.1)))
                    
                    )

                {
                    centerPlaceIsOk = false;
                    //print(" throw " + centerPlace);
                }
            }
        }
        //print(" OK" + centerPlace);
        yield return null;
    }
    //从列表中的一部分里随机选出指定数目的元素(除去其他被筛选的元素)
    IEnumerator findArroundTwoCubePlace2()
    {
        if (aimVictoryCondition_lis.Count > numCondition + 2)
        {
            int i = numCondition +(int)(Random.value * 100 % (aimVictoryCondition_lis.Count - (numCondition)));
            aimVictoryCondition_lis.Remove(aimVictoryCondition_lis[i]);
            yield return StartCoroutine(findArroundTwoCubePlace2());
        }
        yield return null;
    }
}
