﻿using System.Collections.Generic;
using BepInEx.Configuration;
using BubbetsItems.Helpers;
using HarmonyLib;
using InLobbyConfig.Fields;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using UnityEngine;

namespace BubbetsItems.Items
{
    public class RepulsionPlateMk2 : ItemBase
    {
        public static ConfigEntry<bool> _reductionOnTrue;
        private static ScalingInfo _reductionScalingConfig;
        private static ScalingInfo _armorScalingConfig;

        protected override void MakeTokens()
        {
            base.MakeTokens();
            AddToken("REPULSION_ARMOR_MK2_NAME", "Repulsion Armor Plate Mk2");
            //AddToken("REPULSION_ARMOR_MK2_DESC", "Placeholder, swapped out with config value at runtime."); //pickup);

            // this mess #,###;#,###;0 is responsible for throwing away the negative sign when in the tooltip from the scaling function
            AddToken("REPULSION_ARMOR_MK2_DESC_REDUCTION", "Reduce all " + "incoming damage ".Style(StyleEnum.Damage) + "by " + "{0:#,###;#,###;0}".Style(StyleEnum.Damage) + ". Cannot be reduced below " + "1".Style(StyleEnum.Damage) + ". Scales with how much " + "Repulsion Armor Plates ".Style(StyleEnum.Utility) + "you have.");
            AddToken("REPULSION_ARMOR_MK2_DESC_ARMOR", "Increase armor ".Style(StyleEnum.Heal) + "by " + "{0} ".Style(StyleEnum.Heal) + ". Scales with how much " + "Repulsion Armor Plates ".Style(StyleEnum.Utility) + "you have.");
            
            AddToken("REPULSION_ARMOR_MK2_DESC_REDUCTION_SIMPLE", "Reduce all " + "incoming damage ".Style(StyleEnum.Damage) + "by " + "20 ".Style(StyleEnum.Damage) + "(+amount of repulsion plates per stack)".Style(StyleEnum.Stack) + ". Cannot be reduced below " + "1".Style(StyleEnum.Damage) + ". Scales with how much " + "Repulsion Armor Plates ".Style(StyleEnum.Utility) + "you have.");
            AddToken("REPULSION_ARMOR_MK2_DESC_ARMOR_SIMPLE", "Increase armor ".Style(StyleEnum.Heal) + "by " + "20 ".Style(StyleEnum.Heal) + "(+amount of repulsion plates per stack)".Style(StyleEnum.Stack) + ". Scales with how much " + "Repulsion Armor Plates ".Style(StyleEnum.Utility) + "you have.");

            // <style=cIsDamage>incoming damage</style> by <style=cIsDamage>5<style=cStack> (+5 per stack)</style></style>
            AddToken("REPULSION_ARMOR_MK2_PICKUP", "Receive damage reduction from all attacks depending on each " + "Repulsion Plate".Style(StyleEnum.Utility) + ".");
            AddToken("REPULSION_ARMOR_MK2_LORE", @"Order: Experimental Repulsion Armour Augments - Mk. 2
Tracking number: 07 **
Estimated Delivery: 10/23/2058
Shipping Method: Secure, High Priority
Shipping Address: System Police Station 13/ Port of Marv, Ganymede
Shipping Details:

The order contains cutting-edge experimental technology aimed at reducing risk of harm for the users even in the most harsh of conditions. On top of providing protection Mk. 2's smart nano-bot network enhances already existing protection that the user has installed. This kind of equipment might prove highly necessary as crime rates had seen a rise in the Port of Marv area around station 13, higher risk of injury for stationing officers necessitates an increase in measures used to ensure their safety.

The cost of purchase and production associated with Mk2 is considerably higher than that of its prior iterations, however the considerable step-up in efficiency covers for the costs, as drastic as they might be.");
        }
        protected override void MakeConfigs()
        {
            base.MakeConfigs();
            _reductionOnTrue = sharedInfo.ConfigFile!.Bind(ConfigCategoriesEnum.General, "Reduction On True", true,  "Makes the item behave more like mk1 and give a flat reduction in damage taken if set to true.");
            _reductionOnTrue.SettingChanged += (_, _) => UpdateScalingFunction(); 
            
            var name = GetType().Name;;
            AddScalingFunction("[d] - (20 + [p] * (4 + [a]))", name + " Reduction", "[a] = amount, [p] = plate amount, [d] = damage");
            AddScalingFunction("20 + [p] * (4 + [a])", name + " Armor", "[a] = amount, [p] = plate amount");
            _reductionScalingConfig = scalingInfos[0];
            _armorScalingConfig = scalingInfos[1];
            
            //_reductionScalingConfig = configFile.Bind(ConfigCategoriesEnum.BalancingFunctions, name + " Reduction", "[d] - (20 + [p] * (4 + [a]))", "Scaling function for item. ;");
            //_armorScalingConfig = configFile.Bind(ConfigCategoriesEnum.BalancingFunctions, name + " Armor", "", "Scaling function for item. ;");
            UpdateScalingFunction();
        }

        public override void MakeRiskOfOptions()
        {
            base.MakeRiskOfOptions();
            ModSettingsManager.AddOption(new CheckBoxOption(_reductionOnTrue));
        }

        private void UpdateScalingFunction()
        {
            scalingInfos.Clear();
            scalingInfos.Add(_reductionOnTrue.Value ? _reductionScalingConfig : _armorScalingConfig);
        }
        public override string GetFormattedDescription(Inventory? inventory, string? token = null, bool forceHideExtended = false)
        {
            //ItemDef.descriptionToken = _reductionOnTrue.Value ? "BUB_REPULSION_ARMOR_MK2_DESC_REDUCTION" :  "BUB_REPULSION_ARMOR_MK2_DESC_ARMOR"; Cannot do this, it breaks the token matching from the tooltip patch
            var context = scalingInfos[0].WorkingContext;
            context.p = inventory?.GetItemCount(RoR2Content.Items.ArmorPlate) ?? 0;
            context.d = 0f;

            var tokenChoice = _reductionOnTrue.Value
                ? "BUB_REPULSION_ARMOR_MK2_DESC_REDUCTION"
                : "BUB_REPULSION_ARMOR_MK2_DESC_ARMOR";
            
            SimpleDescriptionToken = _reductionOnTrue.Value
                ? "REPULSION_ARMOR_MK2_DESC_REDUCTION_SIMPLE"
                : "REPULSION_ARMOR_MK2_DESC_ARMOR_SIMPLE";
            
            return base.GetFormattedDescription(inventory, tokenChoice, forceHideExtended);
        }

        public override void MakeInLobbyConfig(Dictionary<ConfigCategoriesEnum, List<object>> scalingFunctions)
        {
            base.MakeInLobbyConfig(scalingFunctions);
            scalingFunctions[ConfigCategoriesEnum.BalancingFunctions].Add(ConfigFieldUtilities.CreateFromBepInExConfigEntry(_reductionOnTrue));
        }

        /*
        public void UpdateScalingFunction()
        {
            scalingFunction = _reductionOnTrue.Value ? new Expression(_reductionScalingConfig.Value).ToLambda<ExpressionContext, float>() : new Expression(_armorScalingConfig.Value).ToLambda<ExpressionContext, float>();
        }

        public override float GraphScalingFunction(int itemCount)
        {
            return _reductionOnTrue.Value ? -ScalingFunction(itemCount,1) : ScalingFunction(itemCount);
        }*/

        protected override void MakeBehaviours()
        {
            base.MakeBehaviours();
            RecalculateStatsAPI.GetStatCoefficients += RecalcStats;
        }

        protected override void DestroyBehaviours()
        {
            base.DestroyBehaviours();
            RecalculateStatsAPI.GetStatCoefficients -= RecalcStats;
        }

        public static void RecalcStats(CharacterBody __instance, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (_reductionOnTrue.Value) return;
            var inv = __instance.inventory;
            if (!inv) return;
            var repulsionPlateMk2 = GetInstance<RepulsionPlateMk2>();
            var amount = inv.GetItemCount(repulsionPlateMk2.ItemDef);
            if (amount <= 0) return;
            
            var plateAmount = inv.GetItemCount(RoR2Content.Items.ArmorPlate);
            // 20 + inv.GetItemCount(RoR2Content.Items.ArmorPlate) * (4 + amount);
            var info = repulsionPlateMk2.scalingInfos[0];
            info.WorkingContext.p = plateAmount; 
            args.armorAdd += info.ScalingFunction(amount);
        }
        
        public static bool DoMk2ArmorPlates(HealthComponent hc, ref float damage)
        {
            if (hc == null) return false;
            if (hc.body == null) return false;
            if (hc.body.inventory == null) return false;
            var repulsionPlateMk2 = GetInstance<RepulsionPlateMk2>();
            var amount = hc.body.inventory.GetItemCount(repulsionPlateMk2.ItemDef);
            if (amount <= 0) return false;
            var plateAmount = hc.body.inventory.GetItemCount(RoR2Content.Items.ArmorPlate);
            //damage = Mathf.Max(1f, damage - (20 + plateAmount * (4 + amount)));
            var info = repulsionPlateMk2.scalingInfos[0];
            info.WorkingContext.p = plateAmount;
            info.WorkingContext.d = damage;
            damage = Mathf.Max(1f, info.ScalingFunction(amount));
            return true;
        }

        private delegate bool ArmorPlateDele(HealthComponent hc, ref float damage);

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamage))]
        public static void TakeDamageHook(ILContext il)
        {
            if (!_reductionOnTrue.Value) return;
            var c = new ILCursor(il);
            ILLabel jumpInstruction = null;
            int damageNum = -1;
            c.GotoNext(
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(out damageNum),
                x => x.MatchLdcR4(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>("itemCounts"),
                x => x.OpCode == OpCodes.Ldfld && ((FieldReference) x.Operand).Name == "armorPlate"
            );
            c.GotoPrev(
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>("itemCounts"),
                x => x.OpCode == OpCodes.Ldfld && ((FieldReference) x.Operand).Name == "armorPlate",
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out jumpInstruction)
            );
            if (damageNum == -1 || jumpInstruction == null) return;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloca, damageNum);
            c.EmitDelegate<ArmorPlateDele>(DoMk2ArmorPlates);
            c.Emit(OpCodes.Brfalse, jumpInstruction.Target);
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