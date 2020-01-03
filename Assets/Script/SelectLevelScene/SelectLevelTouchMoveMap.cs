using System.Diagnostics;
using UnityEngine;
using System.Collections;

public class SelectLevelTouchMoveMap : MonoBehaviour
{
    private bool needmove = false;
    private float speed = 4f;
	// Use this for initialization
	void Start () 
    {
        //安卓平台速度降低
        if (Application.platform == RuntimePlatform.Android) 
        {
            speed = 1f;
        }
        transform.Translate(new Vector3(0f - transform.position.x, -5.15f - transform.position.y, -20.71f - transform.position.z));
	}
    void OnGUI()
    {
        if (Event.current.type == EventType.MouseDrag)
        {
            needmove = true;
        }
        else
        {
            needmove = false;
        }
    }
	// Update is called once per frame
	unsafe void Update () {
        if (needmove == true)
        {
            if (transform.position.y > -5.15f && transform.position.y < 5.15f)
            {
                //print(Input.GetAxis("Mouse Y")+" "+transform.position.y);
                transform.Translate(new Vector3(0f, -speed * Time.deltaTime * Input.GetAxis("Mouse Y"), 0f));
            }
            else if (transform.position.y <= -5.15f)
            {
                if (Input.GetAxis("Mouse Y")<0)
                {
                    transform.Translate(new Vector3(0f, -speed * Time.deltaTime * Input.GetAxis("Mouse Y"), 0f));
                }
                else
                    transform.Translate(new Vector3(0f - transform.position.x, -5.15f - transform.position.y, -20.71f - transform.position.z));
            }
            else if (transform.position.y >= 5.15f)
            {
                if (Input.GetAxis("Mouse Y")>0)
                {
                    transform.Translate(new Vector3(0f, -speed * Time.deltaTime * Input.GetAxis("Mouse Y"), 0f));
                }
                else
                transform.Translate(new Vector3(0f - transform.position.x, 5.15f - transform.position.y, -20.71f - transform.position.z));
            }
        }
	}
}
