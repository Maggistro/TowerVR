using UnityEngine;
using System.Collections;

/// <summary>
/// Used to display an object in the editor window only
/// </summary>
public class ShowObjectInEditor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Draws a simple cube at object position.
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1,0,0,0.5f);
		Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
	}
}
