using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class Spikes : Trap
    {
        private Animator _animator;
        private void Start()
        {
            _animator = GetComponent<Animator>();
            StartCoroutine(TrapTimer());
            return;
            IEnumerator TrapTimer()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1f);
                    yield return new WaitForSeconds(Random.Range(0f, 2f));
                    _animator.Play(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash, -1, 0f);
                    Scan(); 
                } 
            }
        }

        protected override void OnTouch(Character character)
        {
            character.TakeDamageServerRpc();
        }
    }
}