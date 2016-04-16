using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    // Button用：関数はファイル分けてね
    public void ReStart()
    {
        // ※using UnityEngine.SceneManagement
        SceneManager.LoadScene("TUMTUM");
    }
}
