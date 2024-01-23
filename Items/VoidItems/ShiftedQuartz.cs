using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BubbetsItems.Helpers;
using BubbetsItems.ItemBehaviors;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using UnityEngine;

namespace BubbetsItems.Items
{
	public class ShiftedQuartz : ItemBase
	{
		public static ConfigEntry<bool> visualOnlyForAuthority;
		public static ConfigEntry<float> visualTransparency;

		protected override void MakeTokens()
		{
			base.MakeTokens();
			AddToken("SHIFTEDQUARTZ_NAME", "Shifted Quartz");
			var convert = "Corrupts all Focus Crystals".Style(StyleEnum.Void) + ".";
			AddToken("SHIFTEDQUARTZ_CONVERT", convert);
			AddToken("SHIFTEDQUARTZ_PICKUP", "Deal bonus damage if there aren't nearby enemies. " + "Corrupts all Focus Crystals".Style(StyleEnum.Void) + ". " + convert);
			AddToken("SHIFTEDQUARTZ_DESC", "Increase damage dealt by " + "{1:0%} ".Style(StyleEnum.Damage) + "when there are no enemies within " + "{0}m ".Style(StyleEnum.Damage) + "of you. ");
			AddToken("SHIFTEDQUARTZ_DESC_SIMPLE", "Increase damage dealt by " + "15% ".Style(StyleEnum.Damage) + "(+15% per stack) ".Style(StyleEnum.Stack) + "when there are no enemies within " + "18m ".Style(StyleEnum.Damage) + "of you. ");
			SimpleDescriptionToken = "SHIFTEDQUARTZ_DESC_SIMPLE";
			AddToken("SHIFTEDQUARTZ_LORE", "");
		}
		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			AddScalingFunction("18", "Distance", oldDefault: "20");
			AddScalingFunction("[a] * 0.15", "Damage", oldDefault: "[a] * 0.2");
		}

		public override void MakeZioOptions()
		{
			visualOnlyForAuthority = sharedInfo.ConfigFile!.Bind(ConfigCategoriesEnum.General,
				"Shifted quartz visual only for authority", false,
				"Should shifted quartz visual effect only show for the player who has the item", networked: false);
			visualTransparency = sharedInfo.ConfigFile.Bind(ConfigCategoriesEnum.General, "Shifted quartz inside transparency",
				0.15f, "The transparency of the dome when enemies are inside it.", networked: false);
		}

		public override void MakeZioRiskOfOptions()
		{
			base.MakeZioRiskOfOptions();
			ModSettingsManager.AddOption(new CheckBoxOption(visualOnlyForAuthority));
			ModSettingsManager.AddOption(new SliderOption(visualTransparency, new SliderConfig {min = 0, max = 1, formatString = "{0:0.00%}"}));
		}

		protected override void FillVoidConversions(List<ItemDef.Pair> pairs)
		{
			AddVoidPairing("NearbyDamageBonus");
		}
		
		[HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamage))]
		public static void IlTakeDamage(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(MoveType.Before, x => x.OpCode == OpCodes.Ldsfld && (x.Operand as FieldReference)?.Name == nameof(RoR2Content.Items.NearbyDamageBonus),
				x => x.MatchCallOrCallvirt(out _),
				x => x.MatchStloc(out _));
			var where = c.Index;
			int num2 = -1;
			c.GotoNext(x => x.MatchLdloc(out num2),
				x => x.MatchLdcR4(1f),
				x => x.MatchLdloc(out _));
			c.Index = where;
			c.Emit(OpCodes.Ldloc_1); // Body; 0 is master
			c.Emit(OpCodes.Ldloc, num2);
			c.EmitDelegate<Func<CharacterBody, float, float>>((body, amount) =>
			{
				var instance = GetInstance<ShiftedQuartz>();
				var count = body.inventory.GetItemCount(instance.ItemDef);
				if (count <= 0) return amount;
				var inside = body.GetComponent<ShiftedQuartzBehavior>().inside; // TODO this might not exist in scope and may throw errors in multiplayer
				if (!inside)
					amount *= 1f + instance.scalingInfos[1].ScalingFunction(count); // 1f + count * 0.2f
				return amount;
			});
			c.Emit(OpCodes.Stloc, num2);
		}

        protected override void FillItemDisplayRules()
        {
            base.FillItemDisplayRules();

            AddDisplayRules(VanillaIDRS.Commando, new ItemDisplayRule()
            {
                childName = "HandR",
                localPos = new Vector3(0.01427F, 0.02265F, -0.00877F),
                localAngles = new Vector3(281.4024F, 289.9227F, 312.8879F),
                localScale = new Vector3(0.08377F, 0.08377F, 0.08377F)

            });

            AddDisplayRules(ModdedIDRS.Nemmando, new ItemDisplayRule()
            {
                childName = "HandR",
                localPos = new Vector3(0.01427F, 0.02265F, -0.00877F),
                localAngles = new Vector3(281.4024F, 289.9227F, 312.8879F),
                localScale = new Vector3(0.08377F, 0.08377F, 0.08377F)

            });

            AddDisplayRules(VanillaIDRS.Huntress, new ItemDisplayRule()
            {
                childName = "BowBase",
                localPos = new Vector3(0.0039F, -0.08401F, -0.0193F),
                localAngles = new Vector3(270.0198F, 0F, 0F),
                localScale = new Vector3(0.06104F, 0.06104F, 0.06104F)

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
                childName = "HandL",
                localPos = new Vector3(-0.15644F, -1.13176F, 0.14458F),
                localAngles = new Vector3(319.1296F, 182.6829F, 328.8047F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)

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
                childName = "Chest",
                localPos = new Vector3(0.02676F, -0.20942F, 0.10917F),
                localAngles = new Vector3(63.9817F, 17.22919F, 46.84008F),
                localScale = new Vector3(0.17493F, 0.17493F, 0.17493F)

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