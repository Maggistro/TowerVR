using UnityEngine;
using System.Collections;

/// <summary>
/// Contains basic functionality for controller such as teleport and grabbing objects.
/// </summary>
public class SteamVR_FirstPersonController : MonoBehaviour
{
	public enum AxisType
	{
		XAxis,
		ZAxis
	}

	Transform reference
	{
		get
		{
			var top = SteamVR_Render.Top();
			return (top != null) ? top.origin : null;
		}
	}
	[Tooltip("pointer color")]
	public Color pointerColor;
	[Tooltip("pointer thickness")]
	public float pointerThickness = 0.002f;
	[Tooltip("outgoing local axis for pointer")]
	public AxisType pointerFacingAxis = AxisType.ZAxis;
	[Tooltip("max pointer length")]
	public float pointerLength = 100f;
	[Tooltip("Display tip?")]
	public bool showPointerTip = true;
	[Tooltip("Teleport active?")]
	public bool teleportWithPointer = true;
	[Tooltip("Teleport delay")]
	public float blinkTransitionSpeed = 0.6f;
	[Tooltip("Highlighting active?")]
	public bool highlightGrabbableObject = true;
	[Tooltip("highlight color")]
	public Color grabObjectHightlightColor;

	private SteamVR_TrackedObject trackedController;
	private SteamVR_Controller.Device device;

	private GameObject pointerHolder;
	private GameObject pointer;
	private GameObject pointerTip;

	private Vector3 pointerTipScale = new Vector3(0.05f, 0.05f, 0.05f);

	private float pointerContactDistance = 0f;
	private Transform pointerContactTarget = null;

	private Rigidbody controllerAttachPoint;
	private FixedJoint controllerAttachJoint;
	private GameObject canGrabObject;
	private Color[] canGrabObjectOriginalColors;
	private GameObject previousGrabbedObject;
	private bool reloader = false;

	private Transform HeadsetCameraRig;
	private float HeadsetCameraRigInitialYPosition;
	private Vector3 TeleportLocation;
	private Material[] oldMats;

	//gets called once per frame
	void Update()
	{
		UpdatePointer();
	}

	//gets called at fixed intervals
	void FixedUpdate()
	{
		device = SteamVR_Controller.Input((int)trackedController.index);

		UpdateGrabbableObjects();
	}

	public void hideModel(bool hide)
	{
		SteamVR_Utils.Event.Send("hide_render_models", hide);
	}

	#region INIT 

	//gets controller reference on 
	void Awake()
	{
		trackedController = GetComponent<SteamVR_TrackedObject>();
	}

	//inits controller, teleport pointer and rig
	void Start()
	{
		InitController();
		InitPointer();
		InitHeadsetReferencePoint();
	}

	//initialises the references to the controller and adds needed components
	void InitController()
	{

		controllerAttachPoint = transform.GetChild(0).Find("tip").GetChild(0).GetComponent<Rigidbody>();

		BoxCollider collider = this.gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(0.1f, 0.1f, 0.2f);
		collider.isTrigger = true;
	}

	//initialises the teleport point and tip with given parameters
	void InitPointer()
	{
		Material newMaterial = new Material(Shader.Find("Unlit/Color"));
		newMaterial.SetColor("_Color", pointerColor);

		pointerHolder = new GameObject();
		pointerHolder.transform.parent = this.transform;
		pointerHolder.transform.localPosition = Vector3.zero;

		pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		pointer.transform.parent = pointerHolder.transform;
		pointer.GetComponent<MeshRenderer>().material = newMaterial;

		pointer.GetComponent<BoxCollider>().isTrigger = true;
		pointer.AddComponent<Rigidbody>().isKinematic = true;
		pointer.layer = 2;

		pointerTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		pointerTip.transform.parent = pointerHolder.transform;
		pointerTip.GetComponent<MeshRenderer>().material = newMaterial;
		pointerTip.transform.localScale = pointerTipScale;

		pointerTip.GetComponent<SphereCollider>().isTrigger = true;
		pointerTip.AddComponent<Rigidbody>().isKinematic = true;
		pointerTip.layer = 2;

		SetPointerTransform(pointerLength, pointerThickness);
		TogglePointer(false);
	}

	//gets the reference to the camera rig
	void InitHeadsetReferencePoint()
	{
		Transform eyeCamera = GameObject.FindObjectOfType<SteamVR_Camera>().GetComponent<Transform>();
		// The referece point for the camera is two levels up from the SteamVR_Camera
		HeadsetCameraRig = eyeCamera.parent;
		HeadsetCameraRigInitialYPosition = HeadsetCameraRig.transform.position.y;
	}

	#endregion

	#region TELEPORT


	/// <summary>
	/// Calculate the lenght and size of the pointer
	/// </summary>
	/// <param name="setLength">Set length.</param>
	/// <param name="setThicknes">Set thicknes.</param>
	void SetPointerTransform(float setLength, float setThicknes)
	{
		//if the additional decimal isn't added then the beam position glitches
		float beamPosition = setLength / (2 + 0.00001f);

		if (pointerFacingAxis == AxisType.XAxis)
		{
			pointer.transform.localScale = new Vector3(setLength, setThicknes, setThicknes);
			pointer.transform.localPosition = new Vector3(beamPosition, 0f, 0f);
			pointerTip.transform.localPosition = new Vector3(setLength - (pointerTip.transform.localScale.x / 2), 0f, 0f);
		}
		else
		{
			pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
			pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);
			pointerTip.transform.localPosition = new Vector3(0f, 0f, setLength - (pointerTip.transform.localScale.z / 2));
		}

		TeleportLocation = pointerTip.transform.position;
	}

	/// <summary>
	/// Gets the length of the pointer beam.
	/// </summary>
	/// <returns>The pointer beam length.</returns>
	/// <param name="hasRayHit">True of raycast has hit a target</param>
	/// <param name="collidedWith">The target hit.</param>
	float GetPointerBeamLength(bool hasRayHit, RaycastHit collidedWith)
	{
		float actualLength = pointerLength;

		//reset if beam not hitting or hitting new target
		if (!hasRayHit || (pointerContactTarget && pointerContactTarget != collidedWith.transform))
		{
			pointerContactDistance = 0f;
			pointerContactTarget = null;
		}

		//check if beam has hit a new target
		if (hasRayHit)
		{
			if (collidedWith.distance <= 0)
			{

			}
			pointerContactDistance = collidedWith.distance;
			pointerContactTarget = collidedWith.transform;
		}

		//adjust beam length if something is blocking it
		if (hasRayHit && pointerContactDistance < pointerLength)
		{
			actualLength = pointerContactDistance;
		}

		return actualLength; ;
	}

	/// <summary>
	/// Toggles the pointer.
	/// </summary>
	/// <param name="state">Active state of pointer and tip</param>
	void TogglePointer(bool state)
	{
		pointer.gameObject.SetActive(state);
		bool tipState = (showPointerTip ? state : false);
		pointerTip.gameObject.SetActive(tipState);
	}

	/// <summary>
	/// Executes a teleport.
	/// </summary>
	void Teleport()
	{
		var t = reference;
		if (t == null)
			return;

		float refY = t.position.y;

		Plane plane = new Plane(Vector3.up, -refY);
		Ray ray = new Ray(this.transform.position, transform.forward);

		bool hasGroundTarget = false;
		float distTerr = 0f;
		float distColl = 0f;
		float dist = 0f;

		RaycastHit hitInfo;
		TerrainCollider tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		hasGroundTarget = tc.Raycast(ray, out hitInfo, 1000f);
		distTerr = hitInfo.distance;

		hasGroundTarget |= Physics.Raycast(ray, out hitInfo);
		distColl = hitInfo.distance;
		dist = distTerr < distColl ? distTerr : distColl; 

		hasGroundTarget &= hitInfo.collider.tag == "Teleportarea";

		if (hasGroundTarget)
		{
			Vector3 headPosOnGround = new Vector3(SteamVR_Render.Top().head.localPosition.x, 0.0f, SteamVR_Render.Top().head.localPosition.z);
			t.position = ray.origin + ray.direction * dist - new Vector3(t.GetChild(0).localPosition.x, 0f, t.GetChild(0).localPosition.z) - headPosOnGround;
		}		
	}

	/// <summary>
	/// Checks for Touchpad press/release and updates pointer if active
	/// </summary>
	void UpdatePointer()
	{
		if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			TogglePointer(true);
		}

		if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
			if (pointerContactTarget != null && teleportWithPointer)
			{
				Teleport();
			}
			TogglePointer(false);
		}


		if (pointer.gameObject.activeSelf)
		{
			//set pointer length and tip
			Ray pointerRaycast = new Ray(transform.position, transform.forward);
			RaycastHit pointerCollidedWith;
			bool rayHit = Physics.Raycast(pointerRaycast, out pointerCollidedWith);
			float pointerBeamLength = GetPointerBeamLength(rayHit, pointerCollidedWith);
			SetPointerTransform(pointerBeamLength, pointerThickness);

			//set pointer tip color
			pointerTip.GetComponent<MeshRenderer>().material.color = pointerCollidedWith.collider.tag == "Teleportarea" ? pointerColor : Color.red;
		}
	}

	#endregion

	#region GRABOBJECT

	/// <summary>
	/// Snaps the grabbable object to controller.
	/// </summary>
	/// <param name="obj">Object to be snapped</param>
	public void SnapCanGrabObjectToController(GameObject obj)
	{
		previousGrabbedObject = obj;
		obj.transform.position = controllerAttachPoint.transform.position;
		obj.transform.rotation = controllerAttachPoint.transform.rotation;

		controllerAttachJoint = obj.AddComponent<FixedJoint>();
		controllerAttachJoint.connectedBody = controllerAttachPoint;
		ToggleGrabbableObjectHighlight(false, canGrabObject);
	}

	/// <summary>
	/// Releases the grabbed object from controller.
	/// </summary>
	/// <returns>The rigidbody of the released object</returns>
	Rigidbody ReleaseGrabbedObjectFromController()
	{
		var jointGameObject = controllerAttachJoint.gameObject;
		var rigidbody = jointGameObject.GetComponent<Rigidbody>();
		Object.DestroyImmediate(controllerAttachJoint);
		controllerAttachJoint = null;

		return rigidbody;
	}

	/// <summary>
	/// Throws the released object using the controller velocity.
	/// </summary>
	/// <param name="rb">Rigidbody to be thrown</param>
	void ThrowReleasedObject(Rigidbody rb)
	{
		var origin = trackedController.origin ? trackedController.origin : trackedController.transform.parent;
		if (origin != null)
		{
			rb.velocity = origin.TransformVector(device.velocity);
			rb.angularVelocity = origin.TransformVector(device.angularVelocity);
		}
		else
		{
			rb.velocity = device.velocity;
			rb.angularVelocity = device.angularVelocity;
		}

		rb.maxAngularVelocity = rb.angularVelocity.magnitude;
	}

	/// <summary>
	/// Updates the grabbable objects available or attached
	/// </summary>
	void UpdateGrabbableObjects()
	{
		if (canGrabObject != null)
		{
			if (controllerAttachJoint == null && device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
			{
				
				SnapCanGrabObjectToController(canGrabObject);
				reloader = previousGrabbedObject.tag == "Reloader";
				if(reloader)
				{
					canGrabObject.GetComponentInParent<WeaponBehaviour>().reload(this.gameObject);
				}
			}
			else if (controllerAttachJoint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
			{
				Rigidbody releasedObjectRigidBody = ReleaseGrabbedObjectFromController();
				ThrowReleasedObject(releasedObjectRigidBody);
				if(reloader)
				{
					canGrabObject.GetComponentInParent<WeaponBehaviour>().fire();
				}
			}
		}
	}

	/// <summary>
	/// Gets the object renderers as array.
	/// </summary>
	/// <returns>The object renderer array.</returns>
	/// <param name="obj">Object to aquire renderer from</param>
	Renderer[] GetObjectRendererArray(GameObject obj)
	{
		return (obj.GetComponents<Renderer>().Length > 0 ? obj.GetComponents<Renderer>() : obj.GetComponentsInChildren<Renderer>());
	}

	/// <summary>
	/// Builds the object color array.
	/// </summary>
	/// <returns>The object color array.</returns>
	/// <param name="obj">Object.</param>
	/// <param name="defaultColor">Default color.</param>
	Color[] BuildObjectColorArray(GameObject obj, Color defaultColor)
	{
		Renderer[] rendererArray = GetObjectRendererArray(obj);

		int length = rendererArray.Length;

		Color[] colors = new Color[length];
		for (int i = 0; i < length; i++)
		{
			colors[i] = defaultColor;
		}
		return colors;
	}

	/// <summary>
	/// Stores the objects original colors.
	/// </summary>
	/// <returns>The objects original colors.</returns>
	/// <param name="obj">Target object</param>
	Color[] StoreObjectOriginalColors(GameObject obj)
	{
		Renderer[] rendererArray = GetObjectRendererArray(obj);

		int length = rendererArray.Length;
		Color[] colors = new Color[length];

		for (int i = 0; i < length; i++)
		{
			var renderer = rendererArray[i];
			colors[i] = renderer.material.color;
		}

		return colors;
	}

	/// <summary>
	/// Changes the color of the object.
	/// </summary>
	/// <param name="obj">Object to change colors</param>
	/// <param name="colors">Colors to change to</param>
	void ChangeObjectColor(GameObject obj, Color[] colors)
	{
		Renderer[] rendererArray = GetObjectRendererArray(obj);
		int i = 0;
		foreach (Renderer renderer in rendererArray)
		{
			renderer.material.color = colors[i];
			i++;
		}
	}

	/// <summary>
	/// Toggles the grabbable object highlight. Restores original colors on false.
	/// </summary>
	/// <param name="highlightObject">If set to <c>true</c> highlight object.</param>
	/// <param name="grabbed">Grabbed.</param>
	void ToggleGrabbableObjectHighlight(bool highlightObject, GameObject grabbed)
	{
		if (highlightGrabbableObject && grabbed != null)
		{            
			if (highlightObject)
			{
				var colorArray = BuildObjectColorArray(grabbed, grabObjectHightlightColor);
				ChangeObjectColor(grabbed, colorArray);
			}
			else
			{
				ChangeObjectColor(grabbed, canGrabObjectOriginalColors);
			}
		}
	}

	//checks if object can be grabbed, and activates highlight
	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Grabbable" || collider.tag == "Reloader")
		{
			if (canGrabObject == null)
			{
				canGrabObjectOriginalColors = StoreObjectOriginalColors(collider.gameObject);
			}
			canGrabObject = collider.gameObject;
			ToggleGrabbableObjectHighlight(true, canGrabObject);
		}
	}

	//checks if object can be grabbed and disables highlight.
	void OnTriggerExit(Collider collider)
	{
		if ( (collider.tag == "Grabbable" || collider.tag == "Reloader" ) && collider.gameObject == previousGrabbedObject)
		{
			ToggleGrabbableObjectHighlight(false, canGrabObject);
			canGrabObject = null;
		}
	}

	#endregion
}