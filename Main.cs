using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace DashboardUI
{
    public class Main
    {
        public static bool enabled { get; private set; }

        public static ModEntry.ModLogger Logger;
        public static Settings settings;

        static List<GameObject> markers;
        static Material markerMat;
        static GameObject leftDashboard;
        static GameObject rightDashboard;
        static GameObject centerDashboard;
        static Dashboard currentDashboard;
        static Settings.DashboardOrientation orientation;

        // Called by the mod manager
        static bool Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = ModSettings.Load<Settings>(modEntry);

            // Harmony patching
            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = (entry) => settings.Draw(entry);
            modEntry.OnSaveGUI = (entry) => settings.Save(entry);

            // load asset bundle
            Try(() =>
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(modEntry.Path, "dashboard_ui"));

                if (bundle != null)
                {
                    leftDashboard = bundle.LoadAsset<GameObject>("DashboardUILeft");
                    rightDashboard = bundle.LoadAsset<GameObject>("DashboardUIRight");
                    centerDashboard = bundle.LoadAsset<GameObject>("DashboardUICenter");
                }
                else
                    Error("Couldn't load asset bundle \"dashboard_ui\"");

                if (bundle != null && !settings.disableInfoLogs)
                    Log("Loaded bundle \"dashboard_ui\"");
            });

            markers = new List<GameObject>();
            orientation = settings.uiOrientation;
            return true;
        }

        static bool OnToggle(ModEntry modEntry, bool state)
        {
            enabled = state;

            if (currentDashboard != null)
                currentDashboard.gameObject.SetActive(state);

            return true;
        }

        public static void Log(string message) => Logger.Log(message);

        public static void Error(string message) => Logger.Error(message);

        public static void Try(Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static T GetField<T, U>(U source, string fieldName, BindingFlags flags)
        {
            FieldInfo info = source.GetType().GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return default(T);
            }

            return (T)info.GetValue(source);
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static void SetField<T, U>(U source, string fieldName, BindingFlags flags, object value)
        {
            FieldInfo info = source.GetType().GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return;
            }

            info.SetValue(source, value);
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static void InvokeMethod<T>(T source, string methodName, BindingFlags flags, object[] args)
        {
            MethodInfo info = source.GetType().GetMethod(methodName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find method info for method \"" + methodName + "\" in type \"" + source.GetType() + "\"");
                return;
            }

            info.Invoke(source, args);
        }

        public static void SetMarkers(bool state)
        {
            CleanMarkerList();
            markers.ForEach(item => item.SetActive(state));
        }

        public static void AddMarker(Transform parent, Vector3 position, float size)
        {
            if (markerMat == null)
            {
                markerMat = new Material(Shader.Find("Standard"));
                markerMat.color = Color.red;
                markerMat.SetColor("_EmissionColor", Color.red);
            }

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.GetComponent<Renderer>().material = markerMat;

            marker.transform.SetParent(parent);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * size;

            marker.SetActive(settings.showMarkers);
            markers.Add(marker);
        }

        static void CleanMarkerList()
        {
            List<int> toRemove = new List<int>();

            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i] == null)
                    toRemove.Add(i);
            }

            toRemove.Reverse();
            toRemove.ForEach(index => markers.RemoveAt(index));
        }

        public static void SpawnUI()
        {
            HudManager hud = GameObject.FindObjectOfType<HudManager>();

            if (hud == null)
            {
                Error("Couldn't find the HUD manager. Aborting.");
                return;
            }

            if (currentDashboard != null)
                GameObject.Destroy(currentDashboard);

            GameObject prefab = null;

            switch (settings.uiOrientation)
            {
                case Settings.DashboardOrientation.Left:
                    prefab = leftDashboard;
                    break;

                case Settings.DashboardOrientation.Right:
                    prefab = rightDashboard;
                    break;

                case Settings.DashboardOrientation.Center:
                    prefab = centerDashboard;
                    break;
            }

            currentDashboard = GameObject.Instantiate(prefab, hud.transform.GetChild(0)).AddComponent<Dashboard>();
            currentDashboard.Init(hud);

            if (!settings.disableInfoLogs)
                Log("Spawned dashboard UI");
        }

        public static void RefreshUI()
        {
            if (settings.uiOrientation != orientation)
                SpawnUI();
            else
            {
                Dashboard.RefreshColors();
                Dashboard.RefreshPosition();
            }
        }
    }
}