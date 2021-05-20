using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class ObjectPool : NetworkBehaviour
    {
        [SerializeField] private GameObject pooledObject;


        public override void OnStartServer()
        {
            //Initialize object pool here
        }
    }
}