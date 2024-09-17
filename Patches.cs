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

    // TODO : Plug values into dashboard UI
    // TODO : Hide game's dahsboard UI
    // TODO : Plug into menu animations
    // TODO : Add positionning and scale settings
    // TODO : Call Dashboard.UpdateUnits in patch
    // TODO : Call Dashboard.UpdateGear in patch
}