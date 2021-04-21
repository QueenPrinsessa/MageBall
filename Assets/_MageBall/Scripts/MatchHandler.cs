using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MatchHandler : NetworkBehaviour
    {

        [SyncVar] private Vector3 ballStartPosition;
        [SyncVar] private Quaternion ballStartRotation;
        [SyncVar] private float ballInitialMass;
        private Rigidbody ballRigidbody;

        private GameObject ball;
        private NetworkManagerMageBall networkManager;
        private Goal[] goals;
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
            ballInitialMass = ballRigidbody.mass;

            goals = FindObjectsOfType<Goal>();

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
            SetGoalCollidersEnabled(false);
            StartCoroutine(ResetGameObjects());
        }

        [Server]
        private IEnumerator ResetGameObjects()
        {
            yield return new WaitForSeconds(NetworkManager.WaitBeforeResetAfterGoalInSeconds);
            ResetPlayerPosition();
            DestroySpellGameObjects();
            ResetBall();
            SetGoalCollidersEnabled(true);
        }

        [Server]
        private void SetGoalCollidersEnabled(bool enabled)
        {
            foreach (Goal goal in goals)
            {
                goal.GoalCollider.enabled = enabled;
            }
        }

        [Server]
        private void ResetPlayerPosition()
        {
            foreach (NetworkGamePlayerMageBall networkGamePlayer in NetworkManager.NetworkGamePlayers)
            {
                Debug.Log($"Resetting player {networkGamePlayer.DisplayName}");
                networkGamePlayer.TargetResetPlayer();
            }
        }

        [Server]
        private void DestroySpellGameObjects()
        {
            GameObject[] spells = GameObject.FindGameObjectsWithTag(Tags.SpellTag);

            foreach (GameObject spell in spells)
                NetworkServer.Destroy(spell);
        }

        [Server]
        private void ResetBall()
        {
            ball.transform.position = ballStartPosition;
            ball.transform.rotation = ballStartRotation;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.useGravity = true;
            ballRigidbody.mass = ballInitialMass;
        }
    }
}