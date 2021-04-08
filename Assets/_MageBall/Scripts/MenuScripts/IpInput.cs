using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IpInput : MonoBehaviour
{

    public const string PlayerPrefsIpAddressKey = "IpAddress";

    private TMP_InputField inputField;
    public string IpAddress { get; private set; }

    private void Awake()
    {
        inputField = gameObject.GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogError("There is no TMP_InputField attached to this component!");
            return;
        }

        SetupInputField();
    }

    private void SetupInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsIpAddressKey))
            return;

        string savedIpAddress = PlayerPrefs.GetString(PlayerPrefsIpAddressKey);
        inputField.text = savedIpAddress;


        if (IsValidIpAddress(savedIpAddress))
            IpAddress = savedIpAddress;
    }

    private bool IsValidIpAddress(string ipAddress) => !string.IsNullOrEmpty(ipAddress);

    public void SaveIpAddressToPlayerPrefs()
    {
        if (inputField == null)
            return;

        if (!IsValidIpAddress(inputField.text))
            return;

        IpAddress = inputField.text;
        PlayerPrefs.SetString(PlayerPrefsIpAddressKey, IpAddress);
    }

}
