using UnityEngine;
using System.Collections;

public class TrapBehaviour : WeaponBehaviour {

	private Collider trigger;

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Enemy")
		{
			this.fire();
		}
	}
}