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
        private static void PostfixOnEnable(PlayerControllerB __instance)
        {
            var aspect = (float)Screen.width / Screen.height;
            RectTransform component = GameObject.Find("Panel").GetComponent<RectTransform>();
            component.anchorMin = new Vector2(0f, 0f);
            component.anchorMax = new Vector2(1f, 1f);
            component.anchoredPosition = Vector2.zero;
            component.sizeDelta = Vector2.zero;
            ManualLogSource mls = CWidescreen.mls;
            float num = ((float)Screen.width / ((float)Screen.height * 1.7777778f));
            mls.LogInfo((object)("!!! " + num + " set to xScaleFactor!"));
            ((Transform)component).localScale = new Vector3(num + 0.1f, 1f, 1f);


            //Scale the hud
            UnityEngine.UI.AspectRatioFitter uiArf = __instance.playerHudUIContainer.GetComponent<AspectRatioFitter>();
            uiArf.aspectRatio = aspect;

            CanvasScaler uiScaler = __instance.playerHudUIContainer.parent.GetComponent<CanvasScaler>();
            uiScaler.scaleFactor = aspect; //Approx aspect ratio seems to work for canvas scaling. Maybe coincidence.
            uiScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            //Recenter Inventory Grid for 21:9
            var inventory = __instance.playerHudUIContainer.Find("Inventory").GetComponent<RectTransform>();
            inventory.anchorMax = new Vector2(0.0f, 1.0f);
            inventory.anchorMin = new Vector2(1.04f, 0.1f);

            //Aspect and FOV are now mostly correct.
            __instance.gameplayCamera.aspect = aspect;

        }

        static float lastActualFov = 60f;

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        private static void PrefixUpdate(PlayerControllerB __instance)
        {
            lastActualFov = __instance.gameplayCamera.fieldOfView;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void PostfixUpdate(PlayerControllerB __instance)
        {
            float newTarget = 60f;
            var scavHelmet = GameObject.Find("ScavengerHelmet").GetComponent<Transform>();


            if (__instance.inTerminalMenu)
            {
                newTarget = 56f;
                scavHelmet.localScale = new Vector3(0.5106f, 0.5256f, 0.5236f);
            }
            else if (__instance.IsInspectingItem)
            {
                newTarget = 46f * 0.65f;
                scavHelmet.localScale = new Vector3(0.5106f, 0.5256f, 0.5236f);
            }
            else
            {
                newTarget = !__instance.isSprinting ? 66f * 0.65f : 68f * 0.65f;
                scavHelmet.localScale = new Vector3(0.5906f, 0.7236f, 0.5236f);
            }

            __instance.gameplayCamera.fieldOfView = UnityEngine.Mathf.Lerp(lastActualFov, newTarget, 6.0f * UnityEngine.Time.deltaTime);

        }
    }
}
