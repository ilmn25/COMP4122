using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Clue : Interactable
    {
        public ItemID id;

        [TextArea] public string clueText; // pass in text in the inspector

        public override void Interact(Character character)
        {
            Debug.Log("Interacted with clue: " + clueText);
            Dialogue.Run(new DialogueData {Text = clueText} , null);
            Audio.PlaySfx(AudioClipID.Item);
        }
    }
}