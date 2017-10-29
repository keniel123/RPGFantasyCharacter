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

    //Controller bumpers
    bool rb_input;
    float rt_axis;
    bool rt_input;
    bool lb_input;
    float lt_axis;
    bool lt_input;


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

        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        //Jump input
        b_input = Input.GetButton("b_input");

        //RT INPUT
        rt_input = Input.GetButton("RT");
        rt_axis = Input.GetAxis("RT");

        if (rt_axis != 0)
        {
            rt_input = true;
        }

        //LT INPUT
        lt_input = Input.GetButton("LT");
        lt_axis = Input.GetAxis("LT");

        if (lt_axis != 0)
        {
            lt_input = true;
        }

        //RB INPUT
        rb_input = Input.GetButton("RB");

        //LB INPUT
        lb_input = Input.GetButton("LB");
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

        if (b_input)
        {
            states.isRunning = (states.moveAmount > 0);
        }
        else
        {
            states.isRunning = false;
        }

        states.rt = rt_input;
        states.lt = lt_input;
        states.rb = rb_input;
        states.lb = lb_input;
    }
}
