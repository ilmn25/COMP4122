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
        private GameObject wiring; 
         
        private void Update()
        {
            bool correct = true;
            foreach (Clock clock in clocks)
                if (!clock.IsCorrect.Value) correct = false; 
            OpenServerRpc(correct);
        } 
    }

    public abstract class FurnitureMovable : NetworkBehaviour
    {
        private readonly NetworkVariable<bool> _open = new ();
        public Vector2 offset; 
        private Vector2 _startPosition;
        private CoroutineTask _coroutine; 
        
        protected virtual void Start()
        {
            _startPosition = transform.position;
            
            _open.OnValueChanged += OnOpenStateChanged;
        } 
        
        private void OnOpenStateChanged(bool oldValue, bool newValue)
        {
            _coroutine?.Stop();
            _coroutine = new CoroutineTask(MoveBookcase(newValue ? _startPosition + offset : _startPosition));  
        }
         
        private IEnumerator MoveBookcase(Vector2 position)
        {
            while (true)
            {
                yield return null;
                transform.position = Vector3.MoveTowards(transform.position, position,
                    3 * Time.deltaTime);
                if (Vector3.Distance(transform.position, position) < 0.01f) break;
            } 
        }
        
        [ServerRpc(RequireOwnership = false)]
        protected void OpenServerRpc(bool open)
        {
            _open.Value = open;
        }
        
        private void OnDestroy()
        {
            _open.OnValueChanged -= OnOpenStateChanged;
        }
    }
}
