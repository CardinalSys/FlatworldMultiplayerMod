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

namespace FlatworldMP
{
    public class Multiplayer : MelonMod
    {
        private GameObject playerObj;
        private PlayerCTRL playerCtrl;
        private CharacterController playerCharacterController;
        private List<GameObject> playerModels = new List<GameObject>();

        private GameObject secondPlayerObj;
        private PlayerCTRL secondPlayerCtrl;

        private GameObject secondCamera;

        private bool firstPlayerCanAttack = true;
        private bool secondPlayerCanAttack = true;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Intro" || sceneName == "SelectFile" || sceneName == "TitleScreen" || sceneName == "Congititlescreen")
                return;

            if (GetPlayerData() && secondPlayerObj == null)
            {
                SpawnSecondPlayer();
            }
        }

        public override void OnUpdate()
        {


            FirstPlayerController();
            SecondPlayerController();


            if (Input.GetKeyDown(KeyCode.O) && secondPlayerObj == null)
            {
                if (!SpawnSecondPlayer())
                    LoggerInstance.Msg("Error spawning second player");

                LoggerInstance.Msg("Second player spawned");
            }

            if (secondPlayerObj != null)
                Traverse.Create(secondPlayerObj).Field("warps").SetValue(null);



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
            }


            if (!Input.GetKey(KeyCode.P))
            {
                var codigoDesMagiarMethod = AccessTools.Method(typeof(PlayerCTRL), "EndMagic");

                codigoDesMagiarMethod.Invoke(secondPlayerCtrl, null);        
            };

        }


        private bool SpawnSecondPlayer() {
            secondPlayerObj = GameObject.Instantiate(playerObj);

            if (secondPlayerObj == null) return false;

            secondPlayerObj.name = "SecondPlayer";
            secondPlayerCtrl = secondPlayerObj.GetComponent<PlayerCTRL>();

            //Hardcode walkSpeed to match runSpeed.
            Traverse.Create(secondPlayerCtrl).Field("walkSpeed").SetValue(5);

            GameObject target = null;

            foreach (Transform g in secondPlayerObj.GetComponentsInChildren<Transform>())
            {
                if(g.name == "CubeCamPlayerND")
                {
                    target = g.gameObject;
                    break;
                }
            }

            if(Camera.main != null)
            {
                secondCamera = GameObject.Instantiate(Camera.main.gameObject);
                Camera.main.rect = new Rect(0, 0, 0.5f, 1);
                secondCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);

                CameraFollow secondCameraFollow = secondCamera.GetComponent<CameraFollow>();

                secondCamera.name = "SecondCamera";
            }



            return true;
        }


        private bool GetPlayerData()
        {
            playerObj = GameObject.Find("obj_player");
            if (playerObj == null)
                return false;
            playerCtrl = playerObj.GetComponent<PlayerCTRL>();

            playerCharacterController = playerObj.GetComponent<CharacterController>();
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

        void AttackDelay(ref bool atk)
        {
            atk = true;
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
            if(__instance.name == "SecondCamera")
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

    

}


