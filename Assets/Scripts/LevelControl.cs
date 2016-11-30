using UnityEngine;
using System.Collections;

public class LevelControl: Fader {

	public SpawnEnemy spawner;

	private SteamVR_Controller.Device device;


	
	// Update is called once per frame
	void Update () {
		if(device != null && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			StartCoroutine("Play");
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

	IEnumerator Play()
	{
		
		yield return new WaitForSeconds(fadeInOut(-1));
		yield return new WaitForSeconds(fadeInOut(1));

		spawner.startSpawn();
	}





}
