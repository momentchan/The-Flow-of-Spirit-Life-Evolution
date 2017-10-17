using UnityEngine;
using System.Collections;

public class MotionControl : MonoBehaviour
{
	public enum Type
	{
		MouseFollow,
		AutoRoam
	}

	public Type Motion;

	public bool	Jitter;
	public float maxStrength;
	float strength;

	public bool randomJump = true;


	float distance;
	Vector3 targetPosition;
	Vector3 nextPosition;

	public float maxStepDistance = 20f;
	public float moveSpeed = 2f;
	private float reachThreshold = 1.0f;

	//horizontal verticle bound
	private int[] bound = { 30, -30, 10, -10 };



	void Start ()
	{
		Vector3 toObjectVector = transform.position - Camera.main.transform.position;
		Vector3 linearDistanceVector = Vector3.Project (toObjectVector, Camera.main.transform.forward); 
		distance = linearDistanceVector.magnitude;
		targetPosition = transform.position;
	}


	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.M)) {
			if (Motion == Type.AutoRoam)
				Motion = Type.MouseFollow;
			else
				Motion = Type.AutoRoam;
		}

		if (Motion == Type.MouseFollow) {
			
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = distance;
			nextPosition = Camera.main.ScreenToWorldPoint (mousePosition);

		} else if (Motion == Type.AutoRoam) {
			
			if (ReachTarget ()) {
				moveSpeed = 0;
				// Generate Random Target Position
				while (true) {
					
					Vector3 displace = UnityEngine.Random.insideUnitCircle * maxStepDistance;

					// Set Target
					targetPosition = transform.position + displace;

					// Move Bound
					if (targetPosition.x < bound [0] && targetPosition.x > bound [1] && targetPosition.y < bound [2] && targetPosition.y > bound [3] && displace.magnitude > 8f)
						break;
				}
			}
			nextPosition = Vector3.Lerp (transform.position, targetPosition, moveSpeed * Time.deltaTime);
			moveSpeed += 2 * Time.deltaTime;
			if (Jitter)
				nextPosition += JitterMotion (transform.position, targetPosition);
			
			if (randomJump) {
				if (Random.value > 0.995)
					nextPosition = targetPosition;
			}
		}	

		// Update Position
		transform.position = nextPosition;
	}

	public bool ReachTarget ()
	{
		float distance = Vector3.Distance (targetPosition, transform.position);
		return (distance < reachThreshold);
	}


	Vector3 JitterMotion (Vector3 start, Vector3 end)
	{
		// Compute Orthogonal vector
		Vector3 dir = end - start;
		dir = dir.normalized;
		float temp = dir.x;
		dir.x = dir.y;
		dir.y = -temp;


		if (strength >= 0) {
			if (strength - Time.deltaTime < 0)
				strength = Random.Range (-1 * maxStrength, maxStrength);
			else
				strength -= Time.deltaTime;
		} else if (strength < 0) {
			if (strength + Time.deltaTime > 0)
				strength = Random.Range (-1 * maxStrength, maxStrength);
			else
				strength += Time.deltaTime;
		}

		return dir * strength;
	}
		
}
