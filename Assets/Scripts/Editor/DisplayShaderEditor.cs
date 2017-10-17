using UnityEngine;
using UnityEditor;

public class DisplayShaderEditor : ShaderGUI
{
	MaterialProperty _color1;
	MaterialProperty _color2;
	MaterialProperty _fadeIn;
	MaterialProperty _fadeOut;
	MaterialProperty _decay;
	MaterialProperty _albedoMap;

	static GUIContent _albedoText = new GUIContent ("Albedo");

	bool _initial = true;

	void FindProperties (MaterialProperty[] props)
	{
		_color1 = FindProperty ("_Color1", props);
		_color2 = FindProperty ("_Color2", props);
		_fadeIn = FindProperty ("_FadeIn", props);
		_fadeOut = FindProperty ("_FadeOut", props);
		_decay = FindProperty ("_Decay", props);
		_albedoMap = FindProperty ("_MainTex", props);
	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		FindProperties (properties);

		if (ShaderPropertiesGUI (materialEditor) || _initial)
			foreach (Material m in materialEditor.targets)
				SetMaterialKeywords (m);

		_initial = false;
	}

	bool ShaderPropertiesGUI (MaterialEditor materialEditor)
	{
		EditorGUI.BeginChangeCheck ();

		var rect = EditorGUILayout.GetControlRect ();
		rect.x += EditorGUIUtility.labelWidth;
		rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;
		materialEditor.ShaderProperty (rect, _color1, "");
		rect.x += rect.width + 4;
		materialEditor.ShaderProperty (rect, _color2, "");
			

		EditorGUILayout.Space ();

		materialEditor.TexturePropertySingleLine (_albedoText, _albedoMap, null);
		materialEditor.TextureScaleOffsetProperty (_albedoMap);

		EditorGUILayout.Space ();
		materialEditor.ShaderProperty (_fadeIn, "FadeIn");
		materialEditor.ShaderProperty (_fadeOut, "FadeOut");
		materialEditor.ShaderProperty (_decay, "Decay");

		return EditorGUI.EndChangeCheck ();
	}

	static void SetMaterialKeywords (Material material)
	{
		SetKeyword (material, "_ALBEDOMAP", material.GetTexture ("_MainTex"));
	}

	static void SetKeyword (Material m, string keyword, bool state)
	{
		if (state)
			m.EnableKeyword (keyword);
		else
			m.DisableKeyword (keyword);
	}
}

