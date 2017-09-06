using UnityEngine;
using System.Collections;

public class MouseFollower : MonoBehaviour 
{
	private float distance;

	void Start () 
	{
		Vector3 toObjectVector = transform.position - Camera.main.transform.position;
		Vector3 linearDistanceVector = Vector3.Project (toObjectVector, Camera.main.transform.forward); 
		distance = linearDistanceVector.magnitude;
	}


	void Update () 
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = distance;
		transform.position = Camera.main.ScreenToWorldPoint (mousePosition);
	}
}
