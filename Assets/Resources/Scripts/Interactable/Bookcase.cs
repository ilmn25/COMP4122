using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Bookcase : FurnitureMovable
    {
        public List<Clock> clocks = new ();
        private void Update()
        {
            bool correct = true;
            foreach (Clock clock in clocks)
                if (!clock.IsCorrect.Value) correct = false; 
            OpenServerRpc(correct);
        }
    }

    public abstract class FurnitureMovable : MonoBehaviour
    {
        private readonly NetworkVariable<bool> _open =  new ();
        public Vector2 offset; 
        private Vector2 _startPosition;
        private CoroutineTask _coroutine;
        private void Start()
        {
            _startPosition = transform.position;
            _open.OnValueChanged += (value, newValue) =>
            {
                _coroutine?.Stop();
                _coroutine = new CoroutineTask(MoveBookcase(_open.Value? _startPosition + offset : _startPosition)); 
            };

            IEnumerator MoveBookcase(Vector2 position)
            {
                while (true)
                {
                    yield return null;
                    transform.position = Vector3.MoveTowards(transform.position, position,
                        3 * Time.deltaTime);
                    if (Vector3.Distance(transform.position, position) < 0.01f) break;
                } 
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        protected void OpenServerRpc(bool open)
        {
            _open.Value = open;
        }
    }
}