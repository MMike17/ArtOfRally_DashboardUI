using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DashboardUI
{
    class Dashboard : MonoBehaviour
    {
        static Dashboard instance;

        Image rev;
        Image limiter;
        Image tickPrefab;
        Text gear;
        Text speed;
        Text unit;
        CanvasGroup group;

        void Awake() => StartCoroutine(InitWhenReady());

        IEnumerator InitWhenReady()
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

            // init
            // refresh values
        }

        void Init()
        {
            Main.Try(() =>
            {
                instance = this;

                // TODO : Get components refs
                // rev = transform.GetChild(0).GetChild(1).GetComponentInChildren<Image>();
            });
        }

        void LateUpdate()
        {
            if (instance == null)
                return;

            //
        }

        // TODO : Add method to set colors
        // TODO : Add method to update units
        // TODO : Add method to update gear
    }
}