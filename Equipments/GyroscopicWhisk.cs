﻿using System.Linq;
using BepInEx.Configuration;
using EntityStates;
using HarmonyLib;
using KinematicCharacterController;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BubbetsItems.Equipments
{
	public class GyroscopicWhisk : EquipmentBase
	{
		private GameObject _indicator;
		private ConfigEntry<bool> _filterOutBosses;
		private ConfigEntry<bool> _filterOutPlayers;

		protected override void MakeTokens()
		{
			base.MakeTokens();
			AddToken("GYROSCOPICWHISK_NAME", "SOM's Gyroscopic Whisk");
			AddToken("GYROSCOPICWHISK_DESC", "Spin enemies until they fly to the sky and die.");
			AddToken("GYROSCOPICWHISK_PICKUP", "Spin enemies until they die.");
			AddToken("GYROSCOPICWHISK_LORE", "SOM told Julienne to stop thinking of bad ideas, they said no.");
		}

		protected override void MakeConfigs()
		{
			base.MakeConfigs();
			_filterOutBosses = sharedInfo.ConfigFile.Bind(ConfigCategoriesEnum.General, "Gyroscopic Whisk Filter Bosses", true, "Should gyroscopic whisk filter out bosses.");
			_filterOutPlayers = sharedInfo.ConfigFile.Bind(ConfigCategoriesEnum.General, "Gyroscopic Whisk Filter Players", false, "Should gyroscopic whisk filter out players.");
			_indicator = BubbetsItemsPlugin.AssetBundle.LoadAsset<GameObject>("WhiskIndicator");
		}

		public override void MakeRiskOfOptions()
		{
			base.MakeRiskOfOptions();
			ModSettingsManager.AddOption(new CheckBoxOption(_filterOutBosses));
			ModSettingsManager.AddOption(new CheckBoxOption(_filterOutPlayers));
		}

		public override EquipmentActivationState PerformEquipment(EquipmentSlot equipmentSlot)
		{
			var trg = equipmentSlot.currentTarget.hurtBox;
			if (!trg) return EquipmentActivationState.DidNothing;
			var hc = trg.healthComponent;
			if (!hc) return EquipmentActivationState.DidNothing;
			if (NetworkServer.active)
				hc.gameObject.AddComponent<WhiskBehavior>();
			return EquipmentActivationState.ConsumeStock;
		}

		public override bool UpdateTargets(EquipmentSlot equipmentSlot)
		{
			base.UpdateTargets(equipmentSlot);
            
			if (equipmentSlot.stock <= 0) return false;

			equipmentSlot.ConfigureTargetFinderForEnemies();
			equipmentSlot.currentTarget = new EquipmentSlot.UserTargetInfo(equipmentSlot.targetFinder.GetResults().FirstOrDefault(x => x.healthComponent && !x.healthComponent.GetComponent<WhiskBehavior>() && !(_filterOutBosses.Value && x.healthComponent.body.isBoss) && !(_filterOutPlayers.Value && x.healthComponent.body.isPlayerControlled)));

			if (!equipmentSlot.currentTarget.transformToIndicateAt) return false;
            
			equipmentSlot.targetIndicator.visualizerPrefab = _indicator;
			return true;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MapZone), nameof(MapZone.TeleportBody))]
		public static bool KillOnOOB(CharacterBody characterBody)
		{
			if (!characterBody.GetComponent<WhiskBehavior>()) return true;
			characterBody.master.TrueKill();
			return false;
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

    public class WhiskBehavior : MonoBehaviour
	{
		private KinematicCharacterMotor kcm;
		private float speed = 10f;
		private CharacterBody body;

		private void Awake()
		{
			//kcm = GetComponent<KinematicCharacterMotor>();
			body = GetComponent<CharacterBody>();
			foreach (var entityStateMachine in GetComponents<EntityStateMachine>())
			{
				if (entityStateMachine.state is GenericCharacterMain)
				{
					entityStateMachine.SetNextState(new Uninitialized());
					break;
				}

				if (entityStateMachine.state is FlyState)
				{
					entityStateMachine.SetNextState(new Uninitialized());
					break;
				}
			}
		}

		private void FixedUpdate()
		{
			speed += Time.fixedDeltaTime * 5f;
			if (body.characterMotor)
			{
				body.characterDirection.forward = Quaternion.Euler(0, speed, 0) * body.characterDirection.forward;
				if (speed > 50f)
				{
					body.characterMotor.velocity.y += 10f;
				}
			}
			else
			{
				var dir = body.GetComponent<RigidbodyDirection>();
				dir.enabled = false;
				var transform = body.GetComponent<Transform>();
				
				transform.forward = Quaternion.Euler(0, speed, 0) * transform.forward;
				
				var mot = body.GetComponent<RigidbodyMotor>() as IPhysMotor;
				if (speed > 50f)
				{
					var motVelocityAuthority = mot.velocityAuthority;
					motVelocityAuthority.y += 10f;
					mot.velocityAuthority = motVelocityAuthority;
				}
			}

			/*
			kcm.CharacterForward = Quaternion.Euler(0, speed, 0) * kcm.CharacterForward;
			if (speed > 10f)
			{
				var kcmVelocity = kcm.BaseVelocity;
				kcmVelocity.y += 10f;
				kcm.BaseVelocity = kcmVelocity;
			}*/
		}
	}
}