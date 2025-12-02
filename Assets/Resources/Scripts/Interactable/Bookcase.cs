using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Bookcase : NetworkBehaviour
    {
        public List<Clock> clocks = new();
        public Vector2 offset;
        private Vector2 _startPosition;
        private CoroutineTask _coroutine;
        private readonly NetworkVariable<bool> _open = new();
        private bool _moved;

        private void Start()
        {
            _startPosition = transform.position;
            _open.OnValueChanged += OnOpenStateChanged;
        }

        private void Update()
        {
            if (_moved) return;

            bool correct = true;
            foreach (Clock clock in clocks)
                if (!clock.IsCorrect.Value) correct = false;

            if (correct)
            {
                _moved = true;
                Dialogue.Run(new DialogueData() { Text = "I heard furniture shifting in the north." });
                OpenServerRpc(true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void OpenServerRpc(bool open)
        {
            _open.Value = open;
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
                transform.position = Vector3.MoveTowards(transform.position, position, 3 * Time.deltaTime);
                if (Vector3.Distance(transform.position, position) < 0.01f) break;
            }
        }

        private void OnDestroy()
        {
            _open.OnValueChanged -= OnOpenStateChanged;
        }
    }
}