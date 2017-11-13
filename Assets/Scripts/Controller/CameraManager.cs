using RPGController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    //There can only be 1 camera manager
    public static CameraManager singleton;

    public float followSpeed = 10;
    public float mouseSpeed = 2;
    public float controllerSpeed = 2;

    public bool lockOn;

    public Transform target;
    public EnemyTarget lockOnTarget;
    public Transform lockOnTransform;

    public Transform pivot;
    public Transform camTransform;
    StateManager states;

    float turnSmoothing = 0.1f;
    float minAngle = -35f;
    float maxAngle = 35f;

    float smoothX;
    float smoothY;

    float smoothXVelocity;
    float smoothYVelocity;

    public float lookAngle;
    public float tiltAngle;

    bool usedRightAxis;
    bool changeTargetLeft;
    bool changeTargetRight;

    void Awake() {
        singleton = this;
    }

    public void Init(StateManager st) {
        states = st;
        target = st.transform;

        camTransform = Camera.main.transform;
        pivot = camTransform.parent;
    }

    public void Tick(float d) {

        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        float c_h = Input.GetAxis("RightAxis X");
        float c_v = Input.GetAxis("RightAxis Y");

        float targetSpeed = mouseSpeed;

        changeTargetLeft = Input.GetKeyUp(KeyCode.V);
        changeTargetRight = Input.GetKeyUp(KeyCode.B);


        if (lockOnTarget != null)
        {
            if (lockOnTransform == null)
            {
                lockOnTransform = lockOnTarget.GetTarget();
                states.lockOnTransform = lockOnTransform;
            }

            if (Mathf.Abs(c_h) > 0.6f)
            {
                if (!usedRightAxis)
                {
                    lockOnTransform = lockOnTarget.GetTarget((c_h>0));
                    states.lockOnTransform = lockOnTransform;
                    usedRightAxis = true;
                }
            }

            if (changeTargetLeft || changeTargetRight)
            {
                lockOnTransform = lockOnTarget.GetTarget(changeTargetLeft);
                states.lockOnTransform = lockOnTransform;
            }
        }

        if (usedRightAxis)
        {
            if (Mathf.Abs(c_h) < 0.6f)
            {
                usedRightAxis = false;
            }

        }

        if (c_h != 0 || c_v != 0)
        {
            h = c_h;
            v = -c_v;
            targetSpeed = controllerSpeed;
        }

        FollowTarget(d);
        HandleRotations(d, v, h, targetSpeed);
    }

    void FollowTarget(float d) {

        float speed = d * followSpeed;
        Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
        transform.position = targetPosition;
    }

    void HandleRotations(float deltaTime, float v, float h, float targetSpeed) {

        if (turnSmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXVelocity, turnSmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYVelocity, turnSmoothing);
        }
        else
        {
            smoothX = h;
            smoothY = v;
        }

        tiltAngle -= smoothY * targetSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
        pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

        //If the player has a locked on a target
        if (lockOn && lockOnTarget!= null)
        {
            //Get the direction of locked on target
            Vector3 targetDir = lockOnTransform.position - transform.position;
            targetDir.Normalize();
            //targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, deltaTime * 9);
                lookAngle = transform.eulerAngles.y;

            return;
        }

        lookAngle += smoothX * targetSpeed;
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);

    }
}
