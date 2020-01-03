using UnityEngine;
using System.Collections;

public class GameTouchLogic : MonoBehaviour {
    //暂停
    public bool isAllPauseUpdate = false;

    //手势识别相关变量（左右移动）
    private Vector2 firstPoint = new Vector2(0, 0);
    private Vector2 secondPoint = new Vector2(0, 0);
    private bool needMove = false;
    private string direction_needMove = "";
    private float r_touchDiretion = 0;//要转的方向向量的弧度

    //主摄像机绘制的画面，转动摄像机（左右移动）
    private Rect maincamer;
    private Transform cameraAxis;

    //记录主摄像机Y轴的旋转角度
    private float y_cameraRotation=0;
    //是否需要开启底座的音效
    private bool idNeedPlayVoiceMoveButtom = false;

    //摄像机在屏幕下方的像素范围的高度=屏幕/widthOfRotateCamera
    private int widthOfRotateCamera = 6;

    //下落方块下落目标位置映射方块（快速下落）
    private GameObject lastFloorPlane;

    //双击时间
    private double t1=-1;  
    private double t2=0;
    private bool floorQuickly=false;

    //外部变量from GameLogic//
    //左右移动
    private float wight;//两个方块之间中心点与中心点的距离
    private int countleght;//x、z方向上的方块数
    private GameObject[] boxGameObject_arr;//下落方块数组
    private int nowActiveBox_count = 0;//目前方块数量nowActiveBox_count-1
    private bool gameover;//游戏结束
    //快速下落
    private bool needFloor;//方块下落计时器中的状态记录
    private int[] hadFloorCubeFloor_arr;//已下落方块每个格子已叠最大层数的数组

	void Start () {
        //初始化外部数据
        wight = GetComponent<GameLogic>().wight;
        countleght = GetComponent<GameLogic>().countleght;

        maincamer = GetComponent<Camera>().pixelRect;
        cameraAxis = transform.parent;

        boxGameObject_arr = GetComponent<GameLogic>().boxGameObject_arr;
        nowActiveBox_count = GetComponent<GameLogic>().nowActiveBox_count;
        
        gameover = GetComponent<GameLogic>().gameover;

        needFloor = GetComponent<GameLogic>().needFloor;
        hadFloorCubeFloor_arr = GetComponent<GameLogic>().hadFloorCubeFloor_arr;

        lastFloorPlane = GameObject.Find("LevelsTipsObj/Cube_homologous");
        //调整转动摄像机的屏幕区域提示
        //GameObject.Find("Canvas/Image").GetComponent<RectTransform>().position = new Vector3((maincamer.xMax - maincamer.xMin)/2, (maincamer.yMax - maincamer.yMin) / widthOfRotateCamera/2, 0);
        //GameObject.Find("Canvas/Image").GetComponent<RectTransform>().sizeDelta = new Vector2((maincamer.xMax - maincamer.xMin),(maincamer.yMax - maincamer.yMin) / widthOfRotateCamera);
	}
	void OnGUI()
    {
        //首先获取鼠标按下和鼠标抬起的点
        if (Input.mousePosition.y > maincamer.yMax / widthOfRotateCamera && Input.mousePosition.y < maincamer.yMax)
        {
            idNeedPlayVoiceMoveButtom = false;
            //如果在屏幕中底座上方操作，则按照手指滑动屏幕的方向获取方块移动方向
            if (Event.current.type == EventType.MouseDown)
            {
                firstPoint = Event.current.mousePosition;
            }
            if (Event.current.type == EventType.MouseUp)
            {
                secondPoint = Event.current.mousePosition;
                r_touchDiretion = Vector2.Angle(secondPoint - firstPoint, new Vector2(cameraAxis.forward.x, cameraAxis.forward.z));
                //以已经转动得角度和屏幕手指滑动屏幕的方向计算出方块移动方向
                if (r_touchDiretion > 0 && r_touchDiretion < 45)
                {
                    direction_needMove = "down";
                    needMove = true;
                }
                else if (r_touchDiretion >= 45 && r_touchDiretion < 135)
                {
                    if (cameraAxis.eulerAngles.y < 45 || cameraAxis.eulerAngles.y > 315)//左
                    {
                        if (secondPoint.x - firstPoint.x > 0)//左
                        {
                            direction_needMove = "right";
                            needMove = true;
                        }
                        else if (secondPoint.x - firstPoint.x < 0)
                        {
                            direction_needMove = "left";
                            needMove = true;
                        }
                    }
                    else if (cameraAxis.eulerAngles.y >= 45 && cameraAxis.eulerAngles.y <= 135)
                    {
                        if (secondPoint.y - firstPoint.y < 0)
                        {
                            direction_needMove = "right";
                            needMove = true;
                        }
                        else if (secondPoint.y - firstPoint.y > 0)
                        {
                            direction_needMove = "left";
                            needMove = true;
                        }
                    }
                    else if (cameraAxis.eulerAngles.y > 135 && cameraAxis.eulerAngles.y < 225)
                    {
                        if (secondPoint.x - firstPoint.x < 0)
                        {
                            direction_needMove = "right";
                            needMove = true;
                        }
                        else if (secondPoint.x - firstPoint.x > 0)
                        {
                            direction_needMove = "left";
                            needMove = true;
                        }
                    }
                    else if (cameraAxis.eulerAngles.y >= 225 && cameraAxis.eulerAngles.y <= 315)
                    {
                        if (secondPoint.y - firstPoint.y < 0)
                        {
                            direction_needMove = "left";
                            needMove = true;
                        }
                        else if (secondPoint.y - firstPoint.y > 0)
                        {
                            direction_needMove = "right";
                            needMove = true;
                        }
                    }

                }
                else if (r_touchDiretion >= 135 && r_touchDiretion <= 180)
                {
                    direction_needMove = "up";
                    needMove = true;
                }
            }
        }
        //如果在屏幕底座下方,则转动摄像机
        else if (Input.mousePosition.y > maincamer.yMin && Input.mousePosition.y < maincamer.yMax / widthOfRotateCamera)
        {
            idNeedPlayVoiceMoveButtom = true;
            if (GameObject.Find("LevelBasicScene/Camera Axis/Particle System"))
            {
                GameObject.Find("LevelBasicScene/Camera Axis/Particle System").GetComponent<ParticleSystem>().Play();
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                direction_needMove = "camera";
                needMove = true;
                if (GetComponent<GameVictoryCondition>().level == 1)
                {
                    //放在Updata里转动摄像机的地方
                }
                else if (GetComponent<GameVictoryCondition>().level == 2)
                {
                    GameObject.Find("Camera Axis/Particle System").GetComponent<ParticleSystem>().Play();
                    //底座转动的音效部分在Updata里
                }
                else if (GetComponent<GameVictoryCondition>().level == 3)
                {
                    //底座转动的音效部分在Updata里
                }
            }
        }
        //firstPoint = secondPoint;

    }
	// Update is called once per frame
    /*void Update()
    {
        //双击快速下落（点击box_aim方块在FixedUpdata中）
        if (Input.GetMouseButtonDown(0))
        {
            t2 = Time.realtimeSinceStartup;
            if (t2 - t1 < 0.005)
            {
                floorQuickly = true;
                print("double click");
            }
            t1 = t2;
        }
    }*/
    void Update()
    {
        //底座转动的音效
        if (Input.GetMouseButton(0) && idNeedPlayVoiceMoveButtom ==true)
        {
            if (direction_needMove == "camera")
            {
                if (GameObject.Find("SoundEffect/moveBottom_SoundEffect").GetComponent<AudioSource>().isPlaying == false)
                {
                    GameObject.Find("SoundEffect/moveBottom_SoundEffect").GetComponent<AudioSource>().Play();
                }
            }
        }
        else
        {
            if (GameObject.Find("SoundEffect/moveBottom_SoundEffect").GetComponent<AudioSource>().isPlaying == true)
            {
                GameObject.Find("SoundEffect/moveBottom_SoundEffect").GetComponent<AudioSource>().Stop();
            }
        }

        if (isAllPauseUpdate == false && gameover == false)
        {
            //避免有新建方块时，box_aim位置更新未计算完毕导致方块叠在一起的bug
            /*if (nowActiveBox_count > 0)
            {
                if (boxGameObject_arr[nowActiveBox_count - 1])
                {
                    if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > 6.7)
                    {
                        lastFloorPlane.transform.position = new Vector3(1.1f,6.6f,1.1f);
                    }
                }
            }*/
            //双击快速下落（点击box_aim方块在FixedUpdata中）
            if (Input.GetMouseButtonDown(0))
            {
                t2 = Time.realtimeSinceStartup;
                if (t2 - t1 < 0.3)
                {
                    //避免有新建方块时，box_aim位置更新未计算完毕导致方块叠在一起的bug,所以从第七层开始可以下落
                    if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < 6.7)
                    {
                        floorQuickly = true;
                    }
                    //print("double click");
                }
                t1 = t2;
            }
            //更新外部数据
            boxGameObject_arr = GetComponent<GameLogic>().boxGameObject_arr;
            nowActiveBox_count = GetComponent<GameLogic>().nowActiveBox_count;
            gameover = GetComponent<GameLogic>().gameover;
            needFloor = GetComponent<GameLogic>().needFloor;
            hadFloorCubeFloor_arr = GetComponent<GameLogic>().hadFloorCubeFloor_arr;
            cameraAxis = transform.parent;//需要更新其空间位置

            //如果点击box_aim方块，则直接下落到该位置(双击快速下落在OnGUI()中)
            if (Input.GetMouseButtonDown(1) || floorQuickly == true)
            {
                //print(Input.touchCount);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);//划出射线，只有在scene视图中才能看到
                }
                if (boxGameObject_arr[nowActiveBox_count - 1])
                {
                    if (hitInfo.collider && hitInfo.collider.gameObject.name == "Cube_homologous")
                    {
                        boxGameObject_arr[nowActiveBox_count - 1].transform.position = new Vector3(lastFloorPlane.transform.position.x, lastFloorPlane.transform.position.y + 0.5f, lastFloorPlane.transform.position.z);
                        needFloor = false;
                    }
                    else if (floorQuickly == true)
                    {
                        boxGameObject_arr[nowActiveBox_count - 1].transform.position = new Vector3(lastFloorPlane.transform.position.x, lastFloorPlane.transform.position.y + 0.5f, lastFloorPlane.transform.position.z);
                        needFloor = false;
                    }
                }
                floorQuickly = false;
            }
            //已下落方块每个格子已叠最大层数,countleght * countleght个格子的数据（从第2个方块出现后开始计算）
            /*if (nowActiveBox_count > 0 && boxGameObject_arr[nowActiveBox_count - 1] != null)
            {
                for (int i = 0; i < nowActiveBox_count - 1 && boxGameObject_arr[i] != null; i++)
                {
                    if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > boxGameObject_arr[i].transform.position.x - 0.01
                        && boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < boxGameObject_arr[i].transform.position.x + 0.01
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > boxGameObject_arr[i].transform.position.z - 0.01
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < boxGameObject_arr[i].transform.position.z + 0.01
                            && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > (boxGameObject_arr[i].transform.position.y + wight - 0.01)))
                    {
                        if (hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[i].transform.position.x / wight) + boxGameObject_arr[i].transform.position.z / wight)] <= boxGameObject_arr[i].transform.position.y / wight)//由二维转化成一维数表示0~countleght * countleght
                        {

                            hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[i].transform.position.x / wight) + boxGameObject_arr[i].transform.position.z / wight)] = (int)(boxGameObject_arr[i].transform.position.y / wight);
                        }
                    }
                }
            }*/
            //改变映射方块标识的目标下落位置
            if (nowActiveBox_count - 1 >= 0 && boxGameObject_arr[nowActiveBox_count - 1])
            {
                lastFloorPlane.transform.position = new Vector3(boxGameObject_arr[nowActiveBox_count - 1].transform.position.x, hadFloorCubeFloor_arr[(int)(countleght * (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x / wight) + boxGameObject_arr[nowActiveBox_count - 1].transform.position.z / wight)] * wight + wight - 0.5f, boxGameObject_arr[nowActiveBox_count - 1].transform.position.z);
            }

            //左右移动
            //按照手指滑动屏幕获得的方向移动方块
            if (needMove == true&&needFloor == true)
            {
                //判断是否要移动的位置已有方块
                for (int i = 0; i < nowActiveBox_count - 1; i++)
                {
                    if (boxGameObject_arr[nowActiveBox_count - 1] && boxGameObject_arr[i])
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > boxGameObject_arr[i].transform.position.x - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < boxGameObject_arr[i].transform.position.x + 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > boxGameObject_arr[i].transform.position.y - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < boxGameObject_arr[i].transform.position.y + 0.1
                            && ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < (boxGameObject_arr[i].transform.position.z + wight + 0.1)) && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > (boxGameObject_arr[i].transform.position.z + wight - 0.1))))
                        {
                            if (direction_needMove == "down")
                            {
                                direction_needMove = "";
                                break;
                            }
                        }
                        else if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > boxGameObject_arr[i].transform.position.x - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < boxGameObject_arr[i].transform.position.x + 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > boxGameObject_arr[i].transform.position.y - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < boxGameObject_arr[i].transform.position.y + 0.1
                            && ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < (boxGameObject_arr[i].transform.position.z - wight + 0.1)) && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > (boxGameObject_arr[i].transform.position.z - wight - 0.1))))
                        {
                            if (direction_needMove == "up")
                            {
                                direction_needMove = "";
                                break;
                            }
                        }
                        else if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > boxGameObject_arr[i].transform.position.z - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < boxGameObject_arr[i].transform.position.z + 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > boxGameObject_arr[i].transform.position.y - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < boxGameObject_arr[i].transform.position.y + 0.1
                            && ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < (boxGameObject_arr[i].transform.position.x + wight + 0.1)) && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > (boxGameObject_arr[i].transform.position.x + wight - 0.1))))
                        {
                            if (direction_needMove == "left")
                            {
                                direction_needMove = "";
                                break;
                            }
                        }
                        else if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > boxGameObject_arr[i].transform.position.z - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < boxGameObject_arr[i].transform.position.z + 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y > boxGameObject_arr[i].transform.position.y - 0.1
                            && boxGameObject_arr[nowActiveBox_count - 1].transform.position.y < boxGameObject_arr[i].transform.position.y + 0.1
                            && ((boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < (boxGameObject_arr[i].transform.position.x - wight + 0.1)) && (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > (boxGameObject_arr[i].transform.position.x - wight - 0.1))))
                        {
                            if (direction_needMove == "right")
                            {
                                direction_needMove = "";
                                break;
                            }
                        }
                    }
                }
                //移动方块、摄像机
                if (gameover == false)
                {
                    if (direction_needMove == "left")
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x > 0)
                        {
                            boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(-1 * wight, 0, 0);
                        }
                    }
                    else if (direction_needMove == "right")
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.x < (countleght - 1) * wight)
                        {
                            boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(1 * wight, 0, 0);
                        }
                    }
                    else if (direction_needMove == "up")
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z < (countleght - 1) * wight)
                        {
                            boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(0, 0, 1 * wight);
                        }
                    }
                    else if (direction_needMove == "down")
                    {
                        if (boxGameObject_arr[nowActiveBox_count - 1].transform.position.z > 0)
                        {
                            boxGameObject_arr[nowActiveBox_count - 1].transform.Translate(0, 0, -1 * wight);
                        }
                    }
                    else if (direction_needMove == "camera")
                    {
                        cameraAxis.transform.Rotate(new Vector3(0, 100 * Time.deltaTime * Input.GetAxis("Mouse X"), 0));
                        if (GetComponent<GameVictoryCondition>().level == 1)
                        {
                            GameObject.Find("Planets").transform.Rotate(new Vector3(0f, 80 * Time.deltaTime * Input.GetAxis("Mouse X"), 0f));
                        }
                        if (GetComponent<GameVictoryCondition>().level == 2)
                        {
                            //GameObject.Find("SoundEffect/moveBottom_SoundEffect").GetComponent<AudioSource>().Play();
                        }

                        y_cameraRotation = cameraAxis.transform.rotation.y;
                    }
                    needMove = false;
                }
            }
        }
    } 
   
}
