using UnityEngine;
using System.Collections;

/// <summary>
/// Script for building the turrets by adding parts from bottom to top and removing parts from top to bottom.
/// Add this to the connectors beetween parts;
/// </summary>
public class TurretBuilder : MonoBehaviour {

	[Tooltip("Defines turret part")]
	public Turretpart isPart;

	public enum Turretpart{
		Base, MountToBase, MountToGun, Gun
	}
		
	private bool isConnected = false;
	private TurretBuilder connectedPart = null;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// Sets the connected part.
	/// </summary>
	/// <param name="other">Turretbuilder this part is connected with.</param>
	public void SetConnectedPart(TurretBuilder other)
	{
		this.connectedPart = other;
		isConnected = true;
	}

	/// <summary>
	/// Removes the connected part.
	/// </summary>
	public void RemoveConnectedPart()
	{
		connectedPart = null;
		isConnected = false;
	}

	//snaps the new part to the existing
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Turretpart" && !isConnected)	
		{
			if(IsValid(isPart, other.GetComponent<TurretBuilder>().isPart))
			{
				SnapOtherToCurrent(other.gameObject);
				SetComponentValues();
			}
		}
	}

	/// <summary>
	/// initialises the values for the turret scripts
	/// </summary>
	void SetComponentValues()
	{
		switch (isPart)
		{
		case Turretpart.Base:
			this.GetComponentInParent<TurretBehaviour>().head = this.connectedPart.GetComponentInParent<Rigidbody>().gameObject;
			break;
		case Turretpart.MountToBase:
		case Turretpart.MountToGun:
			FindTurretBehaviour().AddGun(this.connectedPart.GetComponentInParent<Rigidbody>().gameObject);
			break;
		case Turretpart.Gun:
			break;
		}
	}

	TurretBehaviour FindTurretBehaviour()
	{
		foreach(TurretBuilder part in GetComponentInParent<Rigidbody>().GetComponentsInChildren<TurretBuilder>())
		{
			if(part.isPart == Turretpart.MountToBase)
			{
				return part.connectedPart.GetComponentInParent<TurretBehaviour>();
			}
		}
		return null;
	}

	/// <summary>
	/// Determines whether the colliding part is valid to be attached.
	/// </summary>
	/// <returns><c>true</c> If toAttach can be attached to standing, otherwise, <c>false</c>.</returns>
	/// <param name="standing">Already placed turret part</param>
	/// <param name="toAttach">New part</param>
	bool IsValid(Turretpart standing, Turretpart toAttach)
	{
		switch (standing)
		{
			case Turretpart.Base:
			return toAttach == Turretpart.MountToBase;
			case Turretpart.MountToGun:
			return toAttach == Turretpart.Gun;
			default:
			return false;
		}
	}

	/// <summary>
	/// Snaps the colliding object to this one.
	/// </summary>
	/// <param name="other">The turret part to be added</param>
	void SnapOtherToCurrent(GameObject other)
	{
		//get parent object to connector gameobject
		GameObject otherParent = other.GetComponentInParent<Rigidbody>().gameObject;
		//otherParent.GetComponent<Rigidbody>().isKinematic = true;

		//move other gameobject so that other connector is at same position as this connector
		otherParent.transform.position = this.transform.position;
		otherParent.transform.rotation = this.transform.rotation;
		otherParent.transform.Translate(-other.transform.localPosition);

		//add fixed joint 
		HingeJoint connector = otherParent.AddComponent<HingeJoint>();
		connector.anchor = other.transform.localPosition;
		connector.connectedBody = this.GetComponentInParent<Rigidbody>();
		connector.axis = other.GetComponent<TurretBuilder>().isPart == Turretpart.Gun ? other.transform.right : other.transform.up;
		connector.useSpring = true;
		JointSpring spring = connector.spring;
		spring.damper = 0f;
		spring.spring = 1f;
		spring.targetPosition = 0f;
		connector.spring = spring;

		otherParent.GetComponent<Rigidbody>().useGravity = false;

		//set flag for both objects
		connectedPart = other.GetComponent<TurretBuilder>();
		connectedPart.SetConnectedPart(this);
		this.SetConnectedPart(connectedPart);
	}

	/// <summary>
	/// Removes this part from turret.
	/// </summary>
	void RemovePartFromTurret()
	{
		connectedPart.RemoveConnectedPart();
		this.RemoveConnectedPart();
		Destroy(GetComponentInParent<HingeJoint>());
	}
}
