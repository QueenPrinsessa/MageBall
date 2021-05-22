using Mirror;
using System;
using System.Collections;
using UnityEngine;

namespace MageBall
{
    public class MatchTimer : NetworkBehaviour
    {

        [SerializeField] private AudioSource gameOverAudioSource;

        private Coroutine timerRoutine;
        private bool hasMatchEnded = false;

        [SyncVar] private int minutes;
        [SyncVar] private int seconds;

        public int Minutes => minutes;
        public int Seconds => seconds;

        /// <summary>
        /// Minutes, seconds
        /// </summary>
        public event Action<int, int> TimeChanged;
        public event Action MatchEnd;

        public override void OnStartServer()
        {
            NetworkManagerMageBall networkManager = (NetworkManagerMageBall)NetworkManager.singleton;
            minutes = networkManager.MatchLength;
            TimeChanged?.Invoke(minutes, seconds);
            timerRoutine = StartCoroutine(Timer(networkManager));
        }

        [ClientRpc]
        private void RpcInvokeTimeChanged(int minutes, int seconds)
        {
            TimeChanged?.Invoke(minutes, seconds);
        }

        private IEnumerator Timer(NetworkManagerMageBall networkManager)
        {
            while (!hasMatchEnded)
            {
                if (seconds <= 0)
                {
                    minutes--;
                    seconds = 59;
                }
                else
                    seconds--;

                RpcInvokeTimeChanged(minutes, seconds);

                if (minutes <= 0 && seconds <= 0)
                    EndGame();

                yield return new WaitForSeconds(1);
            }
        }

        private void EndGame()
        {
            hasMatchEnded = true;
            StopCoroutine(timerRoutine);
            RpcInvokeMatchEnd();
        }

        [ClientRpc]
        private void RpcInvokeMatchEnd()
        {
            gameOverAudioSource.Play();
            MatchEnd?.Invoke();
        }
    }
}