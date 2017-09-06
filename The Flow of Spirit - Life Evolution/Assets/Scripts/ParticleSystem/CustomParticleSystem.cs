using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticleSystem : MonoBehaviour 
{

	public enum Type{ Leaf, Smoke }

	[Header("Particle System Paremeter")]
	public Type 	ParticleType;
	public GameObject[] particleShapes;
	public Color[] 	emitColors;	
	public bool 	emission;
	public float	emitVel;
	public float	emitRateTime, emitRateDistance;
	public float	minSizeRatio, maxSizeRatio;
	public float	minLife, maxLife;	

	[Header("Cone Paremeter")]
	public float 	radius;
	public float 	angle;

	[Header("Curl Noise")]
	public bool 	noise;
	[Range(0f, 1f)]
	public float 	strength = 1f, frequency = 1f;

	[Header("Fade Effect")]
	public bool 	fade;
	[Range(0, 1)]
	public float 	fadeIn, fadeOut;

	[HideInInspector]
	public float maxVel = 2f;
	public 	Vector3	displacement;
	private Vector3 prePosition;
	private float	emitTimer;
	private	List<Particle>	particles;


	void Start () 
	{
		particles = new List<Particle> ();
		prePosition = transform.position;
	}
		
	void Update () 
	{
		// Coordinate Translation   
		displacement = transform.position - prePosition;
		prePosition = transform.position;

		// Add Particles
		addParticle();

		// Add Curl Noise
		Perturbation ();

		// Kill Particles
		killParticle();
	}

	private void addParticle() 
	{
		if (emission) 
		{
			float emitRate = (displacement.sqrMagnitude == 0) ? emitRateTime : emitRateDistance;
			emitTimer += Time.deltaTime;

			if (emitTimer > 1.0f/emitRate) 
			{
				GameObject particleObj = Instantiate (particleShapes[Random.Range (0, particleShapes.Length)], transform.position, transform.rotation);
				particleObj.transform.parent = transform;

				if (ParticleType == Type.Leaf) 
				{
					particleObj.AddComponent<LeafParticle> ().emitter = this;
					particles.Add (particleObj.GetComponent<LeafParticle> ());
				}
				if (ParticleType == Type.Smoke) 
				{
					particleObj.AddComponent<SmokeParticle> ().emitter = this;
					particles.Add (particleObj.GetComponent<SmokeParticle> ());
				}

				emitTimer = 0f;
			}
		}
	}


	private void killParticle()
	{
		for(int i = particles.Count-1; i >= 0; i--) 
		{
			if (particles[i].isDead ()) 
			{
				Destroy(particles [i].transform.gameObject);
				particles.RemoveAt (i);
			}
		}
	}


	private void Perturbation () 
	{
		if (noise) 
		{
			Vector3 rotation = Vector3.zero;
			Quaternion q = Quaternion.Euler (rotation);
			Quaternion qInv = Quaternion.Inverse (q);

			float moveSpeed = displacement.sqrMagnitude;

			for (int i = 0; i < particles.Count; i++) 
			{
				Vector3 position = particles [i].position;
				float distToSource = position.sqrMagnitude ;

				float ratio = 8f / Mathf.Log (distToSource);
				float amplitude = (moveSpeed > 2f && distToSource < 500)? 0.01f * strength / frequency * ratio  : 0.0005f * strength / frequency;

				Vector3	point = q * new Vector3 (position.z, position.y, position.x);
				NoiseSample sampleX = CurlNoise.PerlinNoise (point, frequency);
				sampleX *= amplitude;
				sampleX.derivative = qInv * sampleX.derivative;

				point = q * new Vector3 (position.x + 100f, position.z, position.y);
				NoiseSample sampleY = CurlNoise.PerlinNoise (point, frequency);
				sampleY *= amplitude;
				sampleY.derivative = qInv * sampleY.derivative;

				point = q * new Vector3 (position.y, position.x + 100f, position.z);
				NoiseSample sampleZ = CurlNoise.PerlinNoise (point, frequency);
				sampleZ *= amplitude;
				sampleZ.derivative = qInv * sampleZ.derivative;

				Vector3 curl;
				curl.x = sampleZ.derivative.x - sampleY.derivative.y;
				curl.y = sampleX.derivative.x - sampleZ.derivative.y;
				curl.z = sampleY.derivative.x - sampleX.derivative.y;
				particles [i].velocity += curl;
			}
		}
	}
}

