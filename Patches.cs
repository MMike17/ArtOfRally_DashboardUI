using HarmonyLib;

namespace DashboardUI
{
    // Patch model
    // [HarmonyPatch(typeof(), nameof())]
    // [HarmonyPatch(typeof(), MethodType.)]
    // static class type_method_Patch
    // {
    // 	static void Prefix()
    // 	{
    // 		//
    // 	}

    // 	static void Postfix()
    // 	{
    // 		//
    // 	}
    // }

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
        static void Postfix()
        {
            if (!Main.enabled)
                return;

            Main.Try(() => Dashboard.PlayAnimation(true));
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

    // TODO : Add conditions to show/enable ui
    // TODO : Hide game's dahsboard UI
}