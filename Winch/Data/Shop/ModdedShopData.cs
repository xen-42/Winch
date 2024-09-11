﻿using System.Linq;

namespace Winch.Data.Shop;

public class ModdedShopData : ShopData
{
    public string id = string.Empty;

    public GridKey gridKey = GridKey.NONE;

    internal void Populate()
    {
        foreach (var itemData in alwaysInStock.Concat(phaseLinkedShopData.SelectMany(pl => pl.itemData)).Concat(dialogueLinkedShopData.SelectMany(pl => pl.itemData)))
        {
            if (itemData is ModdedShopItemData moddedItemData)
                moddedItemData.Populate();
        }
    }

    public static implicit operator ShopRestocker.ShopDataGridConfig(ModdedShopData shopData) => new ModdedShopDataGridConfig(shopData);
}
