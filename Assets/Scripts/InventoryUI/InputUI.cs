using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InputUI : MonoBehaviour
    {
        public static InputUI Instance;

        public float vertical;
        public float horizontal;

        //Buttons
        public bool b_input;
        public bool a_input;
        public bool x_input;
        public bool y_input;

        //Pad axises
        float d_y;
        float d_x;
        public bool d_up;
        public bool d_down;
        public bool d_right;
        public bool d_left;

        private void Awake()
        {
            Instance = this;
        }

        public void Tick() {
            GetInput();
        }

        void GetInput() {

            //Get input from buttons and axises
            vertical = Input.GetAxis(StaticStrings.Input_Vertical);
            horizontal = Input.GetAxis(StaticStrings.Input_Horizontal);

            b_input = Input.GetButton(StaticStrings.B);
            a_input = Input.GetButton(StaticStrings.A);
            y_input = Input.GetButtonUp(StaticStrings.Y);
            x_input = Input.GetButton(StaticStrings.X);

            //In keyboard item swtich is assigned to between 1-4
            d_up = Input.GetKeyUp(KeyCode.Alpha1) || d_y > 0;
            d_down = Input.GetKeyUp(KeyCode.Alpha2) || d_y < 0;
            d_left = Input.GetKeyUp(KeyCode.Alpha3) || d_x < 0;
            d_right = Input.GetKeyUp(KeyCode.Alpha4) || d_x > 0;
        }

    }
}