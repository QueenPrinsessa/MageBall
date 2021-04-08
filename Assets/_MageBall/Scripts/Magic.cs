using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magic : MonoBehaviour
{
    
    [SerializeField] private float PushSpeed = 10;
    [SerializeField] private float pushRayRange = 2.0f;
    private Vector3 pushLastPos;

    
    

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 2f);
        
    }

    // Update is called once per frame
    void Update()
    {

        pushLastPos = transform.position;
        transform.position += transform.forward * PushSpeed * Time.deltaTime;
        CheckHit();
    }

    void CheckHit()
    {
        RaycastHit hit;
        LayerMask ballMask = LayerMask.GetMask("Ball");
        
        if(Physics.Raycast(transform.position, transform.forward,out hit, pushRayRange, ballMask))
        {
            Debug.Log(hit.transform.name);

            if (hit.rigidbody !=null)
            {
                hit.rigidbody.AddForce(-hit.normal * PushSpeed);
                Destroy(gameObject);
            }
        }

    }
}
