using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace FlatworldMP
{
    public class CameraController
    {
        public Camera firstCamera { get; set; }
        public Camera secondCamera { get; set; }


        public CameraController() 
        {

            firstCamera = Camera.main;

            secondCamera = GameObject.Instantiate(firstCamera.gameObject).GetComponent<Camera>();
            secondCamera.name = "SecondCamera";
            secondCamera.enabled = false;
        }

        public void CheckDistance()
        {

            float x = Mathf.Abs(firstCamera.transform.position.x - secondCamera.transform.position.x);
            float z = Mathf.Abs(firstCamera.transform.position.z - secondCamera.transform.position.z);
            if (x >= 3.2f || z > 4.9f)
            {
                ToggleSplitScreen(true);
            }
            else
                ToggleSplitScreen(false);
        }

        private void ToggleSplitScreen(bool state)
        {
            if (state && !secondCamera.enabled)
            {
                secondCamera.enabled = true;
                firstCamera.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
                secondCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
            }
            else if (!state && secondCamera.enabled)
            {
                secondCamera.enabled = false;
                firstCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            }
        }
    }
}
