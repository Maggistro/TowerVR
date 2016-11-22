using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour {

	[Tooltip("navigation target or spawning prefab")]
	public GameObject navigateTo;
	[Tooltip("Button is spawner")]
	public bool isSpawner;

	private SteamVR_Controller.Device device;
	private Canvas ParentCanvas;
	private SteamVR_FirstPersonController controller;

	// Use this for initialization
	void Start () {
		ParentCanvas = this.GetComponentsInParent<Canvas>()[0];
	}
	
	// Checks if a VR controller is inside the button and the trigger has been pressed
	void Update () {
		if(device != null && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	/// <summary>
	/// On entering the button trigger with VR controller the higlightevent is triggered
	/// </summary>
	/// <param name="other">Object entering the trigger.</param>
	void OnTriggerEnter(Collider other)
	{
		if (device == null && other.gameObject.tag == "Menu")
		{
			ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);

			device = SteamVR_Controller.Input((int)other.GetComponent<FixedJoint>().connectedBody.gameObject.GetComponentInParent<SteamVR_TrackedObject>().index);
			controller = other.GetComponent<FixedJoint>().connectedBody.gameObject.GetComponentInParent<SteamVR_FirstPersonController>();
			Debug.Log("Button entered");
		}
	}


	/// <summary>
	/// On leaving the button trigger with VR controller the unhighlightevent ist triggerd
	/// </summary>
	/// <param name="other">Object exiting the trigger.</param>
	void OnTriggerExit(Collider other)
	{
		if (device != null && other.gameObject.tag == "Menu")
		{
			ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);

			device = null;
			controller = null;
			Debug.Log("Button left");
		}

	}

	/// <summary>
	/// Handles click on the button.
	/// </summary>
	public void On3DButtonClick()
	{
		Debug.Log("Button pressed");
		if(isSpawner)
		{
			StartCoroutine("SpawnObjectPrefab");
		}
		else
		{
			ParentCanvas.GetComponent<UINavigator>().navigateTo(navigateTo);
		}
	}

	/// <summary>
	/// Spawns the defined prefab.
	/// </summary>
	IEnumerator SpawnObjectPrefab()
	{
		yield return new WaitForSeconds(0.5f);
		if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
		{
			GameObject spawned = (GameObject)Instantiate(navigateTo);
			controller.SnapCanGrabObjectToController(spawned);
		}else{
			Debug.Log("Press and hold to spawn");
		}
	}

	/// <summary>
	/// Navigates back using the UINavigator.
	/// </summary>
	public void OnBackButtonClick()
	{
		ParentCanvas.GetComponent<UINavigator>().navigateBack();
	}
}
