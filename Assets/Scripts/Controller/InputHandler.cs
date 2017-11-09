using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {
    
    public float vertical;
    public float horizontal;

    bool b_input;
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


    StateManager states;
    CameraManager cameraManager;

    float delta;

	// Use this for initialization
	void Start () {

        cameraManager = CameraManager.singleton;
        cameraManager.Init(this.transform);

        states = GetComponent<StateManager>();
        states.Init();

	}
	
	// Update is called once per frame
	void FixedUpdate () {
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
    }

    void GetInput() {

        //Get input from buttons and axises
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        b_input = Input.GetButton("B");
        a_input = Input.GetButton("A");
        y_input = Input.GetButtonUp("Y");
        x_input = Input.GetButton("X");

        rt_input = Input.GetButton("RT");
        rt_axis = Input.GetAxis("RT");
        
        //Even if you're not pressing button, but there is movement return true
        if (rt_axis != 0)
        {
            rt_input = true;
        }

        lt_input = Input.GetButton("LT");
        lt_axis = Input.GetAxis("LT");
        if (lt_axis != 0)
        {
            lt_input = true;
        }

        rb_input = Input.GetButton("RB");
        lb_input = Input.GetButton("LB");

        rightAxis_down = Input.GetButtonUp("L");
        Debug.Log("rightAxis_down: " + rightAxis_down);
    }

    void UpdateStates() {

        states.horizontal = horizontal;
        states.vertical = vertical;

        Vector3 vertical_Movement = vertical * cameraManager.transform.forward;
        Vector3 horizontal_Movement = horizontal * cameraManager.transform.right;

        states.moveDirection = (vertical_Movement + horizontal_Movement).normalized;

        //Smooth transition between slow and fast movement animations
        float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        states.moveAmount = Mathf.Clamp01(m);

        states.rollInput = b_input;

        if (b_input)
        {
            //states.isRunning = (states.moveAmount > 0);
        }
        else
        {
            //states.isRunning = false;
        }

        //Update input states
        states.rt = rt_input;
        states.lt = lt_input;
        states.rb = rb_input;
        states.lb = lb_input;

        if (y_input)
        {
            states.isTwoHanded = !states.isTwoHanded;
            states.HandleTwoHanded();
        }

        if (rightAxis_down)
        {
            states.lockOn = !states.lockOn;

            //If there is no target transform to lock on, set status to false
            if (states.lockOnTarget == null)
            {
                states.lockOn = false;
            }

            cameraManager.lockOnTarget = states.lockOnTarget.transform;
            cameraManager.lockOn = states.lockOn;
        }
    }
}
