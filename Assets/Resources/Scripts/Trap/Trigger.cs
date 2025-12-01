using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class Trigger : Trap
    {
        [TextArea] public string[] text; // pass in text in the inspector

        private bool _triggered;
        private void FixedUpdate()
        {
            if (!_triggered) Scan(); 
        }

        protected override void OnTouch(Character character)
        {
            _triggered = true;
            Dialogue.Run(Clue.BuildDialogueData(text));
        }
    }
}