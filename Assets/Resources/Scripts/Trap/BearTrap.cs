using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class BearTrap : Trap
    {
        private bool _triggered;
        private void FixedUpdate()
        {
            if (!_triggered) Scan(); 
        }

        protected override void OnTouch(Character character)
        {
            _triggered = true;
            GetComponent<SpriteRenderer>().sprite = Cache.LoadSprite("BearTrap2");
            character.ChangeHealthServerRpc();
        }
    }
}