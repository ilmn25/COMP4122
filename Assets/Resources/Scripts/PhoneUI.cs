using Resources.Scripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhoneUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputField;

    private string _number = "55566608";
    
    public void Enter(int num)
    {
        if (_inputField.text == "Invalid") _inputField.text = "";
        if (_inputField.text.Length > 12) return;
        _inputField.text += num.ToString();
    }

    public void Dial()
    {
        string[] clueText = {
            "The line finally connected...",
            "With a trembling voice, you spilled everything.",
            "After a brief, stunned silence, the officer's calm but urgent instructions cut through: “We're mobilizing now. Help is on the way. Your only job is to stay safe…”",
            "In that moment, you realized that the only thing you need to do is to hang on.",
            "To outlast the suffocating despair… and claw your way toward a sliver of hope."
        };

        if(_inputField.text != _number) _inputField.text = "Invalid";
        else {
            Dialogue.Run(BuildDialogueData(clueText));
        }
    }
    
    private DialogueData BuildDialogueData(string[] lines)
    {
        if (lines == null || lines.Length == 0) return new DialogueData { Text = "" };
    
        DialogueData root = new DialogueData { Text = lines[0] };
        DialogueData current = root;

        for (int i = 1; i < lines.Length; i++)
        {
            DialogueData next = new DialogueData { Text = lines[i] };
            current.Next = new Dictionary<string, DialogueData>
            {
                { "", next }
            };
            current = next;
        }
        return root;
    }

    public void ClosePanel()
    {
        _inputField.text = "";
        Main.CanMove = true;
    }
}
