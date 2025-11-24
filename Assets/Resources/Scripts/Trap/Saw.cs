using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class Saw : Trap
    {
        private void Start()
        {
            StartCoroutine(TrapTimer());
            return;
            IEnumerator TrapTimer()
            {
                while (true)
                {
                    yield return new WaitForSeconds(0.6f);
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