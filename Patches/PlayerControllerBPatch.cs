using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalWidescreen.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable(PlayerControllerB __instance)
        {
            GameObject ingamePlayerHud = GameObject.Find("Panel");

            RectTransform panelRectTransform = ingamePlayerHud.GetComponent<RectTransform>();

            panelRectTransform.anchorMin = new Vector2(0, 0);
            panelRectTransform.anchorMax = new Vector2(1, 1);
            panelRectTransform.anchoredPosition = Vector2.zero;
            panelRectTransform.sizeDelta = Vector2.zero;

            float xScaleFactor = Screen.width / (Screen.height * (16f / 9f));
            panelRectTransform.localScale = new Vector3(xScaleFactor + 0.1f, 1f, 1f);

            ManualLogSource mls = LethalWidescreen.mls;
            mls.LogInfo("!!! " + xScaleFactor + " set to xScaleFactor!");

            //Scale the hud
            UnityEngine.UI.AspectRatioFitter uiArf = __instance.playerHudUIContainer.GetComponent<AspectRatioFitter>();
            uiArf.aspectRatio = (float)Screen.width / Screen.height;

            CanvasScaler uiScaler = __instance.playerHudUIContainer.parent.GetComponent<CanvasScaler>();
            uiScaler.scaleFactor = (float)Screen.width / Screen.height; //Approx aspect ratio seems to work for canvas scaling. Maybe coincidence.
            uiScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            //Recenter Inventory Grid for 21:9
            var inventory = __instance.playerHudUIContainer.Find("Inventory").GetComponent<RectTransform>();
            inventory.anchorMax = new Vector2(0.0f, 1.0f);
            inventory.anchorMin = new Vector2(1.04f, 0.1f);

            //sliiightly bump camera aspect
            __instance.gameplayCamera.aspect = 1.78f; //Slightly increase the aspect ratio. Actual 21:9 would be cheat-ey. Adds a few degrees of vision on either side to combat stretching further.
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void LateUpdate(PlayerControllerB __instance)
        {
            Camera camera = __instance.gameplayCamera;
            __instance.targetFOV = camera.fieldOfView * 1.25f;
        }
    }
}
