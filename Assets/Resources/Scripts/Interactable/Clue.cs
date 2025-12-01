using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Clue : Interactable
    {  
        [TextArea] public string clueText; // pass in text in the inspector

        public override void Interact(Character character)
        {
            Debug.Log("Interacted with clue: " + clueText);
            Dialogue.Run(new DialogueData {Text = clueText});
            Audio.PlaySfx(AudioClipID.Item);
        }
    }
}