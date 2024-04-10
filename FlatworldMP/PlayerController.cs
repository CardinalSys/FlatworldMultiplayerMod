using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace FlatworldMP
{
    public abstract class PlayerController : MelonMod
    {
        private bool canAttack = true;
        public PlayerCTRL playerCtrl { get; set; }
        public GameObject playerObj { get; set; }

        public KeyCode AttackKey;
        public KeyCode MagicKey;

        private int attackDelay = 300;

        public void Attack()
        {
            if (Input.GetKeyDown(AttackKey) && playerObj != null && !Input.GetKey(MagicKey) && canAttack)
            {
                canAttack = false;

                Utilites.InvokePrivateFunction<PlayerCTRL>(playerCtrl, "codigoAtacar", null);            
                Task.Delay(TimeSpan.FromMilliseconds(attackDelay)).ContinueWith(o => { AttackDelay(ref canAttack); });

            }
        }

        public void Magic()
        {
            if (Input.GetKeyDown(MagicKey) && playerObj != null && !Input.GetKey(AttackKey))
            {
                Utilites.InvokePrivateFunction<PlayerCTRL>(playerCtrl, "codigoMagiar", null);
            }
        }

        private void AttackDelay(ref bool canAttack)
        {
            canAttack = true;
        }

        public void SetupController(KeyCode _AttackKey, KeyCode _MagicKey)
        {
            AttackKey = _AttackKey;
            MagicKey = _MagicKey;
        }
    }

    public class FirstPlayer : PlayerController 
    {
        public FirstPlayer()
        {
            SetupController(KeyCode.X, KeyCode.C);
            SetupFirstPlayer();
        }

        private void SetupFirstPlayer()
        {
            playerObj = GetPlayer();
        }

        public GameObject GetPlayer()
        {
            GameObject _playerObj = GameObject.Find("obj_player");
            playerCtrl = _playerObj.GetComponent<PlayerCTRL>();

            return _playerObj;
        }
    }

    public class SecondPlayer : PlayerController
    {
        public SecondPlayer()
        {
            SetupController(KeyCode.O, KeyCode.P);
            Spawn();
            //ResetPosition();
        }

        private void Spawn()
        {
            playerObj = InstantiateSecondPlayer();
            playerCtrl = playerObj.GetComponent<PlayerCTRL>();

            //Hardcode walkSpeed to match runSpeed. (temporal)
            Traverse.Create(playerCtrl).Field("walkSpeed").SetValue(5);
        }

        private GameObject InstantiateSecondPlayer()
        {
            GameObject firstPlayer = new FirstPlayer().GetPlayer();

            if(!firstPlayer)
                return null;

            GameObject secondPlayer = GameObject.Instantiate(firstPlayer, firstPlayer.transform.position, firstPlayer.transform.rotation);

            if (!secondPlayer)
                return null;

            secondPlayer.name = "SecondPlayer";

            return secondPlayer;

        }


        public void ResetPosition()
        {
            playerObj.transform.position = new FirstPlayer().GetPlayer().transform.position;
            //camera.transform.position = playerObj.transform.position;
        }

        public void CheckForStopMagic()
        {
            if((bool)Traverse.Create(playerCtrl).Field("haciendoMagia").GetValue() && !Input.GetKey(MagicKey))
            {
                Utilites.SetVariable(playerCtrl, "haciendoMagia", false);
                GameObject colliderMagia = Traverse.Create(playerCtrl).Field("colliderMagia").GetValue() as GameObject;
                colliderMagia.transform.Find("obj_CollMag_Calentar_hijo").GetComponent<CollMag_Calentar>().Morir();
            }
                
        }
    }
}
