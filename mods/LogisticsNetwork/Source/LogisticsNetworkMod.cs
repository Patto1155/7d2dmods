using System.Reflection;
using HarmonyLib;
using LogisticsNetwork.Network;
using LogisticsNetwork.Tick;
using LogisticsNetwork.Util;
using UnityEngine;

namespace LogisticsNetwork
{
    public class LogisticsNetworkMod : IModApi
    {
        private const float TickIntervalSeconds = 2f;
        private float elapsedSeconds;
        private World lastWorld;

        public void InitMod(Mod _modInstance)
        {
            var harmony = new Harmony("com.patto1155.logisticsnetwork");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ModEvents.GameUpdate.RegisterHandler(OnGameUpdate);

            Log.Out("Wasteland Logistics loaded — passive network scanner initialized.");
        }

        private void OnGameUpdate(ref ModEvents.SGameUpdateData _data)
        {
            World world = GameManager.Instance.World;
            if (world == null)
                return;

            if (!ReferenceEquals(world, lastWorld))
            {
                lastWorld = world;
                elapsedSeconds = 0f;
                NetworkRegistry.Clear();
                LogisticsNetworkTick.Reset();
            }

            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds < TickIntervalSeconds)
                return;

            elapsedSeconds -= TickIntervalSeconds;
            LogisticsNetworkTick.RunAll(world);
        }
    }
}
