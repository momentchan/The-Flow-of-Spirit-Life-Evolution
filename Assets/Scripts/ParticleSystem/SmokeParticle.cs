using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticle : Particle 
{
	private SpriteRenderer 	renderer;
	private float 			expandTime = 1f;
	private float 			timer;
	private float 			targetSizeRatio;
	private Vector3 		initialSize;



	void Start () 
	{
		// Life Time
		lifeTime = Random.Range(emitter.minLife, emitter.maxLife);
		life = lifeTime;

		// Renderer
		renderer = GetComponent<SpriteRenderer>();
		if (emitter.fade) 
		{
			Color color = emitter.emitColors [0];
			color.a = 0;
			renderer.material.color = color;
		}

		// Transformation
		initialSize = transform.localScale;
		targetSizeRatio = Random.Range (emitter.minSizeRatio, emitter.maxSizeRatio);
		transform.localScale = initialSize * emitter.minSizeRatio;

		// Sphere Emission
		velocity = Random.insideUnitSphere * emitter.emitVel;
	}
		
	void Update () 
	{
		// Transformation Update
		timer += 0.5f * Time.deltaTime;
		transform.position += velocity;
		transform.position -= emitter.displacement;
		transform.localScale = initialSize * Mathf.Lerp (emitter.minSizeRatio, targetSizeRatio, timer / expandTime);

		// Color Fading
		float lifeRatio = 1 - life / lifeTime;
		Color color = renderer.material.color;

		if (emitter.fade) 
		{						
			if(lifeRatio < emitter.fadeIn) 
				color.a = lifeRatio / emitter.fadeIn;
			
			else if (lifeRatio > emitter.fadeOut)
				color.a = 1 - (lifeRatio - emitter.fadeOut) / (1 - emitter.fadeOut);
		}
		renderer.material.color = color;

		life -= 2.0f;
	}
}
