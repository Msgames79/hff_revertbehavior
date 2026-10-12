using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using HumanAPI;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Net;
using Steamworks;
using System.Security;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using UnityEngine.UI;
using Multiplayer;
using System.Configuration;
using System.Collections;
using Mono.Security.X509.Extensions;
using System.Xml.Linq;
using ProBuilder2.Common;
using System.Runtime.CompilerServices;

namespace hff_revertbehavior;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Main : BaseUnityPlugin
{
    public static Main instance;
    public static bool collideDefaultTrigger = false;
    public static bool revertRespawnHeight = false;
    public static bool revertWaterSpawn = false;
    private static Rect mainWindowRect;

    void Start()
    {
        instance = this;
        Shell.RegisterCommand("revertbehavior", new Action<string>(ShellFunction), null);
        Shell.RegisterCommand("rb", new Action<string>(ShellFunction), null);
        mainWindowRect = new Rect(1090f, 60f, 0f, 0f);
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

    void ShellFunction(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            Debug.Log("Invalid argument");
            return;
        }
        string[] args = arg.Split(' ');
        if (args.Length == 0)
        {
            Debug.Log("Invalid argument");
            return;
        }
        switch (args[0].ToLower())
        {
            case "collidedefaulttrigger":
            case "cdt":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref collideDefaultTrigger, nameof(collideDefaultTrigger), "");
                        return;
                    }
                    SetBooleanCommand(ref collideDefaultTrigger, nameof(collideDefaultTrigger), args[1]);
                    return;
                }
            case "revertrespawnheight":
            case "rrh":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref revertRespawnHeight, nameof(revertRespawnHeight), "");
                        return;
                    }
                    SetBooleanCommand(ref revertRespawnHeight, nameof(revertRespawnHeight), args[1]);
                    return;
                }
            case "revertwaterspawn":
            case "rws":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref revertWaterSpawn, nameof(revertWaterSpawn), "");
                        return;
                    }
                    SetBooleanCommand(ref revertWaterSpawn, nameof(revertWaterSpawn), args[1]);
                    return;
                }
            default:
                {
                    Debug.Log("Invalid argument");
                    return;
                }
        }
    }

    private static bool guiOpened = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            guiOpened = !guiOpened;
        }
    }
    private static GUILayoutOption[] expandOptions;
    private static GUIStyle noWrapToggleStyle;
    void OnGUI()
    {
        if (expandOptions == null)
        {
            expandOptions =
            [
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
                GUILayout.MaxWidth(Screen.width)
            ];
            noWrapToggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
        }
        if (guiOpened)
        {
            mainWindowRect = GUILayout.Window(1694068100, mainWindowRect, WindowFunction, "Revert Behavior", expandOptions);
        }
    }

    void WindowFunction(int windowId)
    {
        switch (windowId)
        {
            case 1694068100:
                {
                    collideDefaultTrigger = GUILayout.Toggle(collideDefaultTrigger, "Collide Defalt Trigger", noWrapToggleStyle, expandOptions);
                    revertRespawnHeight = GUILayout.Toggle(revertRespawnHeight, "Revert Respawn Height", noWrapToggleStyle, expandOptions);
                    revertWaterSpawn = GUILayout.Toggle(revertWaterSpawn, "Revert Water Spawn", noWrapToggleStyle, expandOptions);
                    break;
                }
            default:
                {
                    return;
                }
        }
        GUI.DragWindow();
    }

    public static void SetBooleanCommand(ref bool boolean, string name, string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            boolean = !boolean;
        }
        else if (Regex.IsMatch(arg, @"^(t(rue)?|1|on)$", RegexOptions.IgnoreCase))
        {
            boolean = true;
        }
        else if (Regex.IsMatch(arg, @"^(f(alse)?|0|off)$", RegexOptions.IgnoreCase))
        {
            boolean = false;
        }
        else
        {
            Debug.Log("Invalid argument");
        }
        Debug.Log($"Set \"{name}\" to \"{boolean}\"");
    }

    [HarmonyPatch(typeof(Physics), nameof(Physics.Raycast), new Type[] { typeof(Ray), typeof(RaycastHit), typeof(float), typeof(int), typeof(QueryTriggerInteraction) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
    static class CollideDefaultTrigger
    {
        [HarmonyPrefix]
        public static bool Invoke(ref Ray ray, out RaycastHit hitInfo, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction, ref bool __result)
        {
            if (collideDefaultTrigger)
            {
                hitInfo = new RaycastHit();
                __result = Physics.Raycast(ray.origin, ray.direction, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
            }
            else
            {
                __result = Physics.Raycast(ray.origin, ray.direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Human), nameof(Human.SpawnAt), new Type[] { typeof(Vector3) })]
    static class RevertRespawnBehavior
    {
        [HarmonyPrefix]
        public static bool Invoke(ref Vector3 pos, ref Human __instance)
        {
            bool flag = false;
            if (revertWaterSpawn && Game.instance.currentLevelType == WorkshopItemSource.BuiltIn && Game.instance.currentLevelNumber == 6 && Game.instance.currentCheckpointNumber == 0)
            {
                pos = new Vector3(-70.61f, 0.71f, 67.91f);
            }
            if (revertRespawnHeight)
            {
                __instance.state = HumanState.Spawning;
                Vector3 vector = __instance.KillHorizontalVelocity();
                if (Game.currentLevel != null)
                {
                    float maxHumanVelocity = Game.currentLevel.MaxHumanVelocity;
                    if (vector.magnitude > maxHumanVelocity)
                    {
                        __instance.ControlVelocity(maxHumanVelocity, killHorizontal: false);
                        vector = new Vector3(0f, 0f - maxHumanVelocity, 0f);
                    }
                }
                int num = 2;
                Vector3 position = pos - vector * num - Physics.gravity * num * num / 2f;
                __instance.SetPosition(position);
                if (vector.magnitude < 5f)
                {
                    __instance.AddRandomTorque(1f);
                }
                __instance.Reset();
                flag = false;
            }
            return flag;
        }
    }
}
