using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.MelonLogger;
using UnityEngine;
using static HarmonyLib.Code;

namespace FlatworldMP
{
    internal class Utilites
    {
        public static void InvokePrivateFunction<T>(object instance, string functionName, object[] parameters)
        {
            var method = AccessTools.Method(typeof(T), functionName);

            method?.Invoke(instance, parameters);
        }


        public static void SetVariable(object instance, string name, object value)
        {
            Traverse.Create(instance).Field(name).SetValue(value);
        }

        public static string GetPlayer(GameObject Player)
        {
            string[] names = { "Aki", "Prins", "Yami", "Hikari" };

            return names.FirstOrDefault(sub => Player.GetComponentsInChildren<Transform>().Any(x => x.name.Contains(sub)));
        }
    }
}
