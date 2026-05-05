using System.Reflection;
using HarmonyLib;
using LogisticsNetwork.Util;

namespace LogisticsNetwork
{
    public class LogisticsNetworkMod : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            var harmony = new Harmony("com.patto1155.logisticsnetwork");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Out("Wasteland Logistics loaded — passive skeleton initialized.");
        }
    }
}
