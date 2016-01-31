using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class TriggerScript : MonoBehaviour {
	public GameObject page1;
	public GameObject page2;
	public GameObject page3;
	public GameObject page4;
	public GameObject canvas0;


	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.name== "QuestGiver")
		page1.SetActive(true);
		if (other.gameObject.name == "ExitTrigger") 
		{
			StartCoroutine(fade());
			//Debug.Log ("LoadSceneNext");

			//SceneManager.LoadScene (1);
		}

	}



	IEnumerator fade()
	{
			Debug.Log ("LoadSceneNext");
			canvas0.SetActive(true);
			yield return new WaitForSeconds(5);
			SceneManager.LoadScene (2);

	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
