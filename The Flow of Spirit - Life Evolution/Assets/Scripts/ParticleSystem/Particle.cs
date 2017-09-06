using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour 
{

	public  Vector3  	position;
	public  Vector3		velocity;
	public	float 		lifeTime;
	public	float 		life;
	public CustomParticleSystem emitter;


	public bool isDead()
	{
		if (life < 0)	return true;
		else 			return false;
	}
}

