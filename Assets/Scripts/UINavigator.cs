using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// User interface navigator. Stores navigation order and implements basic menu function.
/// </summary>
public class UINavigator : MonoBehaviour {

	private GameObject activePanel;
	private LinkedList<GameObject> panelCallOrder = new LinkedList<GameObject>();

	// Use this for initialization
	void Start () {
		activePanel = transform.Find("MainMenu").gameObject;
		panelCallOrder.AddFirst(activePanel);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// Navigates to target panel
	/// </summary>
	/// <param name="target">Panel to navigate to</param>
	public void navigateTo(GameObject target)
	{
		activePanel.SetActive(false);
		activePanel = target;
		panelCallOrder.AddLast(activePanel);
		activePanel.SetActive(true);
	}

	/// <summary>
	/// Navigates back one step in the navigation order.
	/// </summary>
	public void navigateBack()
	{
		if(panelCallOrder.Count>1)
		{
			panelCallOrder.RemoveLast();
			activePanel.SetActive(false);
			activePanel = panelCallOrder.Last.Value;
			activePanel.SetActive(true);
		}

	}
}
