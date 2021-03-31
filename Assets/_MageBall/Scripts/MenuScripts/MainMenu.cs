using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MageBall
{
    public class MainMenu : MonoBehaviour
    {

        [SerializeField] private TMP_InputField ipInputField;


        public void CreateGame()
        {
            Debug.Log("Create game pressed");
        }

        public void JoinGame()
        {
            Debug.Log("Join game pressed");
        }

        public void OpenOptionsMenu()
        {
            Debug.Log("Options pressed");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

#if UNITY_STANDALONE
            Application.Quit();
#endif
        }



    }
}