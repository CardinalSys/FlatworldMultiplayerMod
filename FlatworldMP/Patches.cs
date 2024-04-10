using HarmonyLib;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlatworldMP
{

    [HarmonyLib.HarmonyPatch(typeof(PlayerCTRL), "getInputKeyboard")]
    static class InputPatch
    {

        static bool Prefix(PlayerCTRL __instance, ref float[] __result)
        {

            float[] array = new float[2];

            if (__instance.gameObject.name == "SecondPlayer")
            {

                if ((bool)Traverse.Create(__instance).Field("haciendoMagia").GetValue())
                    return false;
                if (Input.GetKey(KeyCode.K))
                {
                    array[1] = -1f;
                }
                if (Input.GetKey(KeyCode.I))
                {
                    array[1] = 1f;
                }
                if (Input.GetKey(KeyCode.J))
                {
                    array[0] = -1f;
                }
                if (Input.GetKey(KeyCode.L))
                {
                    array[0] = 1f;
                }


                __result = array;

                return false;
            }

            return true;
        }

    }


    [HarmonyLib.HarmonyPatch(typeof(PlayerCTRL), "StartMagic")]
    static class StartMagicPatch
    {

        static bool Prefix(PlayerCTRL __instance)
        {


            if (__instance.gameObject.name == "SecondPlayer")
            {
                
                GameObject colliderMagia = Traverse.Create(__instance).Field("colliderMagia").GetValue() as GameObject;

                if (!(colliderMagia == null))
                {
                    return false;
                }

                Utilites.SetVariable(__instance, "mantenerBotonMagia", true);

                string player = Utilites.GetPlayer(__instance.gameObject);
                Debug.Log(player);

                switch (player)
                {
                    case "Aki":
                        colliderMagia = UnityEngine.Object.Instantiate(Resources.Load("Personajes/000_Aki/obj_CollMag_Calentar")) as GameObject;
                        colliderMagia.transform.position = __instance.gameObject.transform.position;
                        Utilites.SetVariable(__instance, "colliderMagia", colliderMagia);
                        Utilites.SetVariable(__instance, "haciendoMagia", true);
                        break;
                    case "Prins":
                        colliderMagia = UnityEngine.Object.Instantiate(Resources.Load("Personajes/004_Prins/obj_CollMag_Telequinesia")) as GameObject;
                        colliderMagia.transform.position = __instance.gameObject.transform.position + new Vector3(0f, 0.5f, 0f);
                        Utilites.SetVariable(__instance, "colliderMagia", colliderMagia);
                        Utilites.SetVariable(__instance, "haciendoMagia", true);
                        break;
                    case "Yami":
                        //not working yet
                        break;
                    case "Hikari":
                        //not working yet
                        break;
                }
                return false;
            }
            return true;
        }
    }

    



    [HarmonyLib.HarmonyPatch(typeof(MotherBrain), "get_pulsa_ATACAR_down")]
    static class AttackPatch
    {
        static bool Prefix()
        {
            return false;
        }

    }


    [HarmonyLib.HarmonyPatch(typeof(MotherBrain), "get_pulsa_MAGIA_down")]
    static class MagicPatch
    {
        static bool Prefix()
        {
            return false;
        }

    }

    [HarmonyLib.HarmonyPatch(typeof(CameraFollow), "setObjetivo")]
    static class CameraPatch
    {
        static bool Prefix(GameObject o, CameraFollow __instance)
        {
            if (__instance.name == "SecondCamera")
            {

                Transform transform = o.transform.Find("NoDestruir/CubeCamPlayerND");
                if (transform != null)
                {
                    Traverse.Create(__instance).Field("objetivo").SetValue(transform.gameObject);
                }
                return false;
            }

            return true;
        }

    }

    [HarmonyLib.HarmonyPatch(typeof(CameraFollow), "reiniciar")]
    static class ResetCameraPatch
    {
        static bool Prefix(CameraFollow __instance)
        {
            if (__instance.name == "SecondCamera")
            {

                __instance.reiniciarObjetivo(GameObject.Find("SecondPlayer"));
                if (__instance.centroRotacion != null)
                {
                    __instance.setVelocidadCamara(30f);
                }
                return false;
            }

            return true;
        }

    }

    [HarmonyLib.HarmonyPatch(typeof(TelequinesiaMover), "getInputKeyboard")]
    static class TelequinesisKeyboardPatch
    {
        static bool Prefix(TelequinesiaMover __instance, ref float[] __result)
        {
            if (__instance.gameObject.name.Contains("SecondPlayer"))
            {
                float[] array = new float[2];
                if (Input.GetKey(KeyCode.K))
                {
                    array[1] = -1f;
                }
                if (Input.GetKey(KeyCode.I))
                {
                    array[1] = 1f;
                }
                if (Input.GetKey(KeyCode.J))
                {
                    array[0] = -1f;
                }
                if (Input.GetKey(KeyCode.L))
                {
                    array[0] = 1f;
                }


                __result = array;

                return false;
            }

            return true;
        }

    }

    [HarmonyLib.HarmonyPatch(typeof(TelequinesiaMover), "Start")]
    static class TelequinesisStartPatch
    {
        public static void Postfix(TelequinesiaMover __instance)
        {

        }

    }


    [HarmonyLib.HarmonyPatch(typeof(TelequinesiaMover), "Desposeer")]
    static class TelequinesisDesposeerPatch
    {
        public static bool Prefix(TelequinesiaMover __instance)
        {
            return false;
        }

    }


    [HarmonyLib.HarmonyPatch(typeof(CollMag_Teleq), "Start")]
    static class CollMag_TeleqStartPatch
    {
        public static void Postfix(CollMag_Teleq __instance)
        {
            if (!__instance.name.Contains("SecondPlayer"))
                return;

            __instance.player = GameObject.Find("SecondPlayer").GetComponent<PlayerCTRL>();

            Traverse.Create(__instance).Field("dir").SetValue(__instance.player.getDirection());

        }

    }

    [HarmonyPatch(typeof(CollMag_Teleq), "OnTriggerEnter")]
    public static class OnTriggerEnterPatch
    {
        public static void Postfix(Collider col, CollMag_Teleq __instance)
        {
            if (__instance.name.Contains("SecondPlayer"))
            {
                if (!col.name.Contains("SecondPlayer"))
                    col.name += "SecondPlayer";
            }
            else
            {
                col.name = col.name.Replace("SecondPlayer", "");
            }

        }
    }
}
