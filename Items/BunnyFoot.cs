using System;
using System.Reflection;
using BepInEx.Configuration;
using BubbetsItems.Helpers;
using BubbetsItems.ItemBehaviors;
using EntityStates;
using EntityStates.Assassin2;
using EntityStates.Merc;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace BubbetsItems.Items
{
	public class BunnyFoot : ItemBase
	{
		public static ConfigEntry<bool> addAirControl;
		public static ConfigEntry<bool> isLunar;
		private Sprite greenIcon;
		private Sprite lunarIcon;

		protected override void MakeTokens()
		{
			base.MakeTokens();
			AddToken("BUNNYFOOT_NAME", "Bunny Foot");
			AddToken("BUNNYFOOT_DESC", "You gain the ability to bunny hop. Air control: {0}, Jump control: {3}");
			AddToken("BUNNYFOOT_DESC_SIMPLE", "Gain the ability to bunny hop, increasing air control by " + "150% ".Style(StyleEnum.Utility) + "(+150% per stack) ".Style(StyleEnum.Stack) + "and jump control by " + "50% ".Style(StyleEnum.Utility) + "(+50% per stack)".Style(StyleEnum.Stack) + ".");
			SimpleDescriptionToken = "BUNNYFOOT_DESC_SIMPLE";
			AddToken("BUNNYFOOT_PICKUP", "Your little feets start quivering.");
			AddToken("BUNNYFOOT_LORE", "haha source go brrrr\n\n\n\n\n\nIf you complain about this item being bad you're just outing yourself as bad at videogames.");
		}

		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			AddScalingFunction("[a] * 1.5", "Air Control");
			AddScalingFunction("0.15", "On Ground Mercy");
			AddScalingFunction("1", "Jump velocity retention");
			AddScalingFunction("[a] * 0.5", "Jump Control");
			AddScalingFunction("3", "Auto Jump Requirement");
			AddScalingFunction("0.25", "Merc Dash Exit Mult");
			addAirControl = sharedInfo.ConfigFile.Bind("General", "Bunny Foot Add Air Control", false, "Add a bit of vanilla air control when at low speeds with bunny foot.");
			isLunar = sharedInfo.ConfigFile.Bind("General", "Bunny Foot Is Lunar", false, "Makes bunny foot a lunar item.");
			isLunar.SettingChanged += IsLunarOnSettingChanged;
		}

		private void IsLunarOnSettingChanged(object o, EventArgs e)
		{
			ItemDef.tier = isLunar.Value ? ItemTier.Lunar : ItemTier.Tier2;
			ItemDef.pickupIconSprite = isLunar.Value ? lunarIcon : greenIcon;
		}

		protected override void FillDefsFromContentPack()
		{
			base.FillDefsFromContentPack();
			greenIcon = ItemDef.pickupIconSprite;
			lunarIcon = BubbetsItemsPlugin.AssetBundle.LoadAsset<Sprite>("BunnyFootLunar");
			IsLunarOnSettingChanged(null, null);
		}

		public override void MakeRiskOfOptions()
		{
			base.MakeRiskOfOptions();
			ModSettingsManager.AddOption(new CheckBoxOption(addAirControl));
			ModSettingsManager.AddOption(new CheckBoxOption(isLunar));
		}

		[HarmonyILManipulator, HarmonyPatch(typeof(ProjectileGrappleController.GripState), nameof(ProjectileGrappleController.GripState.FixedUpdateBehavior))]
		public static void FixGrapple(ILContext il)
		{
			// enable air control after grapple
			var c = new ILCursor(il);
			c.GotoNext(
				MoveType.After,
				x => x.MatchCall<Vector3>("op_Multiply"),
				x => x.MatchLdcI4(1),
				x => x.MatchLdcI4(1)
			);
			//c.Index--;
			//c.Remove();
			//c.Emit(OpCodes.Ldc_I4_0);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<bool, ProjectileGrappleController.GripState, bool>>((b, grip) => (!grip.owner.characterBody || grip.owner.characterBody.inventory.GetItemCount(GetInstance<BunnyFoot>().ItemDef) <= 0) && b);
		}

		[HarmonyILManipulator, HarmonyPatch(typeof(Assaulter2), nameof(Assaulter2.OnEnter)), HarmonyPatch(typeof(Assaulter2), nameof(Assaulter2.OnExit)), HarmonyPatch(typeof(FocusedAssaultDash), nameof(FocusedAssaultDash.OnExit)), HarmonyPatch(typeof(EvisDash), nameof(EvisDash.OnExit))]//, HarmonyPatch(typeof(WhirlwindBase), nameof(WhirlwindBase.FixedUpdate))]
		public static void FixAssulter2Dash(ILContext il, MethodBase __originalMethod)
		{
			var c = new ILCursor(il);
			c.GotoNext(x => x.MatchStfld<CharacterMotor>("velocity"));
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Vector3, BaseSkillState, Vector3>>((vector3, assaulter2) =>
			{
				var count = assaulter2.characterBody.inventory.GetItemCount(GetInstance<BunnyFoot>()?.ItemDef);
				if (count <= 0) return vector3;
				if (__originalMethod.Name != "OnExit") return ((IPhysMotor) assaulter2.characterMotor).velocity;
				var outputVelocity = (Vector3) (__originalMethod.DeclaringType?.GetField("dashVector", (BindingFlags) (-1))?.GetValue(assaulter2) ?? Vector3.zero) * (float) (__originalMethod.DeclaringType?.GetField("speedCoefficient")?.GetValue(assaulter2) ?? 0f) * assaulter2.moveSpeedStat * GetInstance<BunnyFoot>().scalingInfos[5].ScalingFunction(count);
				var inputVelocity = ((IPhysMotor) assaulter2.characterMotor).velocity;
				return outputVelocity.normalized * Mathf.Sqrt(Mathf.Max(inputVelocity.sqrMagnitude, outputVelocity.sqrMagnitude));
			});
		}
		//[HarmonyILManipulator, HarmonyPatch(typeof(DashStrike), nameof(DashStrike.FixedUpdate))]
		public static void FixMercDash(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(x => x.MatchCallOrCallvirt<CharacterMotor>("set_" + nameof(CharacterMotor.moveDirection)));
			var index = c.Index;
			c.GotoPrev(MoveType.After, x => x.MatchCallOrCallvirt<EntityState>("get_" + nameof(EntityState.characterMotor)));
			c.Emit(OpCodes.Dup);
			c.Index = index + 1; // for some reason this was emitting before the mult which is weird
			c.EmitDelegate<Func<CharacterMotor, Vector3, Vector3>>((motor, input) =>
			{
				if (motor.body.inventory.GetItemCount(GetInstance<BunnyFoot>()?.ItemDef) <= 0) return input;
				return input.normalized * Mathf.Sqrt(Mathf.Max(input.sqrMagnitude, motor.moveDirection.sqrMagnitude));
			});
			BubbetsItemsPlugin.Log.LogInfo(il);
		}

		[HarmonyILManipulator, HarmonyPatch(typeof(GenericCharacterMain), nameof(GenericCharacterMain.ApplyJumpVelocity))]
		public static void FixJump(ILContext il)
		{
			// Clamp the jump speed add to not add speed when strafing over the speedlimit
			var c = new ILCursor(il);
			
			// if (vault)
			c.GotoNext(x => x.MatchStfld<CharacterMotor>("velocity"));
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<Func<Vector3, CharacterBody, Vector3>>(DoJumpFix);
			c.Index++;
			
			// if (vault) else
			c.GotoNext(
				x => x.MatchStfld<CharacterMotor>("velocity")
			);
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<Func<Vector3, CharacterBody, Vector3>>(DoJumpFix);
		}

		public static Vector3 DoJumpFix(Vector3 vector, CharacterBody characterBody)
		{
			/*
			var horizontal = vector + Vector3.down * vector.y;
			var cmi = characterBody.characterMotor as IPhysMotor;
			var vhorizontal = cmi.velocity + Vector3.down * cmi.velocity.y;
			if (vhorizontal.sqrMagnitude > horizontal.sqrMagnitude) horizontal = vhorizontal;
			horizontal.y = vector.y;*/

			var bh = characterBody.GetComponent<BunnyFootBehavior>();
			if (!bh) return vector;

			var bunnyFoot = GetInstance<BunnyFoot>();
			var count = characterBody.inventory.GetItemCount(bunnyFoot.ItemDef);
			var grounded = true;

			var velocity = bh.hitGroundVelocity;
			var wishDir = vector.normalized;
			var wishSpeed = vector.magnitude;
			if (!characterBody.characterMotor.isGrounded)
			{
				//wishDir = velo.normalized;
				//wishSpeed = velo.magnitude;
				velocity = (characterBody.characterMotor as IPhysMotor).velocity;
				grounded = false;
			}

			var addvel = Accelerate(velocity, wishDir, wishSpeed,
				wishSpeed * bunnyFoot.scalingInfos[2].ScalingFunction(count),
				bunnyFoot.scalingInfos[3].ScalingFunction(count), 1f, vector, wishSpeed);

			addvel.y = vector.y;
			
			if (!grounded) return addvel;

			return Time.time - bh.hitGroundTime > bunnyFoot!.scalingInfos[1].ScalingFunction(characterBody.inventory.GetItemCount(bunnyFoot.ItemDef)) ? vector : addvel;
		}

		[HarmonyILManipulator, HarmonyPatch(typeof(CharacterMotor), nameof(CharacterMotor.PreMove))]
		public static void PatchMovement(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(
				x => x.MatchMul(),
				x => x.MatchCall<Vector3>(nameof(Vector3.MoveTowards))
			);
			c.RemoveRange(2);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Vector3, Vector3, float, float, CharacterMotor, Vector3>>(DoAirMovement);
		}

		public static Vector3 DoAirMovement(Vector3 velocity, Vector3 target, float num, float deltaTime, CharacterMotor motor)
		{
			var bunnyFoot = GetInstance<BunnyFoot>();
			var count = motor.body?.inventory?.GetItemCount(bunnyFoot.ItemDef) ?? 0; 
			if (count <= 0 || motor.disableAirControlUntilCollision || motor.Motor.GroundingStatus.IsStableOnGround)
				return Vector3.MoveTowards(velocity, target, num * deltaTime);

			var newTarget = target;
			if (!motor.isFlying)
				newTarget.y = 0;

			var wishDir = newTarget.normalized;
			var wishSpeed = motor.walkSpeed * wishDir.magnitude;

			return Accelerate(velocity, wishDir, wishSpeed, bunnyFoot.scalingInfos[0].ScalingFunction(count), motor.acceleration, deltaTime, target, num);
		}

		//Ripped from sbox or gmod, i dont remember
		public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration, float deltaTime, Vector3 target, float num)
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
				wishSpeed = speedLimit;

			// See if we are changing direction a bit
			var currentspeed = Vector3.Dot(velocity, wishDir );

			// Reduce wishspeed by the amount of veer.
			var addspeed = wishSpeed - currentspeed;

			// If not going to add any speed, done.
			if ( addspeed <= 0 )
				if (!addAirControl.Value)
					return velocity;
				else
					return target.sqrMagnitude < velocity.sqrMagnitude ? velocity : Vector3.MoveTowards(velocity, target, num * deltaTime); //return velocity;

			// Determine amount of acceleration.
			var accelspeed = acceleration * deltaTime * wishSpeed; // * SurfaceFriction;

			// Cap at addspeed
			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			return velocity + wishDir * accelspeed;
		}
        protected override void FillItemDisplayRules()
        {
            base.FillItemDisplayRules();

            AddDisplayRules(VanillaIDRS.Commando, new ItemDisplayRule()
            {
                childName = "ThighL",
                localPos = new Vector3(0.12701F, 0.19389F, 0.07333F),
                localAngles = new Vector3(319.3254F, 328.2714F, 358.1904F),
                localScale = new Vector3(0.4149F, 0.4149F, 0.4149F)

            });

            AddDisplayRules(ModdedIDRS.Nemmando, new ItemDisplayRule()
            {
                childName = "ThighL",
                localPos = new Vector3(0.12701F, 0.19389F, 0.07333F),
                localAngles = new Vector3(319.3254F, 328.2714F, 358.1904F),
                localScale = new Vector3(0.4149F, 0.4149F, 0.4149F)

            });

            AddDisplayRules(VanillaIDRS.Huntress, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Bandit, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Mult, new ItemDisplayRule()
            {
                childName = "ThighL",
                localPos = new Vector3(0.04776F, 1.88373F, 1.01038F),
                localAngles = new Vector3(333.9735F, 281.9487F, 357.2159F),
                localScale = new Vector3(3.96454F, 3.96454F, 4.02068F)

            });

            AddDisplayRules(ModdedIDRS.Hand, new ItemDisplayRule()
            {

            });

            AddDisplayRules(VanillaIDRS.Engineer, new ItemDisplayRule()
            {
                childName = "ThighL",
                localPos = new Vector3(0.13222F, 0.26922F, 0.00438F),
                localAngles = new Vector3(301.2251F, 351.1798F, 0.14735F),
                localScale = new Vector3(0.4149F, 0.4149F, 0.4149F)

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
                childName = "PlatformBase",
                localPos = new Vector3(-0.68795F, -0.54996F, 0.03073F),
                localAngles = new Vector3(64.63686F, 352.9881F, 178.495F),
                localScale = new Vector3(1.28781F, 1.28781F, 1.28781F)

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