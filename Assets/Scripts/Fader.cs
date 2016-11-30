using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour {


	public float fadeSpeed;

	private Texture2D fadeOutTexture;
	private int drawDepth = -1000;
	private float alpha = 0;
	private int fadeDir = 0;

	// Use this for initialization
	void Start () {
		fadeOutTexture = new Texture2D(2,2);
		fadeOutTexture.SetPixels(0,0,2,2, new Color[]{Color.black, Color.black, Color.black, Color.black});
	}


	/// <summary>
	/// Fades in and out.
	/// </summary>
	/// <returns>The time it takes for a complete fade in / out</returns>
	/// <param name="dir">Direction to fade in. 1 fades in, -1 fades out</param>
	protected float fadeInOut(int dir)
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
