using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoneGun : WeaponBehaviour {

	private int timeout = 10;

	new void Update()
	{
		base.Update();
	}

	/// <summary>
	/// Trajectory of the ammo with the specified startPos, direction, speed and resolution.
	/// </summary>
	/// <param name="startPos">Start position in world.</param>
	/// <param name="direction">Direction in world.</param>
	/// <param name="velocity">Velocity of the ammo in units/s.</param>
	/// <param name="resolution">Resolution for getting points to draw in 1/s </param>
	private List<Vector3> calculateTrajectory(Vector3 startPos, Vector3 direction, float velocity, float resolution, bool debug)
	{
		List<Vector3> trajectoryPoints = new List<Vector3>();

		Vector3 previous = ammoStartPosition.transform.position;

		for(float time = 0; time < timeout; time+=resolution)
		{
			Vector3 newPoint = startPos + direction.normalized*velocity*time + Physics.gravity*time*time*.5f;
			trajectoryPoints.Add(newPoint);

			if (debug)
			{
				Debug.DrawLine(previous, newPoint);
			}
			previous = newPoint;
			RaycastHit hit;
			if(Physics.Raycast(previous, newPoint, out hit))
			{
				if(hit.collider.tag == "Enemy")
				{
					break;
				}
			}
		}

		return trajectoryPoints;
	}

	/// <summary>
	/// Returns the last point in the trajectory where it hits a collider
	/// </summary>
	public Vector3 trajectoryHit(GameObject targetInSight)
	{
		float dist = Vector3.Distance(targetInSight.transform.position, ammoStartPosition.transform.position);
		float time = (dist / projectileSpeed);

		Vector3 targetDirection = targetInSight.GetComponent<NavMeshAgent>().velocity * time;
		float grav = Physics.gravity.magnitude*time*time*.5f;

		return targetInSight.transform.position + new Vector3(0, grav, 0) + targetDirection;
	}

	/// <summary>
	/// Draws the trajectory.
	/// </summary>
	public Vector3 drawTrajectory(Vector3 target)
	{
		calculateTrajectory(ammoStartPosition.transform.position, target - ammoStartPosition.transform.position, projectileSpeed, 0.1f,true);
		return new Vector3();
	}

}
