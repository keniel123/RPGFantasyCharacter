using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InputHandler : MonoBehaviour
    {

        public float vertical;
        public float horizontal;

        public bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        //Bumpers and triggers for XBOX controler
        bool rb_input;  //Right bumper
        bool rt_input; //Right trigger
        float rt_axis;

        bool lb_input;  //Left bumper
        bool lt_input; //Left trigger
        float lt_axis;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float rt_timer;
        float lt_timer;

        float sprintDelay = 0.3f;
        StateManager states;
        CameraManager cameraManager;

        float delta;

        // Use this for initialization
        void Start()
        {
            QuickSlot.Instance.Init();

            states = GetComponent<StateManager>();
            states.Init();

            cameraManager = CameraManager.singleton;
            cameraManager.Init(states);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();

            //Update the state manager
            states.FixedTick(Time.deltaTime);

            //Update the camera manager
            cameraManager.Tick(delta);
        }

        private void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
            ResetInputAndStates();

        }

        void GetInput()
        {

            //Get input from buttons and axises
            vertical = Input.GetAxis(StaticStrings.Vertical);
            horizontal = Input.GetAxis(StaticStrings.Horizontal);

            b_input = Input.GetButton(StaticStrings.B);
            a_input = Input.GetButton(StaticStrings.A);
            y_input = Input.GetButtonUp(StaticStrings.Y);
            x_input = Input.GetButton(StaticStrings.X);

            rt_input = Input.GetButton(StaticStrings.RT);
            rt_axis = Input.GetAxis(StaticStrings.RT);

            //Even if you're not pressing button, but there is movement return true
            if (rt_axis != 0)
            {
                rt_input = true;
            }

            lt_input = Input.GetButton(StaticStrings.LT);
            lt_axis = Input.GetAxis(StaticStrings.LT);
            if (lt_axis != 0)
            {
                lt_input = true;
            }

            rb_input = Input.GetButton(StaticStrings.RB);
            lb_input = Input.GetButton(StaticStrings.LB);

            rightAxis_down = Input.GetButtonUp(StaticStrings.L) || Input.GetKeyUp(KeyCode.T);

            if (b_input)
            {
                b_timer += delta;
            }
        }

        void UpdateStates()
        {

            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 vertical_Movement = vertical * cameraManager.transform.forward;
            Vector3 horizontal_Movement = horizontal * cameraManager.transform.right;

            states.moveDirection = (vertical_Movement + horizontal_Movement).normalized;

            //Smooth transition between slow and fast movement animations
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            states.moveAmount = Mathf.Clamp01(m);


            if (x_input)
            {
                b_input = false;
            }

            if (b_input && b_timer > sprintDelay)
            {
                states.isRunning = (states.moveAmount > 0);
            }

            if (b_input == false && b_timer > 0 && b_timer < sprintDelay)
            {
                states.rollInput = true;
            }

            //Update input states
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;
            states.itemInput = x_input;

            if (y_input)
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            //Check locked on target's status
            if (states.lockOnTarget != null)
            {
                //If the enemy is dead, then stop locking the camera
                if (states.lockOnTarget.eStates.isDead)
                {
                    Debug.Log("Locked on target DIED!");
                    states.lockOn = false;
                    states.lockOnTarget = null;
                    states.lockOnTransform = null;
                    cameraManager.lockOn = false;
                    cameraManager.lockOnTarget = null;
                }
            }
            else
            {
                states.lockOn = false;
                states.lockOnTarget = null;
                states.lockOnTransform = null;
                cameraManager.lockOn = false;
                cameraManager.lockOnTarget = null;
            }


            if (rightAxis_down)
            {
                states.lockOn = !states.lockOn;

                states.lockOnTarget = EnemyManager.Instance.GetEnemy(transform.position);
                //If there is no target transform to lock on, set status to false
                if (states.lockOnTarget == null)
                {
                        states.lockOn = false;
                }
                
                cameraManager.lockOnTarget = states.lockOnTarget;
                states.lockOnTransform = states.lockOnTarget.GetTarget();
                cameraManager.lockOnTransform = states.lockOnTransform;
                cameraManager.lockOn = states.lockOn;
            }
        }

        void ResetInputAndStates()
        {

            //Reset the inputs for next frame
            if (b_input == false)
            {
                b_timer = 0;
            }

            if (states.rollInput)
            {
                states.rollInput = false;
            }

            if (states.isRunning)
            {
                states.isRunning = false;
            }
        }
    }
}