using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class OpenObject : WorldInteraction
    {
        public GameObject obj;  //Can also be array
        public override void InteractActual()
        {
            obj.SetActive(true);
            base.InteractActual();
        }
    }
}