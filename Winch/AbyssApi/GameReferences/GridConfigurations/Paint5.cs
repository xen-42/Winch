namespace Winch.AbyssApi.GameReferences.GridConfigurations;
public static class Paint5
{
    public static GridConfiguration Paint5Instance = (GridConfiguration)System.Linq.Enumerable.First(ScriptableObjectInstances.GridConfigurations, x => x.name == "Paint5");
     ///<json>
     /// {
     ///    "$content": [
     ///        {
     ///            "cells": [],
     ///            "itemType": "",
     ///            "itemSubtype": "",
     ///            "isHidden": true,
     ///            "damageImmune": false
     ///        }
     ///    ],
     ///    "$type": "System.Collections.Generic.List`1[[CellGroupConfiguration, DREDGE_Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]"
     ///}
     ///</json>
    public static System.Collections.Generic.List<CellGroupConfiguration> cellGroupConfigs = Paint5Instance.cellGroupConfigs;
    public static ItemType mainItemType = ItemType.NONE | ItemType.GENERAL;
    public static ItemSubtype mainItemSubtype = ItemSubtype.NONE | ItemSubtype.FISH;
     ///<json>
     /// null
     /// </json>
    public static ItemData mainItemData = null;
    public static bool itemsInThisBelongToPlayer = false;
    public static bool canAddItemsInQuestMode = true;
    public static bool hasUnderlay = false;
    public static int columns = 2;
    public static int rows = 3;
}
