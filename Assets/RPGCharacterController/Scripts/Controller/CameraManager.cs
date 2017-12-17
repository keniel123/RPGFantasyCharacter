using RPGController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    //There can only be 1 camera manager
    public static CameraManager Instance;

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

    public float defaultZ;
    float currentZ;
    public float zSpeed = 19;

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
        Instance = this;
    }

    public void Init(StateManager st) {
        states = st;
        target = st.transform;

        //camTransform = Camera.main.transform;
        //pivot = camTransform.parent;
        currentZ = defaultZ;
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
                    lockOnTransform = lockOnTarget.GetTarget((c_h>  0));
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
        HandlePivotPosition();
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

    void HandlePivotPosition() {

        float targetZ = defaultZ;
        CameraCollision(defaultZ, ref targetZ);

        currentZ = Mathf.Lerp(currentZ, targetZ, states.delta * zSpeed);
        Vector3 targetPos = Vector3.zero;
        targetPos.z = currentZ;
        camTransform.localPosition = targetPos;
    }

    void CameraCollision(float targetZ, ref float actualZ)
    {
        float step = Mathf.Abs(targetZ);
        int stepCount = 5;
        float stepIncrement = step / stepCount;

        RaycastHit hit;
        Vector3 origin = pivot.position;
        Vector3 direction = -pivot.forward;   //The opposite way of pivot is looking at

        Debug.DrawRay(origin, direction * step, Color.blue);
        if (Physics.Raycast(origin, direction, out hit, step, states.ignoreForGroundCheck))
        {
            Debug.Log(hit.transform.root.name);
            //When raycast hits a target, it'll put it in half of the distance
            float distance = Vector3.Distance(hit.point, origin);
            actualZ = -(distance / 2);
        }
        //If the camera is really close to an object
        else
        {
            for (int i = 0; i < stepCount + 1; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector3 dir = Vector3.zero;
                    Vector3 secondOrigin = origin + (dir * i) * stepIncrement;

                    switch (k)
                    {
                        case 0:
                            dir = camTransform.right;
                            break;
                        case 1:
                            dir = -camTransform.right;
                            break;
                        case 2:
                            dir = camTransform.up;
                            break;
                        case 3:
                            dir = -camTransform.up;
                            break;
                    }


                    Debug.DrawRay(secondOrigin, dir * 0.2f, Color.red);
                    if (Physics.Raycast(secondOrigin, dir, out hit, 0.2f, states.ignoreForGroundCheck))
                    {
                        float distance = Vector3.Distance(secondOrigin, origin);
                        actualZ = -(distance / 2);
                        //If you are closer than 0.2, reset the Z
                        if (actualZ < 0.2f)
                        {
                            actualZ = 0;
                        }

                        return;
                    }
                }
            }
        }
    }
}
