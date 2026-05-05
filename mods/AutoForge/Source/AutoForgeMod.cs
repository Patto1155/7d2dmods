using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class AutoForgeMod : IModApi
{
    private const float TICK_INTERVAL = 2f;
    private float elapsed = 0f;

    public void InitMod(Mod _modInstance)
    {
        Harmony harmony = new Harmony("com.patto1155.autoforge");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        ModEvents.GameUpdate.RegisterHandler(OnGameUpdate);

        Log.Out("[AutoForge] AutoForge loaded — v0.1.0");
    }

    private void OnGameUpdate(ref ModEvents.SGameUpdateData _data)
    {
        World world = GameManager.Instance.World;
        if (world == null)
            return;

        elapsed += Time.deltaTime;
        if (elapsed < TICK_INTERVAL)
            return;

        elapsed = 0f;

        AutoForgeTick.RunAll();
    }
}
