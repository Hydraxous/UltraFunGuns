using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UltraFunGuns.Datas;
using UnityEngine;
using HydraDynamics.Keybinds;
using HydraDynamics;
using BepInEx;
using HydraDynamics.DataPersistence;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using HydraDynamics.Events;

namespace UltraFunGuns
{
    public class DebuggingDummy : MonoBehaviour
    {
        private void Awake()
        {
            CrossModEvents.SubscribeToModEvents(InterpretModEvent);
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                PlayerDeathPatch.god = !PlayerDeathPatch.god;
                if(PlayerDeathPatch.god)
                {
                    SonicReverberator.vineBoom_Loud.PlayAudioClip();
                }
            }

            if(Input.GetKeyDown(KeyCode.Keypad1))
            {
                ReadExtModData();
            }
             
        }


        private void ReadExtModData()
        {
            HydraLogger.Log($"REMD: ATTEMPTING.", DebugChannel.Warning);

            if (Hydynamics.TryGetModInfo("Hydraxous.ULTRAKILL.FishingFriend", out ModInfo fishInfo, "1.0.0"))
            {
                FishDataLol fishData = DataManager.ReadExternalModData<FishDataLol>(fishInfo, "fishData.pee");
                if(fishData != null)
                {
                    HydraLogger.Log($"REMD: {fishData.fishName}",DebugChannel.Warning);
                }else
                {
                    HydraLogger.Log($"REMD: FAIL.", DebugChannel.Warning);
                }
            }
        }

        private void InterpretModEvent(ModEventData eventData)
        {
            if (eventData.eventName == "FishingFriend.FishEvent")
            {
                HydraLogger.Log("Received CME.", DebugChannel.Warning);
                if(eventData.TryDeserialize<FishDataLol>(out FishDataLol fishData))
                {
                    HydraLogger.Log($"{fishData.fishName}",DebugChannel.Warning);
                }
            }
        }

    }

    [Serializable]
    public class FishDataLol : Validatable
    {

        public string fishName;

        public float fishAmount;

        public FishDataLol(float fishAmount = 0.01f, string fishName = "")
        {
            if (fishAmount == 0.01f)
                this.fishAmount = UnityEngine.Random.Range(0.01f, 2404.0f);

            if (fishName.IsNullOrWhiteSpace())
                this.fishName = $"Carl ({this.fishAmount})";
        }

        [JsonConstructor]
        public FishDataLol()
        {

        }

        public override bool Validate()
        {
            return !fishName.IsNullOrWhiteSpace();
        }
    }
}
