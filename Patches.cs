using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DashboardUI
{
    [HarmonyPatch(typeof(StageSceneManager), MethodType.Constructor)]
    static class UISpawner
    {
        static void Postfix() => Main.Try(() => Main.SpawnUI());
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.RefreshHudElements))]
    static class UnitsUpdater
    {
        static void Postfix() => Main.Try(() => Dashboard.UpdateUnits());
    }

    static class GearUpdater
    {
        static RPMDisplay ui;

        public static void Update()
        {
            if (ui == null)
                ui = GameObject.FindObjectOfType<RPMDisplay>();

            if (ui != null && Dashboard.initialized)
            {
                Main.InvokeMethod(ui, "UpdateGearDisplay", BindingFlags.Instance, new object[] { });
                Dashboard.UpdateGear(ui.GearDisplay.text);
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.ShowStageHud))]
    static class ShowAnimator
    {
        static HudManager manager;

        static void Postfix(HudManager __instance)
        {
            if (!Main.enabled)
                return;

            manager = __instance;

            Main.Try(() =>
            {
                __instance.SpeedoHudGroup.gameObject.SetActive(false);
                __instance.RPMHudGroup.gameObject.SetActive(false);

                Dashboard.PlayAnimation(true);
            });
        }

        public static void SetHUDState(bool state)
        {
            if (manager == null)
                return;

            manager.SpeedoHudGroup.gameObject.SetActive(!state);
            manager.RPMHudGroup.gameObject.SetActive(!state);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.HideStageHud))]
    static class HideAnimator
    {
        static void Postfix()
        {
            if (!Main.enabled)
                return;

            Main.Try(() => Dashboard.PlayAnimation(false));
        }
    }
}