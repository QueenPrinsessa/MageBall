using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MatchHandler : NetworkBehaviour
    {

        private Vector3 ballStartPosition;
        private Quaternion ballStartRotation;
        private Rigidbody ballRigidbody;

        private GameObject ball;
        private NetworkManagerMageBall networkManager;

        private NetworkManagerMageBall NetworkManager
        {
            get
            {
                if (networkManager != null)
                    return networkManager;

                return networkManager = Mirror.NetworkManager.singleton as NetworkManagerMageBall;
            }
        }

        public override void OnStartServer()
        {
            ball = GameObject.FindGameObjectWithTag(Tags.BallTag);
            ballStartPosition = ball.transform.position;
            ballStartRotation = ball.transform.rotation;
            ballRigidbody = ball.GetComponent<Rigidbody>();

            Goal[] goals = FindObjectsOfType<Goal>();

            foreach (Goal goal in goals)
                goal.score += OnScore;
        }

        [ServerCallback]
        private void OnDestroy()
        {
            Goal[] goals = FindObjectsOfType<Goal>();

            foreach (Goal goal in goals)
                goal.score -= OnScore;
        }

        [Server]
        private void OnScore(Team team)
        {
            Debug.Log("Reset triggered");
            ResetBallAndPlayerToSpawnPositions();
        }

        [Server]
        public void ResetBallAndPlayerToSpawnPositions()
        {
            foreach (NetworkGamePlayerMageBall networkGamePlayer in NetworkManager.NetworkGamePlayers)
            {
                Debug.Log($"Resetting player {networkGamePlayer.DisplayName}");
                networkGamePlayer.TargetResetPosition(); 
            }

            ball.transform.position = ballStartPosition;
            ball.transform.rotation = ballStartRotation;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

    }
}