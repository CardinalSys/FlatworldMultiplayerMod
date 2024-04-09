using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using System.Collections;
using Rewired;
using static UnityEngine.GraphicsBuffer;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace FlatworldMP
{
    public class Multiplayer : MelonMod
    {
        private GameObject playerObj;
        private PlayerCTRL playerCtrl;

        private List<GameObject> playerModels = new List<GameObject>();

        private GameObject secondPlayerObj;
        private PlayerCTRL secondPlayerCtrl;

        private GameObject secondCamera;

        private bool firstPlayerCanAttack = true;
        private bool secondPlayerCanAttack = true;

        private GameObject firstCamera;

        public string SecondPlayerCharacter;

        private bool usingMagic = false;
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Default" || sceneName == "Intro" || sceneName == "SelectFile" || sceneName == "TitleScreen" || sceneName == "Configtitlescreen" || sceneName == "BootGame")
            {
                if (secondPlayerObj != null)
                    GameObject.Destroy(secondPlayerObj);
                return;
            }

            LoggerInstance.Msg(sceneName);
            if (GetPlayerData() && secondPlayerObj == null)
            {
                Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(o => { SpawnSecondPlayer(); });
            }

            if (secondPlayerObj != null)
            {
                Task.Delay(TimeSpan.FromSeconds(1.1)).ContinueWith(o => { ResetSecondPlayerPosition(); });
            }
        }

        private void ResetSecondPlayerPosition()
        {
            secondPlayerObj.transform.position = playerObj.transform.position;
            secondCamera.transform.position = secondPlayerObj.transform.position;
        }

        public override void OnUpdate()
        {
            FirstPlayerController();
            SecondPlayerController();

            CheckForSplitScreen();

            base.OnUpdate();
        }


        private void FirstPlayerController()
        {
            if (Input.GetKeyDown(KeyCode.X) && playerObj != null && !Input.GetKey(KeyCode.C) && firstPlayerCanAttack)
            {
                firstPlayerCanAttack = false;

                var codigoAtacarMethod = AccessTools.Method(typeof(PlayerCTRL), "codigoAtacar");

                codigoAtacarMethod.Invoke(playerCtrl, null);

                Task.Delay(TimeSpan.FromMilliseconds(300)).ContinueWith(o => { AttackDelay(ref firstPlayerCanAttack); });

            }

            if (Input.GetKeyDown(KeyCode.C) && playerObj != null && !Input.GetKey(KeyCode.X))
            {
                var codigoMagiarMethod = AccessTools.Method(typeof(PlayerCTRL), "codigoMagiar");

                codigoMagiarMethod.Invoke(playerCtrl, null);
            }
        }

        private void SecondPlayerController()
        {
            if (Input.GetKeyDown(KeyCode.O) && secondPlayerObj != null && !Input.GetKey(KeyCode.P) && secondPlayerCanAttack)
            {
                secondPlayerCanAttack = false;
                var codigoAtacarMethod = AccessTools.Method(typeof(PlayerCTRL), "codigoAtacar");

                codigoAtacarMethod.Invoke(secondPlayerCtrl, null);

                Task.Delay(TimeSpan.FromMilliseconds(300)).ContinueWith(o => { AttackDelay(ref secondPlayerCanAttack); });
            }

            if (Input.GetKeyDown(KeyCode.P) && secondPlayerObj != null && !Input.GetKey(KeyCode.O))
            {
                var codigoMagiarMethod = AccessTools.Method(typeof(PlayerCTRL), "codigoMagiar");

                codigoMagiarMethod.Invoke(secondPlayerCtrl, null);

                usingMagic = true;
            }


            if (!Input.GetKey(KeyCode.P) && usingMagic)
            {
                usingMagic = false;
                var codigoDesMagiarMethod = AccessTools.Method(typeof(PlayerCTRL), "EndMagic");

                codigoDesMagiarMethod.Invoke(secondPlayerCtrl, null);

                GameObject.Destroy(GameObject.Find("SecondPlayerObj_CollMag_Calentar"));

                var codigoDesposeerMethod = AccessTools.Method(typeof(PlayerCTRL), "EndMagic");

                codigoDesMagiarMethod.Invoke(secondPlayerCtrl, null);

                //Desposeer objeto
            }

        }




        private bool SpawnSecondPlayer()
        {

            secondPlayerObj = GameObject.Instantiate(playerObj);
            secondPlayerObj.SetActive(true);


            if (secondPlayerObj == null) return false;

            secondPlayerObj.name = "SecondPlayer";
            secondPlayerCtrl = secondPlayerObj.GetComponent<PlayerCTRL>();

            //Hardcode walkSpeed to match runSpeed.
            Traverse.Create(secondPlayerCtrl).Field("walkSpeed").SetValue(5);

            GameObject target = null;

            foreach (Transform g in secondPlayerObj.GetComponentsInChildren<Transform>())
            {
                if (g.name == "CubeCamPlayerND")
                {
                    target = g.gameObject;
                    break;
                }
            }

            if (Camera.main != null)
            {

                firstCamera = Camera.main.gameObject;
                secondCamera = GameObject.Instantiate(firstCamera);

                secondCamera.name = "SecondCamera";
            }



            return true;
        }


        private bool GetPlayerData()
        {
            if (playerObj != null)
                return true;
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
                return false;
            playerCtrl = playerObj.GetComponent<PlayerCTRL>();

            UpdatePlayerModels();

            return true;
        }

        private bool UpdatePlayerModels()
        {
            GameObject CHAR = GameObject.Find("CHAR");
            if (CHAR == null) return false;
            playerModels.AddRange(CHAR.GetComponentsInChildren<GameObject>());

            if (playerModels.Count > 0) return true;
            else return false;
        }

        private void AttackDelay(ref bool atk)
        {
            atk = true;
        }

        private void CheckForSplitScreen()
        {
            if (playerObj == null || secondPlayerObj == null)
            {
                return;
            }

            float x = Mathf.Abs(playerObj.transform.position.x - secondPlayerObj.transform.position.x);
            float z = Mathf.Abs(playerObj.transform.position.z - secondPlayerObj.transform.position.z);

            if (x >= 3.2f || z > 4.9f)
            {
                ToggleSplitScreen(true);
            }
            else
            {
                ToggleSplitScreen(false);
            }
        }
        private void ToggleSplitScreen(bool state)
        {
            secondCamera.SetActive(state);
            if (state)
            {
                firstCamera.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
                secondCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
            }
            else
            {
                firstCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            }
        }

    }


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
                float direction = (float)Traverse.Create(__instance).Field("direction").GetValue();


                MotherBrain motherbrain = MotherBrain.Instance;

                if (!(colliderMagia == null))
                {
                    return false;
                }

                Traverse.Create(__instance).Field("mantenerBotonMagia").SetValue(true);

                foreach (Transform c in __instance.GetComponentsInChildren<Transform>())
                {
                    if (c.name.StartsWith("Aki"))
                    {
                        colliderMagia = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Personajes/000_Aki/obj_CollMag_Calentar"));
                        colliderMagia.name = "SecondPlayerObj_CollMag_Calentar";
                        colliderMagia.transform.position = __instance.transform.position;
                        Traverse.Create(__instance).Field("haciendoMagia").SetValue(true);
                        break;
                    }
                    else if (c.name.StartsWith("Prins"))
                    {
                        colliderMagia = UnityEngine.Object.Instantiate(Resources.Load("Personajes/004_Prins/obj_CollMag_Telequinesia"), __instance.transform.position + new Vector3(0, 0.5f, 0) + __instance.transform.forward, __instance.transform.rotation) as GameObject;
                        colliderMagia.name = "SecondPlayerObj_CollMag_Telequinesia";
                        Traverse.Create(__instance).Field("haciendoMagia").SetValue(true);
                        break;
                    }
                    else if (c.name.StartsWith("Yami"))
                    {
                        if (motherbrain.CharGetEquippedMagic("Yami") == 0)
                        {
                            colliderMagia = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Personajes/002_Yami/obj_CollMag_Eletricidad"));
                            Vector3 vector = new Vector3(Mathf.Cos(direction * ((float)Math.PI / 180f)) * 1f, 1f, Mathf.Sin(direction * ((float)Math.PI / 180f)) * 1f);
                            colliderMagia.transform.position = __instance.transform.position + vector;
                        }
                        else if (motherbrain.CharGetEquippedMagic("Yami") == 1)
                        {
                            colliderMagia = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Personajes/002_Yami/obj_CollMag_Luz"), __instance.transform);
                            float num = 0f - __instance.transform.localEulerAngles.y + 90f;
                            Vector3 vector2 = new Vector3(Mathf.Cos(num * ((float)Math.PI / 180f)) * 1f, 1f, Mathf.Sin(num * ((float)Math.PI / 180f)) * 1f);
                            colliderMagia.transform.position = __instance.transform.position + vector2;
                        }

                        Traverse.Create(__instance).Field("haciendoMagia").SetValue(true);
                        break;
                    }
                    else if (c.name.StartsWith("Hikari"))
                    {
                        Traverse.Create(__instance).Field("armaSacada").SetValue(false);
                        Traverse.Create(__instance).Field("mantenerBotonMagia").SetValue(false);
                        Traverse.Create(__instance).Field("hikariTocaLaFlautaPausa").SetValue(true);

                        colliderMagia = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Personajes/001_Hikari/Instrumentos/Instrumento" + motherbrain.CharGetSkn2("Hikari")));
                        colliderMagia.transform.position = __instance.transform.position;
                        colliderMagia.transform.eulerAngles = __instance.transform.eulerAngles;
                        Traverse.Create(__instance).Field("haciendoMagia").SetValue(true);
                        break;
                    }
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
            return true;
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
                if(!col.name.Contains("SecondPlayer"))
                    col.name += "SecondPlayer";
            }
            else
            {
                col.name = col.name.Replace("SecondPlayer", "");
            }

        }
    }
}