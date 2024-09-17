using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace DashboardUI
{
    public class Settings : ModSettings, IDrawable
    {
        readonly static Color Brown = new Color(0.75f, 0.5f, 0.25f);

        public enum ColorTag
        {
            White,
            Grey,
            Black,
            Red,
            Green,
            Blue,
            Yellow,
            Magenta,
            Cyan,
            Brown
        }

        public enum DashboardOrientation
        {
            Left,
            Center,
            Right
        }

        [Draw(DrawType.PopupList)]
        public ColorTag primaryColor = ColorTag.White;
        [Draw(DrawType.PopupList)]
        public ColorTag secondaryColor = ColorTag.Cyan;
        [Draw(DrawType.PopupList)]
        public ColorTag pointerColor = ColorTag.Red;
        [Space]
        [Draw(DrawType.PopupList)]
        public DashboardOrientation uiOrientation = DashboardOrientation.Left;
        [Draw(DrawType.Slider, Min = -1, Max = 1, Precision = 3)]
        public float xPositionPercent;
        [Draw(DrawType.Slider, Min = 0, Max = 1, Precision = 3)]
        public float yPositionPercent;
        [Draw(DrawType.Slider, Min = 0.5f, Max = 2, Precision = 3)]
        public float uiScale = 0.8f;

        // TODO : Add default values here

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool showMarkers = false;
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            Main.SetMarkers(showMarkers);
            Dashboard.RefreshColors();
            Dashboard.RefreshPosition();
        }

        public static Color GetColor(ColorTag tag)
        {
            switch (tag)
            {
                case ColorTag.White:
                    return Color.white;
                case ColorTag.Grey:
                    return Color.grey;
                case ColorTag.Black:
                    return Color.black;
                case ColorTag.Red:
                    return Color.red;
                case ColorTag.Green:
                    return Color.green;
                case ColorTag.Blue:
                    return Color.blue;
                case ColorTag.Yellow:
                    return Color.yellow;
                case ColorTag.Magenta:
                    return Color.magenta;
                case ColorTag.Cyan:
                    return Color.cyan;
                case ColorTag.Brown:
                    return Brown;
                default:
                    return Color.clear;
            }
        }

        public static Vector2 GetScreenPos()
        {
            return new Vector2(Screen.width * Main.settings.xPositionPercent, Screen.height * Main.settings.yPositionPercent);
        }
    }
}
