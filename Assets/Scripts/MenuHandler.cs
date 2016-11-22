using UnityEngine;
using System.Collections;

/// <summary>
/// Menu handler for the VR controller.
/// </summary>
public class MenuHandler : MonoBehaviour {

	private SteamVR_Controller.Device device;
	private Animator animControl;

	[Tooltip("Object representing the menu")]
	public GameObject book;

	[Tooltip("Object for navigating the menu")]
	public GameObject navHelper;

	[Tooltip("Second controller for navigation")]
	public MenuHandler navController;

	// Use this for initialization
	void Start () {
		device = SteamVR_Controller.Input((int)GetComponent<SteamVR_TrackedObject>().index);
		InitBook ();

		navHelper = (GameObject)Instantiate(navHelper, navController.transform.position, navController.transform.rotation);
		navHelper.SetActive(false);
	}

	void InitBook ()
	{
		book = (GameObject)Instantiate (book, transform.position, transform.rotation);
		animControl = book.GetComponent<Animator> ();
		book.SetActive (false);
		foreach (Canvas can in book.GetComponentsInChildren<Canvas> ()) {
			can.enabled = false;
		}
	}
	
	// Check if menu button has been pressed.
	void Update () {
		device = SteamVR_Controller.Input((int)GetComponent<SteamVR_TrackedObject>().index);
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
		{
			if(!navController.book.activeSelf)
			{
				StartCoroutine(toggleMenu());
				StartCoroutine(toggleNavigation());
			}
		}
	}

	/// <summary>
	/// De-/Activates the menu, starts the close/open animation and removes/adds the menu to the controllers position.
	/// </summary>
	/// <returns>The menu.</returns>
	IEnumerator toggleMenu()
	{
		if(book.activeSelf)
		{
			animControl.SetFloat("speed", -1.0f);
			animControl.Play("Default Take", -1, 1);
			StartCoroutine("toggleCanvas", false);
			yield return new WaitForSeconds(book.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length + 0.25f);
			Destroy(book.GetComponent<FixedJoint>());
			book.SetActive(false);
			GetComponent<SteamVR_FirstPersonController>().hideModel(false);
		}
		else
		{
			GetComponent<SteamVR_FirstPersonController>().hideModel(true);
			book.SetActive(true);
			book.transform.position = transform.GetChild(0).Find("tip").GetChild(0).GetComponent<Rigidbody>().transform.position;
			book.transform.rotation = transform.GetChild(0).Find("tip").GetChild(0).GetComponent<Rigidbody>().transform.rotation;
			book.transform.RotateAround(book.transform.position, book.transform.up, -90.0f);
			book.transform.RotateAround(book.transform.position, book.transform.forward, -90.0f);
			book.transform.Translate(book.transform.InverseTransformVector(-book.transform.right * .02f));
			book.AddComponent<FixedJoint>();
			book.GetComponent<FixedJoint>().connectedBody =  transform.GetChild(0).Find("tip").GetChild(0).GetComponent<Rigidbody>();
			book.GetComponent<Animator>().SetFloat("speed", 1.0f);
			yield return new WaitForSeconds(book.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length + 0.25f);
			StartCoroutine("toggleCanvas", true);
		}
	}

	IEnumerator toggleNavigation()
	{
		if(navHelper.activeSelf)
		{
			Destroy(navHelper.GetComponent<FixedJoint>());
			navHelper.SetActive(false);
			navController.GetComponent<SteamVR_FirstPersonController>().hideModel(false);
		}
		else
		{
			navController.GetComponent<SteamVR_FirstPersonController>().hideModel(true);
			navHelper.SetActive(true);
			navHelper.transform.position = navController.transform.GetChild(0).Find("tip").GetChild(0).transform.position;
			navHelper.transform.rotation = navController.transform.GetChild(0).Find("tip").GetChild(0).transform.rotation;

			navHelper.AddComponent<FixedJoint>();
			navHelper.GetComponent<FixedJoint>().connectedBody =  navController.transform.GetChild(0).Find("tip").GetChild(0).GetComponent<Rigidbody>();
		}
		yield return null;
	}

	/// <summary>
	/// Toggles the rendering of the menu canvas.
	/// </summary>
	/// <param name="on">If set to <c>true</c> on.</param>
	IEnumerator toggleCanvas(bool on)
	{
		foreach (Canvas can in book.GetComponentsInChildren<Canvas>())
		{
			can.enabled = on;
		}
		yield return null;
	}

}
