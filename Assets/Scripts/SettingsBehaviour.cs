using UnityEngine;
using System.Collections;

public class SettingsBehaviour : MonoBehaviour {

	public Canvas settingsCanvas;
	public Collider OKCollider;
	public Collider BackCollider;
	public Collider Button1;
	public Collider Button2;

	public Vector3[] LeverStartEnd;
	private string selectedKey;
	private string settingsFile;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void DisplaySettings(){}

	void OKButton(){}

	void BackButton(){}

	void SetLever(float value){}

	void GetLever(float value){}

}
