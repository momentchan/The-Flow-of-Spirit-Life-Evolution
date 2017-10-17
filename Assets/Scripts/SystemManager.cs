using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{

	public CustomParticleSystem SmokeParticleSystem;
	public GPUParticleSystem LeafParticleSystem;
	public TrailRenderer trail;
	public MeshRenderer background;
	public AudioSource audio;


	float moveSpeed = 0.8f;
	float maxDistance = 75f;
	float decay = 0.1f;
	float alpha = 0.5f;


	void Start ()
	{
		Cursor.visible = false;
	}

	void Update ()
	{
		// Move Background
		if (background.transform.position.z < maxDistance) {
			background.transform.position += new Vector3 (0, 0, moveSpeed * Time.deltaTime);
		}


		if (background.transform.position.z > maxDistance - 10) {
			if (GetComponent<Bloom> ().intensity > 0)
				GetComponent<Bloom> ().intensity -= Time.deltaTime * decay;
			
		}
		/// System Fadeout
		if (background.transform.position.z > maxDistance - 5) {

			// Particle System
			SmokeParticleSystem.emission = false;
			LeafParticleSystem.decay -= 2 * Time.deltaTime * decay;

			// Trail
			alpha -= 2 * Time.deltaTime * decay;
			trail.materials [0].SetColor ("_TintColor", new Color (1, 1, 1, alpha));

			// Audio
			audio.volume -= Time.deltaTime * decay;

			// Background
			background.material.color -= Color.white * Time.deltaTime * decay;
			if (background.material.color.r < 0.1f)
				Application.Quit ();
		}
	}
}
