using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour 
{

	public CustomParticleSystem 	SmokeParticleSystem;
	public CustomParticleSystem 	LeafParticleSystem;
	public TrailRenderer			trail;
	public MeshRenderer				background;
	public AudioSource 				audio;


	private float moveSpeed = 1.0f;
	private float maxDistance = 75f;
	private float decay = 0.1f;
	private float alpha= 0.5f;

	void Start ()
	{
		Cursor.visible = false;
	}
		
	void Update () 
	{
		// Move Background
		if (background.transform.position.z < maxDistance){
			background.transform.position += new Vector3 (0, 0, moveSpeed * Time.deltaTime);
		}

		// System Fadeout
		if(background.transform.position.z > maxDistance - 5){
			
			// Particle System
			SmokeParticleSystem.emission = false;
			LeafParticleSystem.emission = false;

			// Trail
			alpha -= 2 * Time.deltaTime * decay;
			trail.materials[0].SetColor ("_TintColor", new Color (1, 1, 1, alpha));

			// Audio
			audio.volume -= Time.deltaTime * decay;
	
			// Background
			background.material.color -= Color.white * Time.deltaTime * decay;
			if (background.material.color.r < 0.1f) 
				Application.Quit ();
		}

	}
}
