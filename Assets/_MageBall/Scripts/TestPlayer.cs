using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public GameObject forcePushPrefab;
    //[SerializeField] private float fireRate = 2f;
    //private float timeSinceLastCast = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //timeSinceLastCast += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) /*&& timeSinceLastCast >fireRate*/)
        {
            Instantiate(forcePushPrefab, transform.position, transform.rotation);
        }
    }
}
