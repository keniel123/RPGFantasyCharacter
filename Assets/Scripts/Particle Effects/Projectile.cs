using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class Projectile : MonoBehaviour
    {
        Rigidbody rigid;

        public float horizontalSpeed = 5;
        public float verticalSpeed = 2;

        //Projectile Target
        public Transform target;

        public void Init()
        {
            rigid = GetComponent<Rigidbody>();

            Vector3 targetForce = transform.forward * horizontalSpeed;
            targetForce += transform.up * verticalSpeed;
            rigid.AddForce(targetForce, ForceMode.Impulse);
        }
        
    }
}