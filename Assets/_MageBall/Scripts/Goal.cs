using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    [RequireComponent(typeof(BoxCollider))]
    public class Goal : NetworkBehaviour
    {
        [SerializeField] private Team team;
        [SerializeField] private AudioSource audioSource;
        private BoxCollider goalCollider;
        public event Action<Team> score;

        public BoxCollider GoalCollider => goalCollider;

        public override void OnStartServer()
        {
            goalCollider = GetComponent<BoxCollider>();
        }

        [ServerCallback]
        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(Tags.BallTag))
                return;

            if (!goalCollider.bounds.Contains(other.bounds.min) && goalCollider.bounds.Contains(other.bounds.max))
                return;

            RpcPlayGoalSound();

            score?.Invoke(team);
        }

        [ClientRpc]
        private void RpcPlayGoalSound()
        {
            if (audioSource != null)
                audioSource.Play();
        }
    }
}