using BubbetsItems.Helpers;
using RoR2;
using UnityEngine;

namespace BubbetsItems.Items
{
	public class AcidSoakedBlindfold : ItemBase
	{
		protected override void MakeTokens()
		{
			// Where III is located in ACIDSOAKEDBLINDFOLD_DESC, create a new config for spawn time please
			base.MakeTokens();
			AddToken("ACIDSOAKEDBLINDFOLD_NAME", "Acid Soaked Blindfold");
			AddToken("ACIDSOAKEDBLINDFOLD_PICKUP", "Recruit a Blind Vermin with items.");
			AddToken("ACIDSOAKEDBLINDFOLD_DESC", "Every {3} seconds, " + "summon a Blind Vermin".Style(StyleEnum.Utility) + " with " + "{1} ".Style(StyleEnum.Utility) + "Common".Style(StyleEnum.White) + " or " + "Uncommon".Style(StyleEnum.Green) + " items.");
			AddToken("ACIDSOAKEDBLINDFOLD_DESC_SIMPLE", "Every 30 seconds, " + "summon a Blind Vermin".Style(StyleEnum.Utility) + " with " + "10 ".Style(StyleEnum.Utility) + "(+5 per stack) ".Style(StyleEnum.Stack) + "Common".Style(StyleEnum.White) + " or " + "Uncommon".Style(StyleEnum.Green) + " items.");
			SimpleDescriptionToken = "ACIDSOAKEDBLINDFOLD_DESC_SIMPLE"; 
			AddToken("ACIDSOAKEDBLINDFOLD_LORE", "What is that smell?");
		}

		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			AddScalingFunction("1", "Vermin Count");
			AddScalingFunction("[a] * 5 + 5", "Item Count");
			AddScalingFunction("0.2", "Green Item Chance");
			AddScalingFunction("30", "Respawn Delay"); 
		}

        protected override void FillItemDisplayRules()
        {
            base.FillItemDisplayRules();

            AddDisplayRules(VanillaIDRS.Commando, new ItemDisplayRule()
            {
                childName = "HeadCenter",
                localPos = new Vector3(0.0002F, -0.04676F, 0.0113F),
                localAngles = new Vector3(287.2982F, 181.1993F, 178.6921F),
                localScale = new Vector3(19.99757F, 19.99757F, 19.99757F)

            });

            AddDisplayRules(ModdedIDRS.Nemmando, new ItemDisplayRule()
            {
                childName = "HeadCenter",
                localPos = new Vector3(0.0002F, -0.04676F, 0.0113F),
                localAngles = new Vector3(287.2982F, 181.1993F, 178.6921F),
                localScale = new Vector3(19.99757F, 19.99757F, 19.99757F)

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
                childName = "HeadCenter",
                localPos = new Vector3(-0.00001F, 0.00038F, -0.04013F),
                localAngles = new Vector3(320.1061F, 359.752F, 0.07683F),
                localScale = new Vector3(13.32841F, 20.5073F, 13.89857F)

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
                childName = "Head",
                localPos = new Vector3(0.00921F, 2.24607F, 0.10421F),
                localAngles = new Vector3(284.3495F, 14.88975F, 138.6814F),
                localScale = new Vector3(237.8454F, 188.0181F, 585.6845F)

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