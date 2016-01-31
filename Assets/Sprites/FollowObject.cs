using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {
    public GameObject target;
    public string targetTag="default";

    public float offsetX = 0;
    public float offsetY = 0;
    public float offsetZ = 0;
    // Use this for initialization
    void Start () {
        


    }
	
	// Update is called once per frame
	void Update () {
        target = GameObject.FindWithTag(targetTag);
        if (target != null)
        {
            this.transform.position = target.transform.position;
            transform.position = new Vector3(transform.position.x+offsetX, transform.position.y + offsetY, transform.position.z + offsetZ);
        }
	
	}
}
