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
        const float MIN_ANGLE = 45;
        const float MAX_ANGLE = -135;

        static Dashboard instance;

        // UI
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
        CanvasGroup group;

        Dictionary<string, SVAColor> colorMap;
        List<Tick> ticks;
        Vector2Int revThresholds;
        Func<int> GetSpeed;
        Func<int> GetRev;

        public void Init(Vector2Int revThresholds, Func<int> GetSpeed, Func<int> GetRev, string units)
        {
            this.revThresholds = revThresholds;
            this.GetSpeed = GetSpeed;
            this.GetRev = GetRev;

            StartCoroutine(InitWhenReady(units));
        }

        IEnumerator InitWhenReady(string units)
        {
            // this is here so we don't have the glitch
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = false;

            // check manager availability
            GameEntryPoint entry = FindObjectOfType<GameEntryPoint>();

            if (entry == null)
            {
                Main.Log("Can't find entry point. Are you sure you're spawning the UI in the right scene ?");
                yield break;
            }

            FieldInfo testInfo = entry.GetType().GetField("eventManager", BindingFlags.Static | BindingFlags.NonPublic);
            yield return new WaitUntil(() => testInfo.GetValue(entry) != null);

            Init(units);
            RefreshColors();
        }

        void Init(string units)
        {
            Main.Try(() =>
            {
                instance = this;

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
                unit.text = units;
                speed = transform.GetChild(3).GetComponent<Text>();

                ticks = new List<Tick>();
                int ticksCount = (revThresholds.y - revThresholds.x) / 1000;

                for (int i = 0; i <= ticksCount; i++)
                {
                    Tick tick = new Tick(Instantiate(tickPrefab.graphic, tickHolder), i + 1);
                    tick.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(MIN_ANGLE, MAX_ANGLE, (float)i / ticksCount));
                    tick.FixDisplay();

                    ticks.Add(tick);
                }

                tickPrefab.transform.gameObject.SetActive(false);
                limiter.fillAmount = (float)1 / ticksCount;

                // store color map
                colorMap = new Dictionary<string, SVAColor>();

                colorMap.Add(nameof(frame), new SVAColor(frame));
                colorMap.Add(nameof(outline), new SVAColor(outline));
                colorMap.Add(nameof(rev), new SVAColor(rev));
                colorMap.Add(nameof(tickPrefab.graphic), new SVAColor(tickPrefab.graphic));
                colorMap.Add(nameof(tickPrefab.display), new SVAColor(tickPrefab.display));
                colorMap.Add(nameof(dial), new SVAColor(dial));
                colorMap.Add(nameof(pointer), new SVAColor(pointer));
                colorMap.Add(nameof(gear), new SVAColor(gear));
                colorMap.Add(nameof(unit), new SVAColor(unit));
                colorMap.Add(nameof(speed), new SVAColor(speed));
            });
        }

        void LateUpdate()
        {
            if (instance == null)
                return;

            float revPercent = Mathf.InverseLerp(revThresholds.x, revThresholds.y, GetRev());
            dial.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(MIN_ANGLE, MAX_ANGLE, revPercent));

            speed.text = Mathf.CeilToInt(GetSpeed()).ToString();
        }

        // TODO : Add method to update units
        // TODO : Add method to update gear

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
            instance.unit.color = instance.colorMap[nameof(instance.)].Apply(Settings.GetColor(Main.settings.primaryColor));
            instance.speed.color = instance.colorMap[nameof(instance.)].Apply(Settings.GetColor(Main.settings.primaryColor));

            Color tickColor = instance.colorMap[nameof(instance.tickPrefab.graphic)].Apply(Settings.GetColor(Main.settings.primaryColor));
            Color tickTextColor = instance.colorMap[nameof(instance.tickPrefab.display)].Apply(Settings.GetColor(Main.settings.primaryColor));

            instance.ticks.ForEach(tick =>
            {
                tick.graphic.color = tickColor;
                tick.display.color = tickTextColor;
            });
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

            public SVAColor(Graphic ui)
            {
                Color.RGBToHSV(ui.color, out _, out s, out v);
                a = ui.color.a;
            }

            public Color Apply(Color color)
            {
                Color.RGBToHSV(color, out float h, out _, out _);
                Color result = Color.HSVToRGB(h, s, v);
                result.a = a;
                return result;
            }
        }
    }
}