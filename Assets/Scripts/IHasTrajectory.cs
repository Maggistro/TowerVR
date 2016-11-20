using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Interface for weapons with a fire trajectory.
/// Implementation is weapon specific.
/// </summary>
public interface  IHasTrajectory {



	/// <summary>
	/// Calculates the needed angles for hitting the target targetInSight.
	/// </summary>
	/// <returns>The angles for targeting the enemy [up/down, left/right]</returns>
	/// <param name="targetInSight">Target in sight.</param>
	float[] trajectoryHit(GameObject targetInSight);


	/// <summary>
	/// Draws the trajectory
	/// </summary>
	Vector3 drawTrajectory();

}
