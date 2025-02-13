﻿using System.Collections.Generic;
using UnityEngine;
using Winch.Core;
using Winch.Serialization.HarvestZone;
using Winch.Data.HarvestZone;
using Winch.Data;

namespace Winch.Util;

public static class HarvestZoneUtil
{
    private static CustomHarvestZoneConverter Converter = new CustomHarvestZoneConverter();

    internal static bool PopulateHarvestZoneFromMetaWithConverter(CustomHarvestZone harvestZone, Dictionary<string, object> meta)
    {
        return UtilHelpers.PopulateObjectFromMeta(harvestZone, meta, Converter);
    }

    internal static Dictionary<string, CustomHarvestZone> ModdedHarvestZoneDict = new();
    internal static Dictionary<string, HarvestZone> CreatedModdedHarvestZoneDict = new();

    internal static void Clear()
    {
        CreatedModdedHarvestZoneDict.Clear();
    }

    public static CustomHarvestZone GetModdedHarvestZone(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (ModdedHarvestZoneDict.TryGetValue(id, out CustomHarvestZone harvestZone))
            return harvestZone;
        else
            return null;
    }

    public static HarvestZone GetCreatedModdedHarvestZone(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (CreatedModdedHarvestZoneDict.TryGetValue(id, out HarvestZone harvestZone))
            return harvestZone;
        else
            return null;
    }

    internal static void CreateModdedHarvestZones()
    {
        foreach (var customHarvestZone in ModdedHarvestZoneDict.Values)
        {
            CreateGameObjectFromCustomHarvestZone(customHarvestZone);
        }
    }

    internal static GameObject CreateGameObjectFromCustomHarvestZone(CustomHarvestZone customHarvestZone)
    {
        GameObject harvestZoneObj = new GameObject(customHarvestZone.id);
        harvestZoneObj.transform.SetParent(GameObject.Find("HarvestZones/FullZones").transform);
        harvestZoneObj.transform.position = customHarvestZone.location;

        if (customHarvestZone.colliderType == ColliderType.SPHERE)
        {
            var sphereCollider = harvestZoneObj.AddComponent<SphereCollider>();
            sphereCollider.radius = customHarvestZone.radius;
            sphereCollider.enabled = true;
            sphereCollider.contactOffset = 0.01f;
        }
        else if (customHarvestZone.colliderType == ColliderType.BOX)
        {
            var boxCollider = harvestZoneObj.AddComponent<BoxCollider>();
            boxCollider.size = customHarvestZone.size;
            boxCollider.enabled = true;
            boxCollider.contactOffset = 0.01f;
        }

        var harvestZone = harvestZoneObj.AddComponent<HarvestZone>();
        harvestZone.harvestableItems = customHarvestZone.HarvestableItems.ToArray();
        harvestZone.day = customHarvestZone.day;
        harvestZone.night = customHarvestZone.night;

        harvestZoneObj.layer = Layer.HarvestZone;

        CreatedModdedHarvestZoneDict.Add(customHarvestZone.id, harvestZone);

        return harvestZoneObj;
    }

    internal static void AddCustomHarvestZoneFromMeta(string metaPath)
    {
        var meta = UtilHelpers.ParseMeta(metaPath);
        if (meta == null)
        {
            WinchCore.Log.Error($"Meta file {metaPath} is empty");
            return;
        }
        var harvestZone = UtilHelpers.GetScriptableObjectFromMeta<CustomHarvestZone>(meta, metaPath);
        if (harvestZone == null)
        {
            WinchCore.Log.Error($"Couldn't create harvest zone");
            return;
        }
        var id = (string)meta["id"];
        if (ModdedHarvestZoneDict.ContainsKey(id))
        {
            WinchCore.Log.Error($"Duplicate harvest zone {id} at {metaPath} failed to load");
            return;
        }
        if (PopulateHarvestZoneFromMetaWithConverter(harvestZone, meta))
        {
            ModdedHarvestZoneDict.Add(id, harvestZone);
        }
        else
        {
            WinchCore.Log.Error($"No harvest zone converter found");
        }
    }
}