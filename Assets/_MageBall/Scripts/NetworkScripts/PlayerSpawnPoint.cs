using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class PlayerSpawnPoint : MonoBehaviour
    {

        [SerializeField] private Team team;

        private void Awake()
        {
            PlayerSpawnSystem.AddSpawnPoint(team, transform);
        }

        private void OnDestroy()
        {
            PlayerSpawnSystem.RemoveSpawnPoint(team, transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = team == Team.Red ? Color.red : Color.blue;
            Gizmos.DrawSphere(transform.position, 1.0f);
        }
    }
}