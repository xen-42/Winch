﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Winch.Core;

namespace Winch.Util;

public static class AssetBundleUtil
{
    private static Texture2D? bump;
    public static Texture2D bumpTexture
    {
        get
        {
            if (bump == null)
            {
                bump = Texture2D.normalTexture.Instantiate().DontDestroyOnLoad();
                bump.name = "UnityBump";
                Color fillColor = new Color(0.5f, 0.5f, 1f, 0.5f);
                Color[] fillColorArray = bump.GetPixels();
                for (int i = 0; i < fillColorArray.Length; ++i)
                    fillColorArray[i] = fillColor;
                bump.SetPixels(fillColorArray);
                bump.Apply();
                bump.hideFlags = Texture2D.normalTexture.hideFlags;
            }
            return bump;
        }
    }

    public static Dictionary<string, AssetBundle> AssetBundles = new();

    private static Dictionary<int, Shader> blacklistedShaders = new Dictionary<int, Shader>();

    private static Dictionary<string, Shader> cachedShaders = new Dictionary<string, Shader>();

    private static readonly string DEFAULT_SHADER_NAME = "Shader Graphs/Lit_Shader";
    private static readonly string DEFAULT_CUTOUT_SHADER_NAME = "Shader Graphs/Lit_CutoutShader";
    private static readonly string DEFAULT_TEXTURE_ALBEDO_PROP = "Texture2D_9aa7ba2263944b48bbf43c218dc48459";
    private static readonly string DEFAULT_TEXTURE_EMISSION_PROP = "Texture2D_c7b8c5c57d6443a5a9f86b68269754f3";
    private static readonly string DEFAULT_TEXTURE_FLICKER_GRADIENT_PROP = "Texture2D_d75ba12263b343d3ad393c05a6dda1b7";
    private static readonly string DEFAULT_FLOAT_EMIT_STRENGTH_PROP = "_LightStrength";
    private static readonly string DEFAULT_BOOLEAN_EMIT_PROP = "BOOLEAN_0965F30455D645A4AD7F01AF266AE935";
    private static readonly string DEFAULT_BOOLEAN_NIGHTTIME_ONLY_EMIT_PROP = "BOOLEAN_1C655FB145BE4DCDA50D5BE036D8DE1E";
    private static readonly string DEFAULT_BOOLEAN_FLICKER_PROP = "BOOLEAN_3E4CD0FFFCDB4460AA7D73A352CD2455";
    private static readonly string DEFAULT_BOOLEAN_RECIEVE_SHADOWS_PROP = "BOOLEAN_831B22E2CB064148ADEAE08DFC09DFD0";
    private static readonly string DEFAULT_BOOLEAN_WET_EDGES_PROP = "_WETEDGES";
    private static readonly string DEFAULT_FLOAT_WET_EDGE_HEIGHT_PROP = "_WetEdgeHeight";
    private static readonly string DEFAULT_FLOAT_WET_EDGE_DARKNESS_PROP = "_WetEdgeDarkness";
    private static readonly string KEYWORD_ON = "_ON";

    private static Material CreateLitMaterial(string shaderName, string name, Texture? albedo = null, Texture? emission = null, float emitStrength = 4, bool turnOffEmitDuringDay = false, Texture? lightFlickerGradient = null, bool recieveShadows = false, bool wetEdges = false, float wetEdgeHeight = 0.3f, float wetEdgeDarkness = 0.7f)
    {
        Material newMaterial = new Material(AssetBundleUtil.GetReplacementShader(shaderName));
        newMaterial.name = name;
        var keywords = new List<string>();

        if (albedo != null) newMaterial.SetTexture(DEFAULT_TEXTURE_ALBEDO_PROP, albedo);

        newMaterial.SetBoolean(DEFAULT_BOOLEAN_EMIT_PROP, emission != null);
        if (emission != null)
        {
            keywords.Add(DEFAULT_BOOLEAN_EMIT_PROP + KEYWORD_ON);
            newMaterial.SetTexture(DEFAULT_TEXTURE_EMISSION_PROP, emission);
        }

        newMaterial.SetFloat(DEFAULT_FLOAT_EMIT_STRENGTH_PROP, emitStrength);

        newMaterial.SetBoolean(DEFAULT_BOOLEAN_NIGHTTIME_ONLY_EMIT_PROP, turnOffEmitDuringDay);
        if (turnOffEmitDuringDay) keywords.Add(DEFAULT_BOOLEAN_NIGHTTIME_ONLY_EMIT_PROP + KEYWORD_ON);

        newMaterial.SetBoolean(DEFAULT_BOOLEAN_FLICKER_PROP, lightFlickerGradient != null);
        if (lightFlickerGradient != null)
        {
            keywords.Add(DEFAULT_BOOLEAN_FLICKER_PROP + KEYWORD_ON);
            newMaterial.SetTexture(DEFAULT_TEXTURE_FLICKER_GRADIENT_PROP, lightFlickerGradient);
        }

        newMaterial.SetBoolean(DEFAULT_BOOLEAN_RECIEVE_SHADOWS_PROP, recieveShadows);
        if (recieveShadows) keywords.Add(DEFAULT_BOOLEAN_RECIEVE_SHADOWS_PROP + KEYWORD_ON);

        newMaterial.SetBoolean(DEFAULT_BOOLEAN_WET_EDGES_PROP, wetEdges);
        if (wetEdges) keywords.Add(DEFAULT_BOOLEAN_WET_EDGES_PROP);

        newMaterial.SetFloat(DEFAULT_FLOAT_WET_EDGE_HEIGHT_PROP, wetEdgeHeight);
        newMaterial.SetFloat(DEFAULT_FLOAT_WET_EDGE_DARKNESS_PROP, wetEdgeDarkness);

        newMaterial.shaderKeywords = keywords.ToArray();

        return newMaterial;
    }

    public static Material CreateLitMaterial(string name, Texture? albedo = null, Texture? emission = null, float emitStrength = 4, bool turnOffEmitDuringDay = true, Texture? lightFlickerGradient = null, bool recieveShadows = false, bool wetEdges = false, float wetEdgeHeight = 0.3f, float wetEdgeDarkness = 0.7f)
        => CreateLitMaterial(DEFAULT_SHADER_NAME, name, albedo, emission, emitStrength, turnOffEmitDuringDay, lightFlickerGradient, recieveShadows, wetEdges, wetEdgeHeight, wetEdgeDarkness);

    public static Material CreateLitCutoutMaterial(string name, Texture? albedo = null, Texture? emission = null, float emitStrength = 4, bool turnOffEmitDuringDay = true, Texture? lightFlickerGradient = null, bool recieveShadows = false, bool wetEdges = false, float wetEdgeHeight = 0.3f, float wetEdgeDarkness = 0.7f)
        => CreateLitMaterial(DEFAULT_CUTOUT_SHADER_NAME, name, albedo, emission, emitStrength, turnOffEmitDuringDay, lightFlickerGradient, recieveShadows, wetEdges, wetEdgeHeight, wetEdgeDarkness);

    internal static Shader CacheShader(this Shader shader)
    {
        if (cachedShaders.TryGetValue(shader.name, out var replacementShader) && replacementShader != null)
            return replacementShader;

        cachedShaders.AddOrChange(shader.name, shader);
        return shader;
    }

    internal static Shader[] GetSortedShaders()
    {
        var shaders = Resources.FindObjectsOfTypeAll<Shader>().Where(shader => shader.isSupported && !blacklistedShaders.ContainsKey(shader.GetInstanceID())).Distinct(UnityObjectComparer<Shader>.Instance).ToList();
        shaders.Sort(UnityObjectComparer.Instance);
        shaders.Reverse();
        return shaders.ToArray();
    }

    public static Shader GetReplacementShader(string name)
    {
        Shader replacementShader;
        if (cachedShaders.TryGetValue(name, out replacementShader) && replacementShader != null) return replacementShader;

        replacementShader = GetSortedShaders().FirstOrDefault(shader => shader.name == name);// Shader.Find(name);
        if (replacementShader != null)
            return replacementShader.CacheShader();

        return null;
    }

    public static AssetBundle GetBundle(string assetBundleName)
    {
        return AssetBundles[assetBundleName];
    }

    public static GameObject GetPrefab(string assetBundleName, string prefabName)
    {
        return GetBundle(assetBundleName).LoadAsset<GameObject>(prefabName);
    }

    public static AssetBundle? LoadBundle(string assetBundlePath)
    {
        string key = Path.GetFileName(assetBundlePath);
        AssetBundle bundle;

        try
        {
            if (AssetBundles.ContainsKey(key))
            {
                bundle = AssetBundles[key];
            }
            else
            {
                bundle = AssetBundle.LoadFromFile(assetBundlePath);
                if (bundle == null)
                {
                    WinchCore.Log.Error($"Couldn't load asset bundle at [{assetBundlePath}]");
                    return null;
                }

                WinchCore.Log.Debug($"Loaded asset bundle at [{assetBundlePath}]");
                bundle.BlacklistShaders();
                AssetBundles[key] = bundle;
            }
            return bundle;
        }
        catch (Exception e)
        {
            WinchCore.Log.Error($"Couldn't load asset bundle at [{assetBundlePath}]:\n{e}");
            return null;
        }
    }

    /// <summary>
    /// Replaces shaders on all of the asset bundle's prefabs with one's from the game (if they are available)
    /// </summary>
    /// <param name="bundle">The bundle to get the prefabs from and replace their shaders</param>
    public static void BlacklistShaders(this AssetBundle bundle)
    {
        foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>())
        {
            if (prefab != null)
            {
                prefab.BlacklistShaders();
            }
        }
        foreach (Material material in bundle.LoadAllAssets<Material>())
        {
            if (material != null)
            {
                material.BlacklistShader();
            }
        }
    }

    /// <summary>
    /// Replaces shaders on an asset bundle prefab with one's from the game (if they are available)
    /// </summary>
    /// <param name="prefab">The prefab to replace the shaders of</param>
    public static void BlacklistShaders(this GameObject prefab)
    {
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            renderer.BlacklistShaders();
        }
        foreach (var harvestableParticles in prefab.GetComponentsInChildren<HarvestableParticles>(true))
        {
            if (harvestableParticles.specialParticlePrefab != null) harvestableParticles.specialParticlePrefab.BlacklistShaders();
            if (harvestableParticles.disturbedWaterParticles != null) harvestableParticles.disturbedWaterParticles.BlacklistShaders();
            if (harvestableParticles.disturbedOozeParticles != null) harvestableParticles.disturbedOozeParticles.BlacklistShaders();
        }
        foreach (var placeTrees in prefab.GetComponentsInChildren<PlaceTrees>(true))
        {
            placeTrees.treeMaterial.BlacklistShader();
        }
        foreach (var recipeEntry in prefab.GetComponentsInChildren<RecipeEntry>(true))
        {
            recipeEntry.silhouetteMaterial.BlacklistShader();
        }
        foreach (var researchableEntry in prefab.GetComponentsInChildren<ResearchableEntry>(true))
        {
            researchableEntry.silhouetteMaterial.BlacklistShader();
        }
    }

    public static void BlacklistShaders(this Renderer renderer)
    {
        foreach (var material in renderer.sharedMaterials)
        {
            material.BlacklistShader();
        }
    }

    public static bool BlacklistShader(this Material material)
    {
        if (material == null) return false;

        return material.shader.Blacklist();
    }

    public static bool Blacklist(this Shader shader)
    {
        if (shader == null) return false;

        blacklistedShaders.AddOrChange(shader.GetInstanceID(), shader);
        return true;
    }

    /// <summary>
    /// Replaces shaders on all of the asset bundle's prefabs with one's from the game (if they are available)
    /// </summary>
    /// <param name="bundle">The bundle to get the prefabs from and replace their shaders</param>
    public static bool ReplaceShaders(this AssetBundle bundle)
    {
        bool result = true;
        foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>())
        {
            if (prefab != null)
            {
                result = result && prefab.ReplaceShaders();
            }
        }
        foreach (Material material in bundle.LoadAllAssets<Material>())
        {
            if (material != null)
            {
                result = result && material.ReplaceShader();
            }
        }
        return result;
    }

    /// <summary>
    /// Replaces shaders on an asset bundle prefab with one's from the game (if they are available)
    /// </summary>
    /// <param name="prefab">The prefab to replace the shaders of</param>
    public static bool ReplaceShaders(this GameObject prefab)
    {
        bool result = true;
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            result = result && renderer.ReplaceShaders();
        }
        foreach (var harvestableParticles in prefab.GetComponentsInChildren<HarvestableParticles>(true))
        {
            if (harvestableParticles.specialParticlePrefab != null) result = result && harvestableParticles.specialParticlePrefab.ReplaceShaders();
            if (harvestableParticles.disturbedWaterParticles != null) result = result && harvestableParticles.disturbedWaterParticles.ReplaceShaders();
            if (harvestableParticles.disturbedOozeParticles != null) result = result && harvestableParticles.disturbedOozeParticles.ReplaceShaders();
        }
        foreach (var placeTrees in prefab.GetComponentsInChildren<PlaceTrees>(true))
        {
            result = result && placeTrees.treeMaterial.ReplaceShader();
        }
        foreach (var recipeEntry in prefab.GetComponentsInChildren<RecipeEntry>(true))
        {
            result = result && recipeEntry.silhouetteMaterial.ReplaceShader();
        }
        foreach (var researchableEntry in prefab.GetComponentsInChildren<ResearchableEntry>(true))
        {
            result = result && researchableEntry.silhouetteMaterial.ReplaceShader();
        }
        return result;
    }

    /// <summary>
    /// Replaces shaders on an asset bundle prefab's renderer with one's from the game (if they are available)
    /// </summary>
    /// <param name="renderer">The renderer to replace the shaders of</param>
    public static bool ReplaceShaders(this Renderer renderer)
        => renderer.sharedMaterials.AllForEach(ReplaceShader);

    /// <summary>
    /// Replaces shaders on an asset bundle prefab's renderer's material with one's from the game (if they are available)
    /// </summary>
    /// <param name="material">The material to replace the shaders of</param>
    public static bool ReplaceShader(this Material material)
    {
        if (material == null) return false;

        if (SceneManager.GetActiveScene().name == "Manager") return false;

        var replacementShader = GetReplacementShader(material.shader.name);
        if (replacementShader == null) return false;

        // preserve override tag and render queue (for Standard shader)
        // keywords and properties are already preserved
        if (material.renderQueue != material.shader.renderQueue)
        {
            var renderType = material.GetTag("RenderType", false);
            var renderQueue = material.renderQueue;
            material.shader = replacementShader;
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;
        }
        else
        {
            material.shader = replacementShader;
        }

        return true;
    }

    /// <summary>
    /// Replaces shaders on an asset bundle prefab's renderer's material with one's from the game (if they are available)
    /// </summary>
    /// <param name="material">The material to replace the shader of</param>
    /// <param name="shaderName">The name of the shader to grab</param>
    public static bool ReplaceShader(this Material material, string shaderName)
    {
        if (material == null) return false;

        if (SceneManager.GetActiveScene().name == "Manager") return false;

        var replacementShader = GetReplacementShader(shaderName);
        if (replacementShader == null) return false;

        // preserve override tag and render queue (for Standard shader)
        // keywords and properties are already preserved
        if (material.renderQueue != material.shader.renderQueue)
        {
            var renderType = material.GetTag("RenderType", false);
            var renderQueue = material.renderQueue;
            material.shader = replacementShader;
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;
        }
        else
        {
            material.shader = replacementShader;
        }

        return true;
    }
}
