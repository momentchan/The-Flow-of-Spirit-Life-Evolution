using UnityEngine;
using System;

[ExecuteInEditMode]
public class Bloom : MonoBehaviour
{
	[Range (0.0f, 2.0f)]
	public float threshold = 0.75f;
	[Range (0.0f, 2.0f)]
	public float intensity = 0.5f;

	[Range (10.0f, 100.0f)]
	public float kernelSize = 50.0f;

	public 	Shader bloomShader;
	private Material material;


	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material = new Material (bloomShader);

		// Create Temporary Textures
		int w = source.width;
		int h = source.height;
		RenderTexture tmp1 = CreateTexture (w, h);
		RenderTexture tmp2 = CreateTexture (w, h);

	
		// Threshold Filter
		material.SetFloat ("_Threshold", threshold);
		Graphics.Blit (source, tmp1, material, 0);


		// Blur Filter
		BlurBlit (tmp1, tmp2, kernelSize);


		// Bloom Filter
		Vector4 weight = new Vector4 (2f * intensity, 0.5f * intensity, 0.0f, 0.0f);
		material.SetVector ("_Weight", weight);
		material.SetTexture ("_MainTex", source);
		material.SetTexture ("_BlurTex", tmp2);
		Graphics.Blit (source, destination, material, 2);


		RenderTexture.ReleaseTemporary (tmp1);
		RenderTexture.ReleaseTemporary (tmp2);
	}

	RenderTexture CreateTexture (int w, int h)
	{
		RenderTexture rt = RenderTexture.GetTemporary (w, h, 0, RenderTextureFormat.ARGBHalf);
		rt.filterMode = FilterMode.Bilinear;
		rt.wrapMode = TextureWrapMode.Clamp;
		return rt;
	}

	void BlurBlit (RenderTexture source, RenderTexture blur, float kernelSize)
	{
		const float kd0 = (4.0f / 3.0f);
		const float kd1 = (1.0f / 3.0f);

		int iterations = (int)Math.Sqrt ((double)kernelSize);

		float dUV = 1.0f;
		int w = source.width;
		int h = source.height;

		// Copy Texture
		RenderTexture rt = CreateTexture (w, h);
		Graphics.Blit (source, rt);

		float s0 = 1f;
		Vector4 weight;

		for (int i = 0; i < iterations; i++) {
			s0 = (i % 2 != 0 ? -1.0f : 1.0f);

			weight = new Vector4 (s0 * dUV * kd0, dUV * kd1, s0 * dUV * kd1, -dUV * kd0);
			material.SetVector ("_Weight", weight);

			RenderTexture rt2 = CreateTexture (w, h);
			material.SetTexture ("_MainTex", rt);
			Graphics.Blit (rt, rt2, material, 1);

			RenderTexture.ReleaseTemporary (rt);
			rt = rt2;
			dUV = dUV * Mathf.Sqrt (2);
		}

		weight = new Vector4 (-s0 * dUV * kd0, dUV * kd1, -s0 * dUV * kd1, -dUV * kd0);
		material.SetVector ("_Weight", weight);
		material.SetTexture ("_MainTex", rt);
		Graphics.Blit (rt, blur, material, 1);

		RenderTexture.ReleaseTemporary (rt);
	}
}
