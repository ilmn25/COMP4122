using System;
using UnityEngine;

namespace Resources.Scripts
{
    public class VisualClue : Clue
    {
        private void Start()
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        public override void Interact(Character character)
        {
            base.Interact(character);
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}