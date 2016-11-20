using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Specific implementation of default behaviour for a dart gun.
/// </summary>
public class DartGun : WeaponBehaviour, IHasTrajectory {

	//maximum time for drawing the trajectory
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
//			RaycastHit hit;
//			if(Physics.Raycast(previous, newPoint, out hit))
//			{
//				if(hit.collider.GetType() == typeof(TerrainCollider))
//				{
//					break;
//				}
//			}
			previous = newPoint;
		}


		return trajectoryPoints;
	}

	public float[] trajectoryHit(GameObject targetInSight)
	{
		GameObject turret = this.GetComponent<HingeJoint>().connectedBody.GetComponent<HingeJoint>().connectedBody.gameObject;

		//first time guess
		float dist = Vector3.Distance(targetInSight.GetComponent<Collider>().bounds.center, ammoStartPosition.transform.position);
		float time = (dist / projectileSpeed); 

		//now factor in enemy movement
		Vector3 targetDirection = targetInSight.GetComponent<NavMeshAgent>().velocity * time;

		//put target on plane with gun
		Vector2 flatTarget = new Vector2();
		flatTarget.x = targetInSight.GetComponent<Collider>().bounds.center.x + targetDirection.x;
		flatTarget.y = targetInSight.GetComponent<Collider>().bounds.center.z + targetDirection.z;

		Vector2 flatStart = new Vector2(turret.transform.position.x, turret.transform.position.z);

		//get left/right angle
		float angleRight = Vector2.Angle(flatTarget - flatStart, new Vector2(turret.transform.forward.x, turret.transform.forward.z));
		angleRight = angleRight * ( ((flatTarget.x - flatStart.x)*turret.transform.forward.z - (flatTarget.y - flatStart.y)* turret.transform.forward.x) > 0 ? 1 : -1);
			
		//get plane distance and elevation
		float x = Vector2.Distance(flatTarget, flatStart);
		float y = targetInSight.GetComponent<Collider>().bounds.center.y + targetDirection.y - ammoStartPosition.transform.position.y;

		float sqrt = projectileSpeed*projectileSpeed*projectileSpeed*projectileSpeed - Physics.gravity.magnitude * ( Physics.gravity.magnitude * x * x + 2 * y * projectileSpeed*projectileSpeed);

		if(sqrt < 0 ){
			Debug.Log("Target not in range");
			return new float[]{0, angleRight};
		}

		sqrt = Mathf.Sqrt(sqrt);

		float anglePos = Mathf.Atan( (projectileSpeed*projectileSpeed + sqrt) /( Physics.gravity.magnitude * x));
		float angleNeg = Mathf.Atan( (projectileSpeed*projectileSpeed - sqrt) /( Physics.gravity.magnitude * x)); 

		return new float[]{angleNeg * Mathf.Rad2Deg, angleRight};
	}

	/// <summary>
	/// Draws the trajectory from the ammospawnpoint in its forward direction.
	/// </summary>
	/// <returns>The first caluclated point on the trajectory in world space.</returns>
	/// <param name="target">The target to draw the trajectory towards in world space.</param>
	public Vector3 drawTrajectory()
	{
		Vector3 vec = calculateTrajectory(ammoStartPosition.transform.position, ammoStartPosition.transform.forward, projectileSpeed, 0.1f,true).ToArray()[1];
		return vec;
	}

}
