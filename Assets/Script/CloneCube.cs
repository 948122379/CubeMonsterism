using UnityEngine;
using System.Collections;

public class CloneCube : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject newOne = Instantiate(this.gameObject);
        Destroy(newOne.GetComponent<CloneCube>());
        newOne.name = this.name + "Clone";
        newOne.AddComponent<DeleteAni>();
        Destroy(this.gameObject);
	}
}
