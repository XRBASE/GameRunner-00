using Cohort.Config;
using GLTFast;
using GLTFast.Materials;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

public class GlbMaterialGenerator : MaterialGenerator
{
	//shader value constants
	private const float SPEC_WORKFLOW = 0;
	private const float METAL_WORKFLOW = 1;
	
	//shader property names
	public static readonly int WORKFLOW_PROP = Shader.PropertyToID("_WorkflowMode");
	public static readonly int SURFACE_PROP = Shader.PropertyToID("_Surface");
	
	public static readonly int DIFF_TEX_PROP = Shader.PropertyToID("_BaseMap");
	
	public static readonly int METALLIC_TEX_PROP = Shader.PropertyToID("_MetallicGlossMap");
	public static readonly int METALLIC_PROP = Shader.PropertyToID("_Metallic");
	
	public static readonly int SPEC_GLOSS_TEX_PROP = Shader.PropertyToID("_SpecGlossMap");
	public static readonly int SPEC_COLOR_PROP = Shader.PropertyToID("_SpecColor");
	
	public static readonly int NORMAL_TEX_PROP = Shader.PropertyToID("_BumpMap");
	public static readonly int NORMAL_SCALE_PROP = Shader.PropertyToID("_BumpScale");
	
	public static readonly int OCC_TEX_PROP = Shader.PropertyToID("_OcclusionMap");
	public static readonly int OCC_STRENGTH_PROP = Shader.PropertyToID("_OcclusionStrength");
	
	public static readonly int EMMISIVE_TEX_PROP = Shader.PropertyToID("_EmissionMap");
	public static readonly int EMMISIVE_COLOR_PROP = Shader.PropertyToID("_EmissionColor");
	
	public static readonly int ALPHA_CLIP_ENABLE_PROP = Shader.PropertyToID("_AlphaCutoffEnable");
	public static readonly int ALPHA_CLIP_PROP = Shader.PropertyToID("_AlphaClip");
	public static readonly int ALPHA_THRESHOLD_PROP = Shader.PropertyToID("_Cutoff");
	
	public static readonly int CULL_PROP = Shader.PropertyToID("_Cull");
	// 0 = both, 1 = back, 2 = front
	
	public static readonly int SMOOTH_PROP = Shader.PropertyToID("_Smoothness");
	
	public override Material GenerateMaterial(MaterialBase gltfMaterial, IGltfReadable gltf, bool pointsSupport = false) {
		MaterialType type;
		PbrSpecularGlossiness specGloss = gltfMaterial.Extensions?.KHR_materials_pbrSpecularGlossiness;
		//workflow for material.
		if (gltfMaterial.Extensions?.KHR_materials_unlit != null) {
			type = MaterialType.Unlit;
		}
		else if (gltfMaterial.Extensions?.KHR_materials_pbrSpecularGlossiness != null) {
			type = MaterialType.SpecularGlossiness;
		}
		else {
			type = MaterialType.MetallicRoughness;
		}
		MaterialBase.AlphaMode alpha = gltfMaterial.GetAlphaMode();
		
		//create material with appropriate shader.
		Material mat = GameConfig.Config.GetBaseMaterial(type == MaterialType.Unlit,
			alpha != GLTFast.Schema.Material.AlphaMode.Opaque);
		mat.name = $"La matérielle de Lotte ({gltfMaterial.name})";
		Color baseCol = specGloss?.DiffuseColor ?? Color.white;
		
		if (type != MaterialType.Unlit) {
			mat.SetFloat(WORKFLOW_PROP, (type == MaterialType.MetallicRoughness)? METAL_WORKFLOW : SPEC_WORKFLOW);
		}
		
		switch (type) {
			case MaterialType.Unlit:
				//unlit uses metallic textures and colors.
				if (gltfMaterial.PbrMetallicRoughness != null) {
					//base color
					baseCol = gltfMaterial.PbrMetallicRoughness.BaseColor;
					//base texture
					TrySetTexture(gltfMaterial.PbrMetallicRoughness.BaseColorTexture, mat, gltf, DIFF_TEX_PROP);
				}
				break;
			case MaterialType.MetallicRoughness:
				if (gltfMaterial.PbrMetallicRoughness!=null) {
					//base color
					baseCol = gltfMaterial.PbrMetallicRoughness.BaseColor;
					//base texture
					TrySetTexture(gltfMaterial.PbrMetallicRoughness.BaseColorTexture, mat, gltf, DIFF_TEX_PROP);
					
					//metallic shine slider
					mat.SetFloat(METALLIC_PROP, gltfMaterial.PbrMetallicRoughness.metallicFactor);
					//metallic color texture
					TrySetTexture(gltfMaterial.PbrMetallicRoughness.MetallicRoughnessTexture, mat, gltf, METALLIC_TEX_PROP);
				}
				break;
			case MaterialType.SpecularGlossiness:
				//base color
				baseCol = specGloss.DiffuseColor;
				//base texture
				TrySetTexture(specGloss.diffuseTexture, mat, gltf, DIFF_TEX_PROP);
				
				//specular slider
				mat.SetFloat(SMOOTH_PROP, specGloss.glossinessFactor);
				//specular color
				mat.SetVector(SPEC_COLOR_PROP, specGloss.SpecularColor);
				//specular texture
				TrySetTexture(specGloss.specularGlossinessTexture, mat, gltf, SPEC_GLOSS_TEX_PROP);
				break;
		}
		
		//normal texture and strength.
		if(TrySetTexture(gltfMaterial.NormalTexture, mat, gltf, NORMAL_TEX_PROP)) {
			mat.SetFloat(NORMAL_SCALE_PROP, gltfMaterial.NormalTexture.scale);
		}
		
		//occlusion texture and strength.
		if(TrySetTexture(gltfMaterial.OcclusionTexture, mat, gltf, OCC_TEX_PROP)) {
			mat.SetFloat(OCC_STRENGTH_PROP, gltfMaterial.OcclusionTexture.strength);
		}
		
		//emmision color and texture.
		TrySetTexture(gltfMaterial.EmissiveTexture, mat, gltf, EMMISIVE_TEX_PROP);
		if (gltfMaterial.Emissive != Color.black) {
			mat.EnableKeyword("_EMISSION");
			mat.SetColor(EMMISIVE_COLOR_PROP, gltfMaterial.Emissive);
		}
		
		//alpha.
		
		RenderQueue queue = (alpha == GLTFast.Schema.Material.AlphaMode.Opaque) ? 
			RenderQueue.Geometry: RenderQueue.Transparent;
		mat.SetFloat(SURFACE_PROP, alpha != GLTFast.Schema.Material.AlphaMode.Opaque? 1.0f : 0.0f);
		
		if (alpha == GLTFast.Schema.Material.AlphaMode.Mask) {
			mat.EnableKeyword("_ALPHATEST_ON");
			mat.SetFloat(ALPHA_CLIP_ENABLE_PROP, 1.0f);
			mat.SetFloat(ALPHA_CLIP_PROP, 1.0f);
			mat.SetFloat(ALPHA_THRESHOLD_PROP, gltfMaterial.alphaCutoff);
		}
		else {
			mat.SetFloat(ALPHA_CLIP_ENABLE_PROP, 0.0f);
			mat.SetFloat(ALPHA_CLIP_PROP, 0.0f);
			mat.SetFloat(ALPHA_THRESHOLD_PROP, 0.0f);
		}

		//render settings.
		mat.renderQueue = (int)queue;
		if (gltfMaterial.doubleSided) {
			mat.doubleSidedGI = true;
			mat.SetFloat(CULL_PROP, 0.0f);
		}
		
		//set base color last, so it's set if it was included.
		mat.color = baseCol;
		
		return mat;
	}
	
	//fallback material
	protected override Material GenerateDefaultMaterial(bool pointsSupport = false) {
		if(pointsSupport) {
			Debug.LogWarning("material importer warning: TopologyPointsMaterialUnsupported");
		}

		var defaultMaterial = GameConfig.Config.GetBaseMaterial(true, false);
		defaultMaterial.name = "Default/Fallback material";
		return defaultMaterial;
	}
}
