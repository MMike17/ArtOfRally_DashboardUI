using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DashboardUI
{
    class Dashboard : MonoBehaviour
    {
        const float MIN_ANGLE = 45;
        const float MAX_ANGLE = -135;

        static Dashboard instance;

        Image rev;
        Image limiter;
        Transform tickHolder;
        Tick tickPrefab;
        Transform dial;
        Text gear;
        Text unit;
        Text speed;
        CanvasGroup group;

        //List<Tick> ticks;
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
            // refresh values
        }

        void Init(string units)
        {
            Main.Try(() =>
            {
                instance = this;

                rev = transform.GetChild(0).GetChild(1).GetComponentInChildren<Image>();
                limiter = transform.GetChild(0).GetChild(2).GetComponentInChildren<Image>();

                tickHolder = transform.GetChild(0).GetChild(3);
                tickPrefab = new Tick(tickHolder.GetChild(0), 0);

                dial = transform.GetChild(0).GetChild(4);
                gear = transform.GetChild(1).GetComponent<Text>();
                unit = transform.GetChild(2).GetComponent<Text>();
                unit.text = units;
                speed = transform.GetChild(3).GetComponent<Text>();

                //ticks = new List<Tick>();
                int ticksCount = (revThresholds.y - revThresholds.x) / 1000;

                for (int i = 0; i <= ticksCount; i++)
                {
                    Tick tick = new Tick(Instantiate(tickPrefab.transform, tickHolder), i + 1);
                    tick.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(MIN_ANGLE, MAX_ANGLE, (float)i / ticksCount));
                    //ticks.Add(tick);
                }

                limiter.fillAmount = (float)1 / ticksCount;
            });
        }

        void LateUpdate()
        {
            if (instance == null)
                return;

            float revPercent = Mathf.InverseLerp(revThresholds.x, revThresholds.y, GetRev());
            dial.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(MIN_ANGLE, MAX_ANGLE, revPercent));

            speed.text = Mathf.CeilToInt(GetSpeed()).ToString();
        }

        // TODO : Add color set mapping for color swapping
        // TODO : Add method to set colors
        // TODO : Add method to update units
        // TODO : Add method to update gear

        class Tick
        {
            public Transform transform;

            public Tick(Transform transform, int level)
            {
                this.transform = transform;

                transform.GetComponentInChildren<Text>().text = level.ToString();
            }
        }
    }
}