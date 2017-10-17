using UnityEngine;
using UnityEngine.Rendering;
using System.IO.Ports;

public partial class GPUParticleSystem : MonoBehaviour
{
	[Header ("Particle System Paremeter")]
	public int particleNums = 1000;
	public Vector3 emitterCenter = Vector3.zero;
	public Vector3 emitterSize = Vector3.one;

	public float life = 4.0f;
	[Range (0, 1)]
	public float lifeRandomness = 0.6f;
	public float decay = 1.0f;
			

	[Header ("Motion Paremeter")]
	public Vector3 velocity = Vector3.forward * 4.0f;

	[Range (0, 1)]
	public float spread = 0.2f;
	[Range (0, 1)]
	public float speedRandomness = 0.5f;

	[Range (0, 10)]
	public float maxWindSpeed = 15f;

	public Vector3 acceleration = Vector3.zero;
	[Range (0, 4)]
	public float dragLevel = 0.1f;

	public float spin = 20.0f;
	public float spinSpeed = 60.0f;
	[Range (0, 1)]
	public float spinRandomness = 0.3f;


	[Header ("Noise Paremeter")]
	public float amplitude = 1.0f;
	public float frequency = 0.2f;


	[Header ("Shape Paremeter")]
	public Mesh[] shapes = new Mesh[1];

	public float scale = 1.0f;
	[Range (0, 1)]
	public float scaleRandomness = 0.5f;


	[Header ("Sensor Paremeter")]
	public Vector3 gyroValue;
	public Vector3 accValue;


	public Material material;
	public Shader kernelShader;
	public Transform follower;

	RenderTexture positionBuffer1;
	RenderTexture positionBuffer2;
	RenderTexture velocityBuffer1;
	RenderTexture velocityBuffer2;
	RenderTexture rotationBuffer1;
	RenderTexture rotationBuffer2;
	MergeMesh mergeMesh;
	Material kernelMaterial;
	MaterialPropertyBlock props;
	bool needReset = true;
	Vector3 prevPosition;

	void Update ()
	{
		if (Input.GetMouseButtonDown (0) || needReset)
			ResetResources ();
		
		SetSensors ();
		UpdateKernel ();
		InvokeKernels ();
		UpdateProps ();

		Mesh mesh = mergeMesh.mesh;
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		Vector2 uv = new Vector2 (0.5f / positionBuffer2.width, 0);

		// Draw Mesh
		for (int i = 0; i < positionBuffer2.height; i++) {
			uv.y = (0.5f + i) / positionBuffer2.height;
			props.SetVector ("_BufferOffset", uv);
			Graphics.DrawMesh (mesh, position, rotation, material, 0, null, 0, props);
		}
	}

	void SetSensors ()
	{
		Server receiver = (Server)GameObject.Find ("SensorReceiver").GetComponent ("Server");

		gyroValue = receiver.gyroValue;

		// Gyro Smoothing
		gyroValue.x = (gyroValue.x < 180) ? gyroValue.x : Mathf.Abs (gyroValue.x - 360);
		gyroValue.y = (gyroValue.y < 180) ? gyroValue.y : Mathf.Abs (gyroValue.y - 360);
		gyroValue.z = (gyroValue.z < 180) ? gyroValue.z : Mathf.Abs (gyroValue.z - 360);
		gyroValue /= 180f;

		Vector3 accValue = receiver.accValue; 
		amplitude = accValue.magnitude * 0.5f;
	}


	RenderTexture CreateBuffer ()
	{
		var width = mergeMesh.particleNums;
		var height = 2;
		var buffer = new RenderTexture (width, height, 0, RenderTextureFormat.ARGBFloat);
		return buffer;
	}


	void UpdateKernel ()
	{
		var m = kernelMaterial;

		emitterCenter = follower.position;
		m.SetVector ("_EmitterPos", emitterCenter);
		m.SetVector ("_EmitterSize", emitterSize);

		var invLifeMax = 1.0f / life;
		var invLifeMin = 1.0f / (life * (1 - lifeRandomness));
		m.SetVector ("_LifeParams", new Vector2 (invLifeMin, invLifeMax));

		Vector3 displace = prevPosition - emitterCenter;
		velocity = new Vector3 (displace.x * 5, displace.y * 5, velocity.z);
		prevPosition = emitterCenter;
		var speed = velocity.magnitude;
		var dir = velocity / speed;
		m.SetVector ("_Direction", new Vector4 (dir.x, dir.y, dir.z, spread));
		m.SetVector ("_SpeedParams", new Vector2 (speed, speedRandomness));

		var drag = Mathf.Exp (-dragLevel * Time.deltaTime);
		var aparams = new Vector4 (acceleration.x, acceleration.y, acceleration.z, drag);
		m.SetVector ("_Acceleration", aparams);

		var sparams = new Vector3 (spin * Mathf.PI / 360, spinSpeed * Mathf.PI / 360, spinRandomness);
		m.SetVector ("_SpinParams", sparams);

		m.SetVector ("_NoiseParams", new Vector2 (frequency, amplitude));

		m.SetVector ("_Config", new Vector2 (Time.deltaTime, Time.time));
	}

	void InvokeKernels ()
	{
		// Swap the buffers.
		var tempPosition = positionBuffer1;
		var tempVelocity = velocityBuffer1;
		var tempRotation = rotationBuffer1;

		positionBuffer1 = positionBuffer2;
		velocityBuffer1 = velocityBuffer2;
		rotationBuffer1 = rotationBuffer2;

		positionBuffer2 = tempPosition;
		velocityBuffer2 = tempVelocity;
		rotationBuffer2 = tempRotation;

		// Invoke the position update kernel.
		kernelMaterial.SetTexture ("_PositionBuffer", positionBuffer1);
		kernelMaterial.SetTexture ("_VelocityBuffer", velocityBuffer1);
		kernelMaterial.SetTexture ("_RotationBuffer", rotationBuffer1);
		Graphics.Blit (null, positionBuffer2, kernelMaterial, 3);
		Graphics.Blit (null, velocityBuffer2, kernelMaterial, 4);
		Graphics.Blit (null, rotationBuffer2, kernelMaterial, 5);
	}

	void UpdateProps ()
	{
		if (props == null)
			props = new MaterialPropertyBlock ();
		props.SetTexture ("_PositionBuffer", positionBuffer2);
		props.SetTexture ("_RotationBuffer", rotationBuffer2);
		props.SetFloat ("_ScaleMin", scale * (1 - scaleRandomness));
		props.SetFloat ("_ScaleMax", scale);
		props.SetFloat ("_Decay", decay);
		props.SetColor ("_Color1", new Color (gyroValue.x, gyroValue.y, gyroValue.z));
		props.SetColor ("_Color2", new Color (gyroValue.x, gyroValue.y, gyroValue.z) * 2);
	}

	void OnDestroy ()
	{
		if (mergeMesh != null)
			mergeMesh.Release ();
		if (positionBuffer1)
			DestroyImmediate (positionBuffer1);
		if (positionBuffer2)
			DestroyImmediate (positionBuffer2);
		if (velocityBuffer1)
			DestroyImmediate (velocityBuffer1);
		if (velocityBuffer2)
			DestroyImmediate (velocityBuffer2);
		if (rotationBuffer1)
			DestroyImmediate (rotationBuffer1);
		if (rotationBuffer2)
			DestroyImmediate (rotationBuffer2);
		if (kernelMaterial)
			DestroyImmediate (kernelMaterial);
	}

	void ResetResources ()
	{ 
		prevPosition = follower.position;
		if (mergeMesh == null)
			mergeMesh = new MergeMesh (shapes, particleNums);
		else
			mergeMesh.Rebuild (shapes, particleNums);

		if (positionBuffer1)
			DestroyImmediate (positionBuffer1);
		if (positionBuffer2)
			DestroyImmediate (positionBuffer2);
		if (velocityBuffer1)
			DestroyImmediate (velocityBuffer1);
		if (velocityBuffer2)
			DestroyImmediate (velocityBuffer2);
		if (rotationBuffer1)
			DestroyImmediate (rotationBuffer1);
		if (rotationBuffer2)
			DestroyImmediate (rotationBuffer2);

		positionBuffer1 = CreateBuffer ();
		positionBuffer2 = CreateBuffer ();
		velocityBuffer1 = CreateBuffer ();
		velocityBuffer2 = CreateBuffer ();
		rotationBuffer1 = CreateBuffer ();
		rotationBuffer2 = CreateBuffer ();

		if (!kernelMaterial)
			kernelMaterial = new Material (kernelShader);

		UpdateKernel ();
		Graphics.Blit (null, positionBuffer2, kernelMaterial, 0);
		Graphics.Blit (null, velocityBuffer2, kernelMaterial, 1);
		Graphics.Blit (null, rotationBuffer2, kernelMaterial, 2);
		needReset = false;
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube (emitterCenter, emitterSize);
	}
}

