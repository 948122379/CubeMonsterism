using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class levelUpdata : MonoBehaviour {
    public int level = 0;
    public float voiceVelue = 0.3f;
    public int LevelMax=3;

	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
	void Update () {
        if (SceneManager.GetActiveScene().name == "Start")
        {
            Destroy(gameObject);
        }
        if (GameObject.Find("MusicMiner"))
        {
            GameObject.Find("MusicMiner").GetComponent<AudioSource>().volume = voiceVelue;
        }
	}
}
