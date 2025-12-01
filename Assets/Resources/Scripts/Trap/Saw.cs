using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public class Saw : Trap
    {
        public float speed = 3;   // movement speed
        private Vector3 _a;
        private Vector3 _b;
        private Vector3 _target;

        private void Start()
        {
            _a = transform.Find("A").position;
            _b = transform.Find("B").position;
            _target = _b; 

            StartCoroutine(TrapTimer());
            return;
            IEnumerator TrapTimer()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1f);
                    Scan();
                }
            }
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _target) < 0.01f)
                _target = _target == _a ? _b : _a;
        }   

        protected override void OnTouch(Character character)
        {
            character.ChangeHealthServerRpc();
        }
    }
}