﻿using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BubbetsItems.Helpers;
using HarmonyLib;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using RoR2.Items;

namespace BubbetsItems.Items
{
	public class VoidJunkToScrapTier1 : ItemBase
	{
		public static ConfigEntry<bool>? CanConsumeLastStack;
		private static CostTypeDef.IsAffordableDelegate? _oldCan;
		private static CostTypeDef.PayCostDelegate? _oldCost;
		protected override void MakeTokens()
		{
			base.MakeTokens();
			AddToken("VOIDJUNKTOSCRAPTIER1_NAME", "Void Scrap");
			AddToken("VOIDJUNKTOSCRAPTIER1_PICKUP", "Prioritized when used with " + "Common ".Style(StyleEnum.White) + "3D Printers. " + "Corrupts all Broken items".Style(StyleEnum.Void) + ".");
			AddToken("VOIDJUNKTOSCRAPTIER1_DESC", "Does nothing. " + "Prioritized when used with " + "Common ".Style(StyleEnum.White) + "3D Printers. {0}" + "Corrupts all Broken items".Style(StyleEnum.Void) + ".");
			AddToken("VOIDJUNKTOSCRAPTIER1_LORE", "");
		}
		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			CanConsumeLastStack = sharedInfo.ConfigFile!.Bind(ConfigCategoriesEnum.General, "Void Scrap Consume Last Stack", false, "Should the void scrap consume the last stack when being used for scrap.");
		}

		public override void MakeRiskOfOptions()
		{
			base.MakeRiskOfOptions();
			ModSettingsManager.AddOption(new CheckBoxOption(CanConsumeLastStack));
		}

		public override string GetFormattedDescription(Inventory? inventory, string? token = null, bool forceHideExtended = false)
		{
			return Language.GetStringFormatted(ItemDef.descriptionToken, !CanConsumeLastStack!.Value ? "Cannot consume the last stack. " : "");
		}
		protected override void FillVoidConversions(List<ItemDef.Pair> pairs)
		{
			AddVoidPairing("FragileDamageBonusConsumed HealingPotionConsumed ExtraLifeVoidConsumed ExtraLifeConsumed MysticsItems_LimitedArmorBroken DRUMSTICKCONSUMED ITEM_CARTRIDGECONSUMED ITEM_SINGULARITYCONSUMED ITEM_BROKEN_MESS ConsumedGlassShield_ItemDef TreasureCacheConsumed TreasureCacheVoidConsumed", oldDefault: "FragileDamageBonusConsumed HealingPotionConsumed ExtraLifeVoidConsumed ExtraLifeConsumed");
		}
		[HarmonyPostfix, HarmonyPatch(typeof(CostTypeCatalog), nameof(CostTypeCatalog.Init))]
		public static void FixBuying()
		{
			try
			{
				var def = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.WhiteItem);
				_oldCan = def.isAffordable;
				def.isAffordable = IsAffordable;
				_oldCost = def.payCost;
				def.payCost = PayCost;
			}
			catch (Exception e)
			{
				BubbetsItemsPlugin.Log.LogError(e);
			}
		}

		public static void PayCost(CostTypeDef typeDef, CostTypeDef.PayCostContext context)
		{
			if (typeDef.itemTier != ItemTier.Tier1)
			{
				_oldCost!(typeDef, context);
				return;
			}

			try
			{
				var inv = context.activatorBody.inventory;

				var highestPriority = new WeightedSelection<ItemIndex>();
				var higherPriority = new WeightedSelection<ItemIndex>();
				var highPriority = new WeightedSelection<ItemIndex>();
				var normalPriority = new WeightedSelection<ItemIndex>();

				var voidJunkToScrapTier1 = GetInstance<VoidJunkToScrapTier1>();
				var voidAmount = Math.Max(0, inv.GetItemCount(voidJunkToScrapTier1!.ItemDef) - (CanConsumeLastStack!.Value ? 0 : 1));
				if (voidAmount > 0) highestPriority.AddChoice(voidJunkToScrapTier1.ItemDef.itemIndex, voidAmount);

				foreach (var itemIndex in ItemCatalog.tier1ItemList)
				{
					if (itemIndex == context.avoidedItemIndex) continue;
					var count = inv.GetItemCount(itemIndex);
					if (count > 0)
					{
						var itemDef = ItemCatalog.GetItemDef(itemIndex);
						(itemDef.ContainsTag(ItemTag.PriorityScrap) ? higherPriority : itemDef.ContainsTag(ItemTag.Scrap) ? highPriority : normalPriority).AddChoice(itemIndex, count);
					}
				}

				var itemsToTake = new List<ItemIndex>();

				TakeFromWeightedSelection(highestPriority, ref context, ref itemsToTake);
				TakeFromWeightedSelection(higherPriority, ref context, ref itemsToTake);
				TakeFromWeightedSelection(highPriority, ref context, ref itemsToTake);
				TakeFromWeightedSelection(normalPriority, ref context, ref itemsToTake);

				for (var i = itemsToTake.Count; i < context.cost; i++) itemsToTake.Add(context.avoidedItemIndex);

				context.results.itemsTaken = itemsToTake;
				foreach (var itemIndex in itemsToTake) inv.RemoveItem(itemIndex);
				MultiShopCardUtils.OnNonMoneyPurchase(context);
			}
			catch (Exception e)
			{
				BubbetsItemsPlugin.Log.LogError(e);
			}
		}

		public static bool IsAffordable(CostTypeDef typeDef, CostTypeDef.IsAffordableContext context)
		{
			if (_oldCan!(typeDef, context)) return true;
			try
			{
				if (typeDef.itemTier != ItemTier.Tier1) return false;
				var inv = context.activator.GetComponent<CharacterBody>().inventory;
				var voidJunkToScrapTier1 = GetInstance<VoidJunkToScrapTier1>();
				var voidAmount = Math.Max(0, inv.GetItemCount(voidJunkToScrapTier1!.ItemDef) - (CanConsumeLastStack!.Value ? 0 : 1));
				return inv.GetTotalItemCountOfTier(ItemTier.Tier1) + voidAmount >= context.cost;
			}
			catch (Exception e)
			{
				BubbetsItemsPlugin.Log.LogError(e);
				return false;
			}
		}

		public static void TakeFromWeightedSelection(WeightedSelection<ItemIndex> weightedSelection, ref CostTypeDef.PayCostContext context, ref List<ItemIndex> itemsToTake)
		{
			while (weightedSelection.Count > 0 && itemsToTake.Count < context.cost)
			{
				var choiceIndex = weightedSelection.EvaluateToChoiceIndex(context.rng.nextNormalizedFloat);
				var choice = weightedSelection.GetChoice(choiceIndex);
				var value = choice.value;
				var num = (int)choice.weight;
				num--;
				if (num <= 0)
				{
					weightedSelection.RemoveChoice(choiceIndex);
				}
				else
				{
					weightedSelection.ModifyChoiceWeight(choiceIndex, num);
				}
				itemsToTake.Add(value);
			}
		}

        protected override void FillItemDisplayRules()
        {
            base.FillItemDisplayRules();

            AddDisplayRules(VanillaIDRS.Commando, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Nemmando, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Huntress, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Bandit, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Mult, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Hand, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Engineer, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Enforcer, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.NemesisEnforcer, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Artificer, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Mercenary, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.NemMerc, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Paladin, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Rex, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Loader, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Acrid, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Captain, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Executioner, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.Chirr, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.RailGunner, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.VoidFiend, new ItemDisplayRule()
            {

            });


            AddDisplayRules(ModdedIDRS.ReinSniper, new ItemDisplayRule()
            {

            });


            AddDisplayRules(ModdedIDRS.Miner, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.CHEF, new ItemDisplayRule()
            {

            });

            AddDisplayRules(ModdedIDRS.BanditReloaded, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Scavenger, new ItemDisplayRule()
            {

            });
        }

    }
}