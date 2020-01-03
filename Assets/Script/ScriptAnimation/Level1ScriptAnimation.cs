using UnityEngine;
using System.Collections;

public class Level1ScriptAnimation : MonoBehaviour {
    //Autorotation
    int planetsCount=10;

    //AutoLightsGlitter
    private bool isadd=true;

    //Model
    private Transform[] Planets;
    private Light PlanetLight;

	void Start () 
    {
        Planets=new Transform[planetsCount];
        for(int i=0;i<planetsCount;i++)
        {
            Planets[i] = GameObject.Find("Planets/Planet"+(i+1).ToString()).transform;
        }
        PlanetLight = GameObject.Find("Planets/Planet5/PointLight").GetComponent<Light>();
	}
	void FixedUpdate () {
        for(int i=0;i<planetsCount;i++)
        {
            Autorotation(Planets[i],(int)(Random.value * 90)%3);
        }
        AutoLightsGlitter(PlanetLight);
	}
     void Autorotation (Transform theTransform,int autorotation_Axis) 
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
    void AutoLightsGlitter(Light theLight)
    {
        if (isadd == true)
        {
            theLight.range += Time.deltaTime/2;
            if (theLight.range > 1.2)
            {
                isadd = false;
            }
        }
        else
        {
            theLight.range -= Time.deltaTime/2;
            if (theLight.range < 0.8)
            {
                isadd = true;
            }
        }
    }
}
