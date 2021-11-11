using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
#pragma warning disable 169

namespace APIPlugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.api";
        private const string PluginName = "API";
        private const string PluginVersion = "1.9.0.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> configEnergy;
        internal static ConfigEntry<bool> configDrone;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            configEnergy = Config.Bind("Energy",
                                         "Energy Refresh",
                                         false,
                                         "Max energy increaces and energy refreshes at end of turn");
           configDrone = Config.Bind("Energy",
                                        "Energy Drone",
                                        false,
                                        "Drone is visible to display energy");
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
          if (scene.name == "Part1_Cabin")
          {
            UnityEngine.Object.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));
            if(Plugin.configDrone.Value)
            {
              StartCoroutine(AwakeDrone());
            }
          }
        }

        private IEnumerator AwakeDrone()
        {
          yield return new WaitForSeconds(1);
          Singleton<ResourceDrone>.Instance.Awake();
        }
      }
}
