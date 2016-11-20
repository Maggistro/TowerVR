using UnityEngine;
using System.Collections;

/// <summary>
/// Defines the common behaviour for enemeys.
/// </summary>
public class EnemyBehaviour : MonoBehaviour {

	public enum State{MOVE, DESTROYED, STOPPED}


	[Tooltip("Array of waypoints in order")]
	public GameObject[] waypoints;
	[Tooltip("base damage of enemy attacks")]
	public int damage;
	[Tooltip("enemy resistances in order")]
	public float[] bluntSharpSpikeResistance = new float[3];
	[Tooltip("health of the enemy")]
	public float health = 10;
	[Tooltip("Dying animation")]
	public GameObject destroyAnimation;
	[Tooltip("walking animation")]
	public Animator walkAnimation;

	private NavMeshAgent navAgent;
	private int indexOfNextWaypoint;
	private GameObject currentWaypoint;
	public State state;

	// Use this for initialization
	void Start () {
		navAgent = GetComponent<NavMeshAgent>();
		currentWaypoint = waypoints[findIndexOfClosest()];
	}
	
	// Cheap AI statemachine
	void Update () {
		switch(state)
		{
		case State.STOPPED:
			stop();
			break;
		case State.MOVE:
			move();
			break;
		case State.DESTROYED:
			destroy();
			break;
		default:
			print("Invalid enemy AI state");
			break;
		}
		updateAnimation();
	}

	/// <summary>
	/// Finds the index of closest waypoint and returns it.
	/// Use this only for starting/restarting movement.
	/// </summary>
	/// <returns>The index of closest waypoint.</returns>
	int findIndexOfClosest()
	{
		float closestDistance = int.MaxValue;
		float distance = 0;

		for (int i = 0; i < waypoints.Length; i++)
		{
			distance = Vector3.Distance(this.transform.position, waypoints[i].transform.position);
			if (distance < closestDistance)
			{
				indexOfNextWaypoint = i;
				closestDistance = distance;
			}
		}
		return indexOfNextWaypoint;
	}

	/// <summary>
	/// Moves the enemy according to the waypoint list.
	/// </summary>
	void move()
	{
		if (Vector3.Distance(this.transform.position, currentWaypoint.transform.position) <= navAgent.stoppingDistance*2)
		{
			if (waypoints.Length > indexOfNextWaypoint + 1)
			{
				indexOfNextWaypoint++;
				currentWaypoint = waypoints[indexOfNextWaypoint];
			}
			else
			{
				indexOfNextWaypoint = 0;
				currentWaypoint = waypoints[indexOfNextWaypoint];
			}	
		}
		navAgent.SetDestination(currentWaypoint.transform.position);
	}

	/// <summary>
	/// Stops the enemy.
	/// </summary>
	void stop()
	{
		navAgent.Stop();
	}

	//use for handling the emeny animation.
	void updateAnimation()
	{
	//	enemyAnim.speed = navAgent.velocity.magnitude / 14f;
	}

	/// <summary>
	/// Destroy the enemy, play the dying animation and destroy the dying animation on finish.
	/// </summary>
	void destroy()
	{
		GameObject anim = (GameObject)Instantiate(destroyAnimation, this.GetComponentInChildren<MeshRenderer>().transform.position, this.GetComponentInChildren<MeshRenderer>().transform.localRotation);
		this.gameObject.SetActive(false);

		Destroy(anim, anim.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);
		Destroy(this.gameObject);
	}

	/// <summary>
	/// Calculates the damage and sets remaining health for the enemy.
	/// </summary>
	/// <param name="bluntSharpSpikeDamage">Array of damage ordered by type.</param>
	public void takeDamage(float[] bluntSharpSpikeDamage)
	{
		for(int i=0; i < bluntSharpSpikeDamage.Length; i++)
		{
			health -= bluntSharpSpikeDamage[i] * (1 - bluntSharpSpikeResistance[i]);
		}

		Debug.Log("damage taken: " + bluntSharpSpikeDamage.ToString() + " Health at " + health);

		if(health <= 0)
		{
			state = State.DESTROYED;
		}
	}

}
