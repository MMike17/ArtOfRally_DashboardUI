using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DashboardUI
{
    public class Dashboard : MonoBehaviour
    {
        float minAngle => Main.settings.uiOrientation == Settings.DashboardOrientation.Center ? 120 : 45;
        float maxAngle => Main.settings.uiOrientation == Settings.DashboardOrientation.Center ? -120 : -135;

        static Dashboard instance;

        // UI
        CanvasGroup group;
        Image frame;
        Image outline;
        Image rev;
        Image limiter;
        Transform tickHolder;
        Tick tickPrefab;
        Image dial;
        Image pointer;
        Text gear;
        Text unit;
        Text speed;

        Dictionary<string, SVAColor> colorMap;
        List<Tick> ticks;
        HudManager hud;
        Vector2 revThresholds;
        Func<float> GetSpeed;
        Func<float> GetRev;

        public void Init(HudManager hud)
        {
            instance = this;
            this.hud = hud;

            StartCoroutine(InitWhenReady());
        }

        IEnumerator InitWhenReady()
        {
            // this is here so we don't have the glitch
            group = gameObject.GetComponent<CanvasGroup>();
            group.alpha = 0;

            // check manager availability
            GameEntryPoint entry = FindObjectOfType<GameEntryPoint>();

            if (entry == null)
            {
                Main.Log("Can't find entry point. Are you sure you're spawning the UI in the right scene ?");
                yield break;
            }

            FieldInfo testInfo = entry.GetType().GetField("eventManager", BindingFlags.Static | BindingFlags.NonPublic);
            yield return new WaitUntil(() => testInfo.GetValue(entry) != null);

            GetRefs();
            RefreshColors();
            RefreshPosition();
            UpdateUnits();
        }

        void GetRefs()
        {
            Main.Try(() =>
            {
                // get data
                revThresholds = new Vector2(
                    GameEntryPoint.EventManager.playerManager.drivetrain.minRPM,
                    GameEntryPoint.EventManager.playerManager.drivetrain.maxRPM
                );

                GetRev = () => GameEntryPoint.EventManager.playerManager.drivetrain.rpm;
                GetSpeed = () => Main.GetField<float, HudManager>(hud, "digitalSpeedoVelo", BindingFlags.Instance);

                // get refs
                frame = transform.GetComponent<Image>();
                outline = transform.GetChild(0).GetChild(0).GetComponent<Image>();
                rev = transform.GetChild(0).GetChild(1).GetComponentInChildren<Image>();
                limiter = transform.GetChild(0).GetChild(2).GetComponentInChildren<Image>();

                tickHolder = transform.GetChild(0).GetChild(3);
                tickPrefab = new Tick(tickHolder.GetChild(0).GetComponent<Image>(), 0);

                dial = transform.GetChild(0).GetChild(4).GetComponent<Image>();
                pointer = dial.transform.GetComponentInChildren<Image>();
                gear = transform.GetChild(1).GetComponent<Text>();
                unit = transform.GetChild(2).GetComponent<Text>();
                speed = transform.GetChild(3).GetComponent<Text>();

                ticks = new List<Tick>();
                int ticksCount = Mathf.RoundToInt((revThresholds.y - revThresholds.x) / 1000);

                for (int i = 0; i <= ticksCount; i++)
                {
                    Tick tick = new Tick(Instantiate(tickPrefab.graphic, tickHolder), i + 1);
                    tick.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(minAngle, maxAngle, (float)i / ticksCount));
                    tick.FixDisplay();

                    ticks.Add(tick);
                }

                tickPrefab.transform.gameObject.SetActive(false);
                limiter.fillAmount = (float)1 / ticksCount;

                // store color map
                colorMap = new Dictionary<string, SVAColor>();

                colorMap.Add(nameof(frame), new SVAColor(frame, false));
                colorMap.Add(nameof(outline), new SVAColor(outline, false));
                colorMap.Add(nameof(rev), new SVAColor(rev, false));
                colorMap.Add(nameof(tickPrefab.graphic), new SVAColor(tickPrefab.graphic, false));
                colorMap.Add(nameof(tickPrefab.display), new SVAColor(tickPrefab.display, false));
                colorMap.Add(nameof(dial), new SVAColor(dial, false));
                colorMap.Add(nameof(pointer), new SVAColor(pointer, true));
                colorMap.Add(nameof(gear), new SVAColor(gear, false));
                colorMap.Add(nameof(unit), new SVAColor(unit, false));
                colorMap.Add(nameof(speed), new SVAColor(speed, false));
            });
        }

        void LateUpdate()
        {
            if (instance == null)
                return;

            float revPercent = Mathf.InverseLerp(revThresholds.x, revThresholds.y, GetRev());
            dial.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(minAngle, maxAngle, revPercent));

            speed.text = Mathf.CeilToInt(GetSpeed()).ToString();
        }

        public static void RefreshColors()
        {
            if (instance == null)
                return;

            instance.frame.color = instance.colorMap[nameof(instance.frame)].Apply(Settings.GetColor(Main.settings.primaryColor));
            instance.outline.color = instance.colorMap[nameof(instance.outline)].Apply(Settings.GetColor(Main.settings.primaryColor));
            instance.rev.color = instance.colorMap[nameof(instance.rev)].Apply(Settings.GetColor(Main.settings.secondaryColor));
            instance.dial.color = instance.colorMap[nameof(instance.dial)].Apply(Settings.GetColor(Main.settings.primaryColor));
            instance.pointer.color = instance.colorMap[nameof(instance.pointer)].Apply(Settings.GetColor(Main.settings.pointerColor));
            instance.gear.color = instance.colorMap[nameof(instance.gear)].Apply(Settings.GetColor(Main.settings.secondaryColor));
            instance.unit.color = instance.colorMap[nameof(instance.unit)].Apply(Settings.GetColor(Main.settings.primaryColor));
            instance.speed.color = instance.colorMap[nameof(instance.speed)].Apply(Settings.GetColor(Main.settings.primaryColor));

            Color tickColor = instance.colorMap[nameof(instance.tickPrefab.graphic)].Apply(Settings.GetColor(Main.settings.primaryColor));
            Color tickTextColor = instance.colorMap[nameof(instance.tickPrefab.display)].Apply(Settings.GetColor(Main.settings.primaryColor));

            instance.ticks.ForEach(tick =>
            {
                tick.graphic.color = tickColor;
                tick.display.color = tickTextColor;
            });
        }

        public static void RefreshPosition()
        {
            if (instance == null)
                return;

            instance.transform.position = Settings.GetScreenPos();
            instance.transform.localScale = Vector3.one * Main.settings.uiScale;
        }

        public static void UpdateUnits()
        {
            if (instance == null)
                return;

            instance.unit.text = SaveGame.GetInt("SETTINGS_SPEED_UNITS", 0) == 0 ? "mph" : "kmh";
        }

        public static void UpdateGear(string gear)
        {
            if (instance == null)
                return;

            instance.gear.text = gear;
        }

        class Tick
        {
            public Transform transform;
            public Image graphic;
            public Text display;

            public Tick(Image graphic, int level)
            {
                this.graphic = graphic;
                transform = graphic.transform;

                display = graphic.GetComponentInChildren<Text>();
                display.text = level.ToString();
            }

            public void FixDisplay() => display.transform.localRotation = Quaternion.Euler(0, 0, -transform.localEulerAngles.z);
        }

        class SVAColor
        {
            public float s;
            public float v;
            public float a;

            bool useSaturation;

            public SVAColor(Graphic ui, bool useSaturation)
            {
                this.useSaturation = useSaturation;

                if (useSaturation)
                    Color.RGBToHSV(ui.color, out _, out s, out v);
                else
                    Color.RGBToHSV(ui.color, out _, out _, out v);

                a = ui.color.a;
            }

            public Color Apply(Color color)
            {
                float hue;

                if (useSaturation)
                    Color.RGBToHSV(color, out hue, out _, out _);
                else
                    Color.RGBToHSV(color, out hue, out s, out _);

                Color result = Color.HSVToRGB(hue, s, v);
                result.a = a;
                return result;
            }
        }
    }
}