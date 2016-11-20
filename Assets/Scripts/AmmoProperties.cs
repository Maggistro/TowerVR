using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class containing the basic properties of ammo in game.
/// Add this to everything that should damage an enemy.
/// </summary>
public class AmmoProperties : MonoBehaviour {

	[Tooltip("base damage of the ammo")]
	public float baseDamage = 0;
	[Tooltip("Factors for the 3 damage types in order")]
	public float[] bluntSharpSpikeFactor = new float[3];
	[Tooltip("Colliders to inflict each damage types. Can be null.")]
	public Collider[] ammoParts = new Collider[3];
	[Tooltip("Destroy object after colliders hit?")]
	public bool destroyAfterHit = false;

	private bool gotHit = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	// checks if an enemy was hit and inflicts the damage.
	// Only one collider can damage the enemy per hit.
	void OnCollisionEnter(Collision other)
	{
		
		if (other.collider.tag == "Enemy") // only if enemy hit
		{
			
			if(!gotHit) //only if this is the first hit
			{
				gotHit = true; //mark as hit
				float[] damage = new float[3];

				if( other.contacts[0].thisCollider == ammoParts[0] ) //blunt collider
				{
					damage[0]  += baseDamage*bluntSharpSpikeFactor[0];
				}

				if (other.contacts[0].thisCollider == ammoParts[1] ) //sharp collider
				{
					damage[1]  += baseDamage*bluntSharpSpikeFactor[1];
				}

				if (other.contacts[0].thisCollider == ammoParts[2] ) //spike collider
				{
					damage[2]  += baseDamage*bluntSharpSpikeFactor[2];
				}

				other.collider.GetComponent<EnemyBehaviour>().takeDamage(damage);

			}
		}
		else if (other.collider.GetType() == typeof(TerrainCollider))
		{
			if(destroyAfterHit)
			{
				destroyAfterHit = false;
				StartCoroutine("destroyAmmo");
			}
		}
	}

	//resets flag so next hit can deal damage again.
	void OnCollisionExit(Collision other)
	{
		gotHit = false;
	}

	//destroys the ammo after a brief delay.
	IEnumerator destroyAmmo()
	{
		yield return new WaitForSeconds(.5f);
		Destroy(this.gameObject);
	}
}
