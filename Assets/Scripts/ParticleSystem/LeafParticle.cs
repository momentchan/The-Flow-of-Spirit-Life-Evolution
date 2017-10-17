using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafParticle : Particle
{
	public 	Vector3 randRotate;
	private MeshRenderer renderer;

	void Start ()
	{
		// Life Time
		lifeTime = Random.Range (emitter.minLife, emitter.maxLife);
		life = lifeTime;

		// Renderer
		renderer = GetComponent<MeshRenderer> ();
		if (emitter.fade) {
			Color color = emitter.emitColors [0];
			color.a = 0;
			renderer.material.color = color;
		}

		// Transformation
		randRotate = new Vector3 (Random.Range (0, 90), Random.Range (0, 90), Random.Range (0, 90));
		transform.position = emitter.transform.position + (Vector3)Random.insideUnitCircle * emitter.radius;
		transform.localScale *= Random.Range (emitter.minSizeRatio, emitter.maxSizeRatio);
		transform.Rotate (0, 0, Random.Range (0, 360));
		transform.Rotate (0, Random.Range (30, 80), 0);

		// Conial Emission
		float angleAxis = emitter.angle * Mathf.Deg2Rad;
		float anglePlane = Random.Range (0, 360) * Mathf.Deg2Rad;
		velocity = new Vector3 (Mathf.Sin (angleAxis) * Mathf.Cos (anglePlane), Mathf.Sin (angleAxis) * Mathf.Sin (anglePlane), Mathf.Cos (angleAxis)) * emitter.emitVel;
	}

	void Update ()
	{
		// Transformation Update
		transform.position += velocity;
		transform.position -= emitter.displacement;
		transform.Rotate (randRotate * Time.deltaTime);
		position = transform.position;


		// Color Blending and Fading
		float lifeRatio = 1 - life / lifeTime;
		int colorStages = emitter.emitColors.Length;
		int stage = (int)(lifeRatio * colorStages);

		Color colorStart = (stage != colorStages) ? emitter.emitColors [stage] : emitter.emitColors [stage - 1];
		Color colorEnd = (stage + 1 < colorStages) ? emitter.emitColors [stage + 1] : emitter.emitColors [colorStages - 1];
		Color color = renderer.material.color;

		if (stage < colorStages)
			color = Color.Lerp (colorStart, colorEnd, (lifeRatio - (float)stage / colorStages) * colorStages);
		if (emitter.fade) {						
			if (lifeRatio < emitter.fadeIn)
				color.a = lifeRatio / emitter.fadeIn;
			else if (lifeRatio > emitter.fadeOut)
				color.a = 1 - (lifeRatio - emitter.fadeOut) / (1 - emitter.fadeOut);
		}
		renderer.material.color = color;

		life -= 2.0f;
	}
}
