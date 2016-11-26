using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	public SpawnEnemy spawner;
	public float fadeSpeed;

	private SteamVR_Controller.Device device;

	private int drawDepth = -1000;
	private float alpha = 0;
	private int fadeDir = 0;

	private Texture2D fadeOutTexture;
	// Use this for initialization
	void Start () {
		fadeOutTexture = new Texture2D(2,2);
		fadeOutTexture.SetPixels(0,0,2,2, new Color[]{Color.black, Color.black, Color.black, Color.black});
	}
	
	// Update is called once per frame
	void Update () {
		if(device != null && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			StartCoroutine("StartGame");
		}
	}

	// checks if player entered the trigger area
	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<SteamVR_FirstPersonController>() !=  null)
		{
			device = SteamVR_Controller.Input((int)other.GetComponent<FixedJoint>().connectedBody.gameObject.GetComponentInParent<SteamVR_TrackedObject>().index);
		}
	}

	// checks if player leaves the trigger area
	void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<SteamVR_FirstPersonController>() !=  null)
		{
			device = null;
		}
	}

	IEnumerator StartGame()
	{
		
		yield return new WaitForSeconds(fadeInOut(-1));
		yield return new WaitForSeconds(fadeInOut(1));

		spawner.startSpawn();
	}

	/// <summary>
	/// Fades in and out.
	/// </summary>
	/// <returns>The time it takes for a complete fade in / out</returns>
	/// <param name="dir">Direction to fade in. 1 fades in, -1 fades out</param>
	private float fadeInOut(int dir)
	{
		fadeDir = dir;
		return 1/fadeSpeed;
	}


	void OnGUI ()
	{
		alpha += fadeDir * fadeSpeed * Time.deltaTime; //increase/decrese alpha
		alpha = Mathf.Clamp01(alpha);
		GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
		GUI.depth = drawDepth;
		GUI.DrawTexture (new Rect(0,0,Screen.width, Screen.height), fadeOutTexture); //set texture to fill screen;
	}



}
