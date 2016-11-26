using UnityEngine;
using System.Collections;

/// <summary>
/// Default spawner script for adding object during runtime.
/// </summary>
public class SpawnEnemy : MonoBehaviour {
	
	[Tooltip("Prefab to spawn")]
	public GameObject spawnPrefab;
	[Tooltip("Amount of spawns")]
	public int spawnAmount;
	[Tooltip("Delay in beetween spawns")]
	public float spawnDelay = 5.0f;

	public bool autoStart = true;

	// Use this for initialization
	void Start () {
		if(autoStart)
		{
			StartCoroutine("spawn");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void startSpawn()
	{
		StartCoroutine("spawn");
	}

	/// <summary>
	/// Spawns the spawnPrefab each spawnDelay for spawnAmount times.
	/// </summary>
	IEnumerator spawn()
	{
		for(int i = 0; i < spawnAmount; i++)
		{
			GameObject clone = (GameObject) Instantiate(spawnPrefab, transform.position, transform.localRotation);
			clone.SetActive(true);
			if(clone.GetComponent<EnemyBehaviour>() != null)
			{
				Debug.Log("Spawning enemy");
				clone.GetComponent<EnemyBehaviour>().state = EnemyBehaviour.State.MOVE;
			}
			yield return new WaitForSeconds(spawnDelay);
		}
	}
}
