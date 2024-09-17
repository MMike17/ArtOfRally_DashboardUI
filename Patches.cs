using HarmonyLib;

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

    [HarmonyPatch(typeof(RPMDisplay), "UpdateGearDisplay")]
    static class GearUpdater
    {
        static void Postfix(RPMDisplay __instance) => Main.Try(() => Dashboard.UpdateGear(__instance.GearDisplay.text));
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