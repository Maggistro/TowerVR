using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGame : Fader {

	public int startLevel = 0;

	private SteamVR_Controller.Device device;

	// Update is called once per frame
	void Update () {
		if(device != null && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			StartCoroutine("LoadLevel");
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

	IEnumerator LoadLevel()
	{

		yield return new WaitForSeconds(fadeInOut(-1));
		SceneManager.LoadScene(startLevel);
	}
}
