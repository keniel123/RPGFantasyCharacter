using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    //There can only be 1 camera manager
    public static CameraManager singleton;

    public float followSpeed = 10;
    public float mouseSpeed = 2;

    public bool lockOn;

    public Transform target;
    public Transform pivot;
    public Transform camTransform;

    float turnSmoothing = 0.1f;
    float minAngle = -35f;
    float maxAngle = 35f;

    float smoothX;
    float smoothY;

    float smoothXVelocity;
    float smoothYVelocity;

    public float lookAngle;
    public float tiltAngle;

    void Awake() {
        singleton = this;
    }

    public void Init(Transform t) {
        target = t;

        camTransform = Camera.main.transform;
        pivot = camTransform.parent;
    }

    public void Tick(float d) {

        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        float targetSpeed = mouseSpeed;
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

        if (lockOn)
        {

        }

        lookAngle += smoothX * targetSpeed;
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);

        tiltAngle -= smoothY * targetSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
        pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
    }
}
