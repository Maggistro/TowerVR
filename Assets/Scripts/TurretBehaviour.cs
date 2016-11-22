using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehaviour : MonoBehaviour {

	public GameObject head;
	public GameObject[] guns = new GameObject[0];

	private GameObject currentTarget;
	private LinkedList<GameObject> targetsInSight = new LinkedList<GameObject>();

	private bool isActive = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (currentTarget != null && isActive)
		{
			aim();
			fire();
		}
		else if (targetsInSight.Count != 0)
		{
			currentTarget = targetsInSight.Last.Value;
		}
	}

	/// <summary>
	/// Activates the turret
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
	public void setActive(bool active)
	{
		this.isActive = active;
	}

	/// <summary>
	/// Adds a gun to the existing guns array
	/// </summary>
	/// <param name="gun">Gun to be added</param>
	public void AddGun(GameObject gun)
	{
		GameObject[] newGuns = new GameObject[this.guns.Length + 1];
		for(int i = 0; i < guns.Length; i++)
		{
			newGuns[i] = guns[i];
		}
		newGuns[guns.Length] = gun;
		guns = newGuns;
	}

	/// <summary>
	/// Aim the turret at the detected enemy
	/// </summary>
	void aim()
	{
		float[] angles = guns[0].GetComponent<IHasTrajectory>().trajectoryHit(currentTarget);

		RotateHead(angles[1]);

		foreach(GameObject gun in guns)
		{
			RotateGuns(angles[0], gun);
		}
		Vector3 targetPoint = guns[0].GetComponent<IHasTrajectory>().drawTrajectory();

	}

	/// <summary>
	/// Rotates the head left/right to look in the general direction of the target
	/// </summary>
	/// <param name="hit">The point of the Target in world space</param> 
	void RotateHead(float angle)
	{
		JointSpring spring = head.GetComponentInChildren<HingeJoint> ().spring;
		spring.targetPosition = angle;
		head.GetComponentInChildren<HingeJoint> ().spring = spring;
	}

	/// <summary>
	/// Rotates the guns down/upward to look in the general direction of the target
	/// </summary>
	/// <param name="hit">Point of the target in world space</param>
	/// <param name="gun">Gun to be rotated</param>
	void RotateGuns(float hit, GameObject gun)
	{
		JointSpring spring = gun.GetComponentInChildren<HingeJoint> ().spring;
		spring.targetPosition = hit;
		gun.GetComponentInChildren<HingeJoint>().spring = spring;
	}

	private void cheat(Vector3 hit, GameObject gun)
	{
		gun.GetComponent<WeaponBehaviour>().ammoStartPosition.transform.LookAt(hit);
	}

	/// <summary>
	/// Tells all attached guns to fire
	/// </summary>
	void fire()
	{
			foreach (GameObject gun in guns)
			{
				gun.GetComponent<WeaponBehaviour>().fire();
			}
	}

	// Adds all enemy entering range to list
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
		{
			targetsInSight.AddFirst(other.gameObject);
			if(targetsInSight.Last.Value == null)
			{
				targetsInSight.RemoveLast();
			}
		}
	}

	// Removes enemies exiting range from list and resets current target if neccessary
	void OnTriggerExit(Collider other)
	{
		if(other.tag == "Enemy")
		{
			targetsInSight.Remove(other.gameObject);
			if (currentTarget == other.gameObject)
			{
				currentTarget = null;
			}
		}
	}
}
