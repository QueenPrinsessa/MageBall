using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    [CreateAssetMenu(fileName = "Passive", menuName = "Passive", order = 1)]
    public class Passive : ScriptableObject
    {
        public float modifier = 1;
    }
}