using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class BearTrap : Trap
    {
        private bool _triggered;
        private void FixedUpdate()
        {
            Scan(); 
        }

        protected override void OnTouch(Character character)
        {
            if (_triggered) return;
            _triggered = true;
            GetComponent<SpriteRenderer>().sprite = Cache.LoadSprite("BearTrap2");
            character.ChangeHealthServerRpc();
            character.Status.Add((int)StatusID.Stuck);
        }
    }
}