using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Resources.Scripts;

public class PasswordLockUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TextMeshProUGUI passwordDisplay;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button[] numberButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button closeButton;
    
    [Header("Settings")]
    [SerializeField] private float statusDisplayTime = 1.5f;
    
    private string currentPassword = "";
    private string correctPassword = "";
    private Action<bool> onPasswordAttempt;
    private Door currentDoor;
    
    private void Start()
    {
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int number = i;
            numberButtons[i].onClick.AddListener(() => AddNumber(number));
        }

        confirmButton.onClick.AddListener(ConfirmPassword);
        clearButton.onClick.AddListener(ClearPassword);
        closeButton.onClick.AddListener(ClosePanel);
        
        ResetUI();
        passwordPanel.SetActive(false);
    }
    
    public void Initialize(string correctPasscode, Door door, Action<bool> onAttemptCallback)
    {
        correctPassword = correctPasscode;
        currentDoor = door;
        onPasswordAttempt = onAttemptCallback;
        ResetUI();
    }
    
    private void AddNumber(int number)
    {
        if (currentPassword.Length < 4)
        {
            currentPassword += number.ToString();
            UpdatePasswordDisplay();
        }
    }
    
    private void ConfirmPassword()
    {
        bool isCorrect = currentPassword == correctPassword;
        onPasswordAttempt?.Invoke(isCorrect);
        
        if (isCorrect)
        {
            ShowStatus("Correct!", Color.green);
            Invoke(nameof(ClosePanel), statusDisplayTime);
        }
        else
        {
            ShowStatus("Wrong!", Color.red);
            currentPassword = "";
            UpdatePasswordDisplay();
            Invoke(nameof(ResetStatus), statusDisplayTime);
        }
    }
    
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
            statusText.gameObject.SetActive(true);
        }
    }
    
    private void ResetStatus()
    {
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }
    
    private void ClearPassword()
    {
        currentPassword = "";
        UpdatePasswordDisplay();
        ResetStatus();
    }
    
    public void ClosePanel()
    {
        currentPassword = "";
        currentDoor = null;
        ResetUI();
        passwordPanel.SetActive(false);
    }
    
    private void ResetUI()
    {
        currentPassword = "";
        UpdatePasswordDisplay();
        ResetStatus();
    }
    
    private void UpdatePasswordDisplay()
    {
        if (passwordDisplay != null)
        {
            string display = "";
            for (int i = 0; i < currentPassword.Length; i++)
            {
                display += "● ";
            }
            for (int i = currentPassword.Length; i < 4; i++)
            {
                display += "_ ";
            }
            passwordDisplay.text = display.Trim();
        }
    }
    
    public void ShowPanel()
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);
            ResetUI();
        }
    }
    
    public bool IsPanelActive()
    {
        return passwordPanel != null && passwordPanel.activeInHierarchy;
    }
    
    public Door GetCurrentDoor()
    {
        return currentDoor;
    }
}