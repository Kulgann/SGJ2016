using UnityEngine;
using System.Collections;

public class nextPageScript : MonoBehaviour {

	public GameObject nextPage;
	public bool isLastPage = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)&& !isLastPage) 
		{
			Debug.Log ("Pressed left click.");
			nextPage.SetActive(true);
			gameObject.SetActive (false);
		}
		else if (Input.GetMouseButtonDown (0)&& isLastPage)
			gameObject.SetActive (false);

	}
}
