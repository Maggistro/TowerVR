using UnityEngine;
using System.Collections;

/// <summary>
/// Basic weapon behaviour implementation. MUST be overwritten by specific weapon to use.
/// </summary>
public abstract class WeaponBehaviour : MonoBehaviour {

	[Tooltip("magazin capacity")]
	public int magazineSize = 0;
	[Tooltip("reload rate in 1/s")]
	public float reloadRate = 0.2f;
	[Tooltip("fire rate in 1/s")]
	public float fireRate = 0.5f;
	[Tooltip("reloadamount per tick")]
	public int reloadSize = 0;
	[Tooltip("show ammo before firing")]
	public bool visibleAmmo = false;
	[Tooltip("projectile speed in m/s")]
	public float projectileSpeed;
	[Tooltip("initial shot position ( blue axis = shot direction )")]
	public GameObject ammoStartPosition;
	[Tooltip("the ammounition prefab")]
	public GameObject shot;

	protected GameObject activeShot;
	protected int ammunition = 0;
	protected enum Weaponstates { FIRING, RELOADING, IDLE };
	protected Weaponstates activeState = Weaponstates.IDLE;

	// Use this for initialization
	protected void Start() {
		activeState = Weaponstates.IDLE;
	}
	
	// Update is called once per frame
	protected void Update () {
		if(ammunition == 0)
		{
			reload();
		}
	}

	/// <summary>
	/// Default implementation to trigger a shot to be fired.
	/// </summary>
	public void fire()
	{
		if(activeState == Weaponstates.IDLE)
		{
			activeState = Weaponstates.FIRING; 
			StartCoroutine("Fire");
		}
	}

	/// <summary>
	/// Default implementation to trigger reloading the weapon without using the touch up ( player interaction ) object.
	/// </summary>
	/// <param name="touchUp">The object the player has to activate to reload. Is ignored by default.</param>
	public void reload(GameObject touchUp = null)
	{
		if(activeState == Weaponstates.IDLE)
		{
			activeState = Weaponstates.RELOADING;
			StartCoroutine("Reload");
		}
	}

	/// <summary>
	/// Can be called to reload the weapon at a given rate with a given reloadSize.
	/// </summary>
	protected IEnumerator Reload()
	{
		Debug.Log("start reloading");
		while(ammunition < magazineSize)
		{
			Debug.Log("reload shot");
			yield return new WaitForSeconds(reloadRate);
			if (visibleAmmo)
			{
				activeShot = (GameObject)Instantiate(shot, ammoStartPosition.transform.position, ammoStartPosition.transform.rotation);
			}
			ammunition+=reloadSize;
		}
		activeState = Weaponstates.IDLE;
	}

	/// <summary>
	/// Fire the weapon using the given fire rate by instantiating the shoot and adding the initial force to the projectile.
	/// </summary>
	protected IEnumerator Fire()
	{
		Debug.Log("try fire");
		if(ammunition != 0)
		{	
			Debug.Log("fire");
			yield return new WaitForSeconds(fireRate);
			ammunition--;
			if(!visibleAmmo)
			{
				activeShot = (GameObject)Instantiate(shot, ammoStartPosition.transform.position, ammoStartPosition.transform.rotation);
			}
			activeShot.GetComponent<Rigidbody>().AddForce(ammoStartPosition.transform.forward.normalized * projectileSpeed, ForceMode.VelocityChange);
			//clone.GetComponent<Rigidbody>().velocity = ammoStartPosition.transform.forward * projectileSpeed;
		}
		activeState = Weaponstates.IDLE;
	}
}
