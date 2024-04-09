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
        private FirstPlayer firstPlayer;
        private SecondPlayer secondPlayer;

        private string[] badScenes = { "Default", "Intro", "SelectFile", "TitleScreen", "Configtitlescreen", "BootGame" };
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (badScenes.Contains(sceneName))
                return;

            firstPlayer = new FirstPlayer();

            secondPlayer = new SecondPlayer();
        }


        public override void OnUpdate()
        {
            if (firstPlayer == null)
                return;

            if (firstPlayer.playerCtrl != null)
            {
                firstPlayer.Attack();
                firstPlayer.Magic();
            }

            if(secondPlayer.playerCtrl != null)
            {
                secondPlayer.Attack();
                secondPlayer.Magic();
                secondPlayer.CheckForStopMagic();
            }

            base.OnUpdate();
        }




    }


}