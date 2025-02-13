﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.Localization;
using Winch.Data.Quest.Step;
using Winch.Util;

namespace Winch.Serialization.Quest.Step;

public class QuestStepDataConverter : DredgeTypeConverter<DeferredQuestStepData>
{
    internal static readonly string TableDefinition = LanguageManager.STRING_TABLE;

    private readonly Dictionary<string, FieldDefinition> _definitions = new()
    {
        { "id", new(string.Empty, null) },
        { "mapMarkersToAddOnStart", new(new List<string>(), o=>DredgeTypeHelpers.ParseStringList((JArray)o)) },
        { "mapMarkersToDeleteOnCompletion", new(new List<string>(), o=>DredgeTypeHelpers.ParseStringList((JArray)o)) },
        { "hiddenWhenActive", new(false, o=> bool.Parse(o.ToString())) },
        { "hiddenWhenComplete", new(false, o=> bool.Parse(o.ToString())) },
        { "shortActiveKey", new(LocalizationUtil.Empty, o=> CreateLocalizedString(o.ToString())) },
        { "longActiveKey", new(LocalizationUtil.Empty, o=> CreateLocalizedString(o.ToString())) },
        { "completedKey", new(LocalizationUtil.Empty, o=> CreateLocalizedString(o.ToString())) },
        { "hideIfThisStepIsComplete", new(string.Empty, null) },
        { "showAtDock", new(false, o=> bool.Parse(o.ToString())) },
        { "stepDock", new(string.Empty, null) },
        { "showAtSpeaker", new(false, o=> bool.Parse(o.ToString())) },
        { "stepSpeaker", new(string.Empty, null) },
        { "yarnRootNode", new(string.Empty, null) },
        { "showConditions", new(new List<QuestStepCondition>(), o=>DredgeTypeHelpers.ParseQuestStepConditions((JArray)o)) },
        { "canBeFailed", new(false, o=> bool.Parse(o.ToString())) },
        { "failureEvents", new(new List<QuestStepEvent>(), o=>DredgeTypeHelpers.ParseQuestStepEvents((JArray)o)) },
        { "allowAutomaticCompletion", new(false, o=> bool.Parse(o.ToString())) },
        { "conditionMode", new(ConditionMode.NULL, o => DredgeTypeHelpers.GetEnumValue<ConditionMode>(o)) },
        { "completeConditions", new(new List<QuestStepCondition>(), o=>DredgeTypeHelpers.ParseQuestStepConditions((JArray)o)) },
    };

    public QuestStepDataConverter()
    {
        AddDefinitions(_definitions);
    }

    protected static LocalizedString CreateLocalizedString(string value) => CreateLocalizedString(TableDefinition, value);
}
