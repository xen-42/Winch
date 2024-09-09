﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using Winch.Data.Dock;

namespace Winch.Serialization.Dock;

public class DockDataConverter : DredgeTypeConverter<DeferredDockData>
{
    internal const string DockTableDefinition = "Strings";

    private readonly Dictionary<string, FieldDefinition> _definitions = new()
    {
        { "id", new(string.Empty, null) },
        { "dockNameKey", new(null, o=> CreateLocalizedString(o.ToString())) },
        { "musicAssetReference", new(new AssetReference(), null) },
        { "musicAssetOverrides", new(new List<AssetReferenceOverride>(), null) },
        { "ambienceDayAssetReference", new(new AssetReference(), null) },
        { "ambienceNightAssetReference", new(new AssetReference(), null) },
        { "ambienceDayAssetOverrides", new(new List<AssetReferenceOverride>(), null) },
        { "ambienceNightAssetOverrides", new(new List<AssetReferenceOverride>(), null) },
        { "yarnRootNode", new(string.Empty, null) },
        { "progressTitleLocalizationKey", new(string.Empty, null) },
        { "progressValueLocalizationKey", new(string.Empty, null) },
        { "dockProgressType", new(DockProgressType.NONE, o=>DredgeTypeHelpers.GetEnumValue<DockProgressType>(o)) },
        { "speakers", new(new List<string>(), o=>DredgeTypeHelpers.ParseStringList((JArray)o)) },
        { "hasCameraOverride", new(false, o => bool.Parse(o.ToString())) },
        { "cameraOverrideX", new(0.5f, o => float.Parse(o.ToString())) },
        { "cameraOverrideY", new(0.5f, o => float.Parse(o.ToString()))}
    };

    public DockDataConverter()
    {
        AddDefinitions(_definitions);
    }

    protected static LocalizedString CreateLocalizedString(string value) => CreateLocalizedString(DockTableDefinition, value);
}
