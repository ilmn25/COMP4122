using Resources.Scripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputField;

    private string _number = "12345678";
    
    public void Enter(int num)
    {
        if (_inputField.text == "Invalid") _inputField.text = "";
        if (_inputField.text.Length > 12) return;
        _inputField.text += num.ToString();
    }

    public void Dial()
    {
        if(_inputField.text != _number) _inputField.text = "Invalid";
        else {
            // TODO: if corrent number need to trigger ending scene and end the game


        }
    }

    public void ClosePanel()
    {
        _inputField.text = "";
        Main.CanMove = true;
    }
}
