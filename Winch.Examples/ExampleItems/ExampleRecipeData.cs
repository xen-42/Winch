﻿using UnityEngine;
using UnityEngine.Localization;
using Winch.Core;
using Winch.Data.Recipe;
using Winch.Util;

namespace ExampleItems;

public class ExampleRecipeData : ModdedRecipeData
{
    public override LocalizedString GetItemNameKey() => LocalizationUtil.CreateStringsReference("exampleitems.recipe.name");
    public override LocalizedString GetItemDescriptionKey() => LocalizationUtil.CreateStringsReference("exampleitems.recipe.description");
    public override Color GetItemNameColor() => Color.blue;
    public override Color GetItemDescriptionColor() => Color.yellow;
    public override Sprite GetSprite() => TextureUtil.GetSprite("RepairIcon");
    public override Sprite GetTooltipIcon() => TextureUtil.GetSprite("JunkIcon");

    public override int GetQuantityProduced() => 1;

    public override int GetWidth() => 2;
    public override int GetHeight() => 2;

    public override bool IsOneTimeAndAlreadyOwned()
    {
        WinchCore.Log.Warn("IsOneTimeAndAlreadyOwned");
        try
        {
            return ExampleSaveBehaviour.Instance.SaveData.recipeCrafted;
        }
        catch (System.Exception ex)
        {
            WinchCore.Log.Error(ex);
            return false;
        }
    }

    public override void OnRecipeCompleted()
    {
        WinchCore.Log.Warn("OnRecipeCompleted");
        try
        {
            ExampleSaveBehaviour.Instance.SaveData.recipeCrafted = true;
        }
        catch (System.Exception ex)
        {
            WinchCore.Log.Error(ex);
        }
    }

    public ExampleRecipeData()
    {
        recipeId = "exampleitems.recipe";
        questGridConfig = "exampleitems.recipe";
        cost = 1;
        quantityProduced = 1;
        researchRequired = 1;
        onRecipeShownDialogueNodeName = "ExampleItems_Steve_RecipeShown";
        onRecipeBuiltDialogueNodeName = "ExampleItems_Steve_RecipeBuilt";
        name = recipeId;
        this.DontDestroyOnLoad();
        Register();
    }
}
