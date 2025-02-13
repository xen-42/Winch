﻿using Winch.Util;

namespace Winch.Data.WorldEvent.Condition;

public abstract class InventoryItemConditon : InventoryCondition
{
    public string id = string.Empty;

    public ItemData ItemData
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(id) && ItemUtil.AllItemDataDict.TryGetValue(id, out var itemData))
            {
                return itemData;
            }
            return null;
        }
    }

    public abstract override bool Evaluate();

    public bool EvaluateItemInstance(SpatialItemInstance instance) => EvaluateItem(instance.GetItemData<SpatialItemData>());

    public bool EvaluateItem(SpatialItemData data) => data == ItemData;
}