﻿using BepInEx.Configuration;
using BubbetsItems.Helpers;
using HarmonyLib;
using R2API;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;

namespace BubbetsItems.Items.BarrierItems
{
	public class CeremonialProbe : ItemBase
	{
		public static ConfigEntry<bool> RegenOnStage;

		protected override void MakeTokens()
		{
			base.MakeTokens();
			AddToken("CEREMONIALPROBE_NAME", "Ceremonial Probe");
			AddToken("CEREMONIALPROBE_DESC", "Falling bellow " + "{0:0%} health ".Style(StyleEnum.Health) + " consumes this item and gives you " + "{1:0%} temporary barrier. ".Style(StyleEnum.Heal));
			AddToken("CEREMONIALPROBE_DESC_SIMPLE", "Falling bellow " + "35% health ".Style(StyleEnum.Health) + " consumes this item and gives you " + "75% temporary barrier. ".Style(StyleEnum.Heal));
			SimpleDescriptionToken = "CEREMONIALPROBE_DESC_SIMPLE";
			AddToken("CEREMONIALPROBE_PICKUP", "Get barrier at low health.");
			AddToken("CEREMONIALPROBE_LORE", "");
		}

		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			AddScalingFunction("0.35", "Health Threshold");
			AddScalingFunction("0.75", "Barrier Add Percent");
			RegenOnStage = sharedInfo.ConfigFile.Bind(ConfigCategoriesEnum.General, "Ceremonial Probe Regen On Stage",
				true, "Should ceremonial probe regenerate on stage change.");
		}

		public override void MakeRiskOfOptions()
		{
			base.MakeRiskOfOptions();
			ModSettingsManager.AddOption(new CheckBoxOption(RegenOnStage));
		}

		protected override void MakeBehaviours()
		{
			base.MakeBehaviours();
			GlobalEventManager.onServerDamageDealt += OnHit;
		}
		protected override void DestroyBehaviours()
		{
			base.DestroyBehaviours();
			GlobalEventManager.onServerDamageDealt -= OnHit;
		}
		private void OnHit(DamageReport obj)
		{
			if (!obj.victim) return;
			var body = obj.victim.body;
			if (!body) return;
			DoEffect(body);
		}

		public static void DoEffect(CharacterBody body)
		{
			var inst = GetInstance<CeremonialProbe>();
			if (inst == null) return;
			var inv = body.inventory;
			if (!inv) return;
			var amount = inv.GetItemCount(inst.ItemDef);
			if (amount <= 0) return;
			if (body.healthComponent.combinedHealth / body.healthComponent.fullCombinedHealth <
			    inst.scalingInfos[0].ScalingFunction(amount))
			{
				body.healthComponent.AddBarrier(body.healthComponent.fullCombinedHealth *
				                                inst.scalingInfos[1].ScalingFunction(amount));
				var broke = GetInstance<BrokenCeremonialProbe>()!.ItemDef;
				body.inventory.RemoveItem(inst.ItemDef);
				body.inventory.GiveItem(broke);
				CharacterMasterNotificationQueue.SendTransformNotification(body.master, inst.ItemDef.itemIndex, broke.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(CharacterMaster), nameof(CharacterMaster.OnServerStageBegin))]
		public static void RegenItem(CharacterMaster __instance)
		{
			if (!RegenOnStage.Value) return;
			var broke = GetInstance<BrokenCeremonialProbe>()!.ItemDef;
			var regular = GetInstance<CeremonialProbe>()!.ItemDef;
			var itemCount = __instance.inventory.GetItemCount(broke);
			if (itemCount <= 0) return;
			__instance.inventory.RemoveItem(broke, itemCount);
			__instance.inventory.GiveItem(regular, itemCount);
			CharacterMasterNotificationQueue.SendTransformNotification(__instance, broke.itemIndex, regular.itemIndex, CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
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