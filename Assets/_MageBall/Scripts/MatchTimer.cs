using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MatchTimer : NetworkBehaviour
    {
        //This should probably be obtained from somewhere else later.
        private Coroutine timerRoutine;
        private bool hasMatchEnded = false;

        [SyncVar] private int minutes;
        [SyncVar(hook = nameof(OnTimeChanged))] private int seconds;

        /// <summary>
        /// Minutes, seconds
        /// </summary>
        public event Action<int, int> timeChanged;
        public event Action matchEnd;

        public override void OnStartServer()
        {
            NetworkManagerMageBall networkManager = (NetworkManagerMageBall)NetworkManager.singleton;
            minutes = networkManager.MatchLength;
            timeChanged?.Invoke(minutes, seconds);
            timerRoutine = StartCoroutine(Timer());
        }

        private void OnTimeChanged(int oldValue, int newValue)
        {
            timeChanged?.Invoke(minutes, seconds);
        }

        private IEnumerator Timer()
        {
            while (!hasMatchEnded)
            {
                if (seconds <= 0)
                {
                    minutes--;
                    seconds = 10;
                }
                else
                    seconds--;

                if (minutes <= 0 && seconds <= 0)
                    EndGame();

                yield return new WaitForSeconds(1);
            }
        }

        private void EndGame()
        {
            hasMatchEnded = true;
            StopCoroutine(timerRoutine);
            matchEnd?.Invoke();
        }
    }
}