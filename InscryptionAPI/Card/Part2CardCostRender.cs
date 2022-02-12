using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using InscryptionAPI.Helpers;
using GBC;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class Part2CardCostRender
{
    private static Dictionary<string, Texture2D> AssembledTextures = new();

    public static Texture2D GetFinalTexture(int cardCost, Texture2D artCost, bool left)
    {
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);

        List<Texture2D> list = new List<Texture2D>();
        if (cardCost <= 4)
        {
            for (int i = 0; i < cardCost; i ++)
                list.Add(artCost);
        }
        else
        {
            list.Add(artCost);
            list.Add(TextureHelper.GetImageAsTexture($"pixel_L_{cardCost}.png", typeof(Part2CardCostRender).Assembly));
        }

        int xOffset = left ? 0 : cardCost >= 10 ? 30 - 20 - artCost.width : cardCost <= 4 ? 30 - artCost.width * cardCost : 30 - 14 - artCost.width;
        return TextureHelper.CombineTextures(list, baseTexture, xOffset:xOffset, xStep:artCost.width);
    }

    public static Sprite Part2SpriteFinal(CardInfo card, bool left=true)
    {
        string costKey = $"b{card.cost}_o{card.bonesCost}_g{card.energyCost}_e{Part1CardCostRender.GemCost(card)}";

        if (AssembledTextures.ContainsKey(costKey))
		{
			if (AssembledTextures[costKey] == null)
				AssembledTextures.Remove(costKey);
			else			
				return TextureHelper.ConvertTexture(AssembledTextures[costKey], left ? TextureHelper.SpriteType.Act2CostDecalLeft : TextureHelper.SpriteType.Act2CostDecalRight);
		}

        //A list to hold the textures (important later, to combine them all)
        List<Texture2D> masterList = new List<Texture2D>();

        if (card.cost > 0)
            masterList.Add(GetFinalTexture(card.cost, TextureHelper.GetImageAsTexture("pixel_blood.png", typeof(Part2CardCostRender).Assembly), left)); 

        if (card.bonesCost > 0)
            masterList.Add(GetFinalTexture(card.bonesCost, TextureHelper.GetImageAsTexture("pixel_bone.png", typeof(Part2CardCostRender).Assembly), left));

        if (card.energyCost > 0)
            masterList.Add(GetFinalTexture(card.energyCost, TextureHelper.GetImageAsTexture("pixel_energy.png", typeof(Part2CardCostRender).Assembly), left));
        
        if (card.gemsCost.Count > 0)
        {
            List<Texture2D> gemCost = new List<Texture2D>();

            //If a card has a green mox, set the green mox
            if (card.GemsCost.Contains(GemType.Green))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_green.png", typeof(Part2CardCostRender).Assembly));

            if (card.GemsCost.Contains(GemType.Orange))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_orange.png", typeof(Part2CardCostRender).Assembly));

            if (card.GemsCost.Contains(GemType.Blue))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_blue.png", typeof(Part2CardCostRender).Assembly));

            Texture2D gemBaseTexture = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);

            if (!left)
                gemCost.Reverse();

            masterList.Add(TextureHelper.CombineTextures(gemCost, gemBaseTexture, xOffset:left ? 0 : 30 - 7 * gemCost.Count, xStep:7));
        }

        while (masterList.Count < 4)
            masterList.Add(null);

        //Combine all the textures from the list into one texture
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("pixel_base.png", typeof(Part2CardCostRender).Assembly);
        Texture2D finalTexture = TextureHelper.CombineTextures(masterList, baseTexture, yStep:8);

        AssembledTextures.Add(costKey, finalTexture);

        //Convert the final texture to a sprite
        Sprite finalSprite = TextureHelper.ConvertTexture(finalTexture, left ? TextureHelper.SpriteType.Act2CostDecalLeft : TextureHelper.SpriteType.Act2CostDecalRight);
        return finalSprite;
    }

    [HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.GetCostSpriteForCard))]
	[HarmonyPrefix]
	public static bool Part2CardCostDisplayerPatch(ref Sprite __result, ref CardInfo card, ref CardDisplayer __instance)
	{	
		//Make sure we are in Leshy's Cabin
		if (__instance is PixelCardDisplayer) 
		{ 
			/// Set the results as the new sprite
			__result = Part2SpriteFinal(card, !InscryptionAPIPlugin.rightAct2Cost.Value);
			return false;
		}

		return true;
	}
}