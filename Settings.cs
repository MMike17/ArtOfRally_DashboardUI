using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace ModBase
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]
        
        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool showMarkers;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            Main.SetMarkers(showMarkers);

            //
        }
    }
}
