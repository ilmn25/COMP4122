using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Resources.Scripts
{
    public class Objectives : MonoBehaviour
    {
        public TextMeshProUGUI textMeshProUGUI;
        public Door firstDoor; 
        public List<Generator> generators;

        private void Update()
        {
            if (!Main.Begun) return;
            int activated = 0; 
            foreach(var generator in generators) if (generator.IsActivated.Value) activated++;
            
            if (!firstDoor.IsUnlocked) textMeshProUGUI.text =  "- Find a way to escape the room.";
            else if (activated != generators.Count) textMeshProUGUI.text =  $"- Activate all generators ({activated}/{generators.Count}).";
            else textMeshProUGUI.text = "- Find clue to locate the secret passage.";
        }
    }
}