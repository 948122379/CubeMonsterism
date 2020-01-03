using UnityEngine;
using System.Collections;

public class Level3ScriptAnimation : MonoBehaviour {
	//AutoLightLookAt
	private int LightLineCount=4;
	private Transform MainCamera;

	//Model
	private Transform[] LightLines;

	void Start () {
		LightLines = new Transform[LightLineCount];
		for(int i=0;i<LightLineCount;i++)
		{
			LightLines[i]=GameObject.Find("BottomLight/LightLine"+(i+1).ToString()).transform;
		}
		//AutoLightLookAt
		MainCamera=GameObject.Find("Camera Axis/Main Camera").transform;
	}
	
	// Update is called once per frame
	void Update () 
    {
		for(int i=0;i<LightLineCount;i++)
		{
			AutoLightLookAt(LightLines[i]);
		}
	}
	void AutoLightLookAt(Transform theTransforms)
	{
		theTransforms.LookAt(new Vector3(MainCamera.position.x, theTransforms.position.y, MainCamera.position.z));
	}
}
