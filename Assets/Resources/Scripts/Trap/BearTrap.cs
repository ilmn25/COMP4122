using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class BearTrap : Trap
    {
        private bool _triggered;
        private void Update()
        {
            Scan(); 
        }

        protected override void OnTouch(Character character)
        {
            if (_triggered) return;
            _triggered = true;
            Animator animator = GetComponent<Animator>();
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, -1, 0f);
            character.ChangeHealthServerRpc();
            character.Status.Add((int)StatusID.Stuck);
        }
    }
}