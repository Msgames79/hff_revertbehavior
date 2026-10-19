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
    public static bool revertFpsRendering = false;
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
            case "revertfpsrendering":
            case "rfr":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref revertFpsRendering, nameof(revertFpsRendering), "");
                        return;
                    }
                    SetBooleanCommand(ref revertFpsRendering, nameof(revertFpsRendering), args[1]);
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
        if (NetGame.isLocal)
        {
            if (Camera.main.name == "GameCamera(Clone)" && Camera.main.renderingPath != RenderingPath.Forward)
            {
                if (revertFpsRendering)
                {
                    Camera.main.renderingPath = RenderingPath.Forward;
                }
            }
            else if (Camera.main.name == "GameCamera(Clone)" && !revertFpsRendering)
            {
                Camera.main.renderingPath = RenderingPath.UsePlayerSettings;
            }
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
                    revertFpsRendering = GUILayout.Toggle(revertFpsRendering, "Revert FPS Rendering", noWrapToggleStyle, expandOptions);
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

    [HarmonyPatch(typeof(CameraController3), nameof(CameraController3.CompensateForWallsNearPlane))]
    static class CollideDefaultTrigger
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Vector3 targetPos, ref Quaternion lookRot, ref float desiredDist, ref float minDist, ref CameraController3 __instance, ref float __result)
        {
            Vector3 vector = lookRot * Vector3.forward;
            float nearClipPlane = __instance.gameCam.nearClipPlane;
            __instance.transform.rotation = lookRot;
            __instance.transform.position = targetPos - vector * (__instance.gameCam.nearClipPlane + minDist);
            ref Vector3 reference = ref __instance.rayStarts[0];
            reference = __instance.gameCam.ViewportToWorldPoint(new Vector3(0f, 0f, nearClipPlane));
            ref Vector3 reference2 = ref __instance.rayStarts[1];
            reference2 = __instance.gameCam.ViewportToWorldPoint(new Vector3(0f, 1f, nearClipPlane));
            ref Vector3 reference3 = ref __instance.rayStarts[2];
            reference3 = __instance.gameCam.ViewportToWorldPoint(new Vector3(1f, 0f, nearClipPlane));
            ref Vector3 reference4 = ref __instance.rayStarts[3];
            reference4 = __instance.gameCam.ViewportToWorldPoint(new Vector3(1f, 1f, nearClipPlane));
            float num = desiredDist - nearClipPlane;
            for (int i = 0; i < __instance.rayStarts.Length; i++)
            {
                Ray ray = new Ray(__instance.rayStarts[i], -vector);
                if (collideDefaultTrigger)
                {
                    if (Physics.Raycast(ray, out var hitInfo, num, __instance.wallLayers) && hitInfo.distance < num)
                    {
                        num = hitInfo.distance;
                    }
                }
                else
                {
                    if (Physics.Raycast(ray, out var hitInfo, num, __instance.wallLayers, QueryTriggerInteraction.Ignore) && hitInfo.distance < num)
                    {
                        num = hitInfo.distance;
                    }
                }
            }

            if (!__instance.useWallDistance)
            {
                num += nearClipPlane + minDist;
            }

            __result = Mathf.Max(num - 0.1f, minDist * 2f);
            return false;
        }
    }

    [HarmonyPatch(typeof(Human), nameof(Human.SpawnAt), new Type[] { typeof(Vector3) })]
    static class RevertRespawnBehavior
    {
        [HarmonyPrefix]
        public static bool Invoke(ref Vector3 pos, ref Human __instance)
        {
            bool flag = true;
            if (revertWaterSpawn && Game.instance.currentLevelType == WorkshopItemSource.BuiltIn && Game.instance.currentLevelNumber == 6 && Game.instance.currentCheckpointNumber == 0)
            {
                pos = new Vector3(-70.61f, 0.71f, 67.91f);
            }
            if (revertRespawnHeight)
            {
                __instance.state = HumanState.Spawning;
                Vector3 vector = __instance.KillHorizontalVelocity();
                int num = 2;
                if (Game.currentLevel != null)
                {
                    float maxHumanVelocity = Game.currentLevel.MaxHumanVelocity;
                    if (vector.magnitude > maxHumanVelocity)
                    {
                        __instance.ControlVelocity(maxHumanVelocity, killHorizontal: false);
                        vector = new Vector3(0f, 0f - maxHumanVelocity, 0f);
                    }
                }
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
