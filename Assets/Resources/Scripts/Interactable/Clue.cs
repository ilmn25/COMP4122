using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace Resources.Scripts
{
    public class Clue : Interactable
    {  
        [TextArea] public string[] clueText; // pass in text in the inspector

        public override void Interact(Character character)
        {
            Dialogue.Run(BuildDialogueData(clueText));
            Audio.PlaySfx(AudioClipID.Item);
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
    }
}