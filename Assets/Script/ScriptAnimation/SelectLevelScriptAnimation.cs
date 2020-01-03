using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLevelScriptAnimation : MonoBehaviour
{
    //Autorotation
    private int autorotation_Axis;
    private bool ScaleAdding = true;

    //MoveUpAndDown var
    private bool UpDownAdding = true;
    private float minSclace=1f;
    private float maxSclace = 1.5f;
    private float minY;
    private float maxY;
    private float arange = 2f;
    private float time_move = 2f;
    private float sclace_speed;
    private float Y_speed;

    //AutoScaleAphla
    private SpriteRenderer  CircleRender_1;
    private SpriteRenderer  CircleRender_2;
    private SpriteRenderer  CircleRender_3;

    //AutoMapBigSmall
    private float speedDistance = 0.5f;
    private bool[] isMoveUps = {false,false,false,false};
    private float[] MapMoveSpeeds= {1f,1f,1f,1f};

    //Model
    private Transform SelectLevelModel_1;
    private Transform SelectLevelModel_2;
    private Transform SelectLevelModel_3;
    private Transform SelectLevelCircle_1;
    private Transform SelectLevelCircle_2;
    private Transform SelectLevelCircle_3;
    private Transform SelectLevelBg_1;
    private Transform SelectLevelBg_2;
    private Transform SelectLevelBg_3;
    private Transform SelectLevelBg_4;
    void Start()
    {
        //Model
        SelectLevelModel_1=GameObject.Find("Levels/level_1").transform;
        SelectLevelModel_2=GameObject.Find("Levels/level_2/Cacti").transform;
        SelectLevelModel_3=GameObject.Find("Levels/level_3/Windows/Pointlight").transform;
        SelectLevelCircle_1=GameObject.Find("Background/LevelBackground/selectLevel_levelWay/Level1Circle").transform;
        SelectLevelCircle_2=GameObject.Find("Background/LevelBackground/selectLevel_levelWay/Level2Circle").transform;
        SelectLevelCircle_3=GameObject.Find("Background/LevelBackground/selectLevel_levelWay/Level3Circle").transform;
        SelectLevelBg_1=GameObject.Find("Background/Bgs/1").transform;
        SelectLevelBg_2=GameObject.Find("Background/Bgs/2").transform;
        SelectLevelBg_3=GameObject.Find("Background/Bgs/3").transform;
        SelectLevelBg_4=GameObject.Find("Background/Bgs/4").transform;
        //Autorotation
        autorotation_Axis = (int)(Random.value * 90)%3;
        //AutoMoveUpAndDown
        minY=SelectLevelModel_3.position.z-arange/2;
        maxY=SelectLevelModel_3.position.z+arange/2;
        sclace_speed = (maxSclace - minSclace) / time_move;
        Y_speed = arange / time_move;
        //AutoScaleAphla
        CircleRender_1=SelectLevelCircle_1.GetComponent<SpriteRenderer>();
        CircleRender_2=SelectLevelCircle_2.GetComponent<SpriteRenderer>();
        CircleRender_3=SelectLevelCircle_3.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        Autorotation(SelectLevelModel_1);
        AutoScale(SelectLevelModel_2);
        AutoLightScleUpDown(SelectLevelModel_3);
        AutoScaleAphla(SelectLevelCircle_1,CircleRender_1);
        AutoScaleAphla(SelectLevelCircle_2,CircleRender_2);
        AutoScaleAphla(SelectLevelCircle_3,CircleRender_3);
        AutoMapUpDown(SelectLevelBg_1,isMoveUps,MapMoveSpeeds,0);
        AutoMapUpDown(SelectLevelBg_2,isMoveUps,MapMoveSpeeds,1);
        AutoMapUpDown(SelectLevelBg_3,isMoveUps,MapMoveSpeeds,2);
        AutoMapUpDown(SelectLevelBg_4,isMoveUps,MapMoveSpeeds,3);
    }
    void Autorotation (Transform theTransform) 
    {
        if (autorotation_Axis == 0)
        {
            theTransform.Rotate(new Vector3( Random.value/2,0.2f, 0));
        }
        else if (autorotation_Axis == 1)
        {
            theTransform.Rotate(new Vector3(0.2f, Random.value/2 ,0));
        }
        else if (autorotation_Axis == 2)
        {
            theTransform.Rotate(new Vector3(Random.value/2, 0,0.2f ));
        }
	}
    void AutoScale (Transform theTransform) 
    {
        if (ScaleAdding)
        {
            float i = Time.deltaTime/10;
            theTransform.localScale = new Vector3(theTransform.localScale.x + i, theTransform.localScale.y + i, theTransform.localScale.z + i);
            if (theTransform.localScale.x > 1.1)
            {
                ScaleAdding = false;
            }
        }
        else
        {
            float i = -Time.deltaTime/10;
            theTransform.localScale = new Vector3(theTransform.localScale.x + i, theTransform.localScale.y + i, theTransform.localScale.z + i);
            if (theTransform.localScale.x < 0.9)
            {
                ScaleAdding = true;
            }
        }
    }
    void AutoLightScleUpDown(Transform theTransform)
    {
        Light theLight=theTransform.GetComponent<Light>();
        if (UpDownAdding == true)
        {
            theLight.range += sclace_speed/60;
            theTransform.Translate(new Vector3(0, 0, +Y_speed / 60));
            if (theLight.range > maxSclace || theTransform.position.z > maxY )
            {
                theLight.range = maxSclace;
                theTransform.Translate(new Vector3(0, 0, maxY -theTransform.position.z));
                UpDownAdding = false;
            }
        }
        else if (UpDownAdding == false)
        {
            theLight.range -= sclace_speed / 60;
            theTransform.Translate(new Vector3(0, 0, -Y_speed/60));
            if (theLight.range < minSclace || theTransform.position.z< minY)
            {
                theLight.range = minSclace;
                theTransform.Translate(new Vector3(0, 0, minY -  theTransform.position.z));
                UpDownAdding = true;
            }
        }
    }
    void AutoScaleAphla(Transform theTransform,SpriteRenderer theRender)
    {
        if(theTransform.gameObject.name.Substring(5,1)=="1")
        {
            float speed = 1f;
            theTransform.Rotate(new Vector3(0,0,speed),Space.World);
        }
        else    //leve2&3的圆圈用此模式
        {
            float speed=0.002f;
            float startScale=0.45f;
            float distoryScale=0.7f;
            float speedAphla=(float)(speed/(distoryScale-startScale));   //(1-0)/speedAphla=(7-4.5)/speed,颜色各值范围为0~1
            if(theTransform.localScale.x<distoryScale)
            {
                theTransform.localScale+=new Vector3(speed,speed,speed);
                theRender.color-=new Color(0,0,0,speedAphla);
            }
            else 
            {
                theTransform.localScale=startScale*new Vector3(1f,1f,1f);
                theRender.color+=new Color(0,0,0,1-theRender.color.a);
            }
        }
    }
    void AutoMapUpDown(Transform theTransform,bool[] isMoveUps,float[] MapMoveSpeeds,int i)
    {
        if (isMoveUps[i] == true)
        {
            theTransform.Translate(new Vector3(0, MapMoveSpeeds[i]*Time.deltaTime, 0));
            if (theTransform.position.y >= 0)
            {
                isMoveUps[i] = false;
                MapMoveSpeeds[i]  = Random.Range(0.1f,0.6f);
            }
        }
        if (isMoveUps[i] == false)
        {
            theTransform.Translate(new Vector3(0, -MapMoveSpeeds[i]*Time.deltaTime, 0));
            if (theTransform.position.y < -speedDistance)
            {
                isMoveUps[i] = true;
                MapMoveSpeeds[i] = Random.Range(0.1f,0.6f);
            }
        }
    }
}
