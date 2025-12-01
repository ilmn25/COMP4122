using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Resources.Scripts
{
    public class CharacterRevive : Interactable
    {
        public Character character;
        private CircleCollider2D _collider2D;
        private TextMeshPro _text;
        private Coroutine _coroutine;

        private void Start()
        {
            _text = GetComponent<TextMeshPro>();
            _text.text = "";
            _collider2D = GetComponent<CircleCollider2D>();
            _collider2D.enabled = false;
            character.CurrentHealth.OnValueChanged += (value, newValue) =>
            {
                _collider2D.enabled = character.CurrentHealth.Value <= 0;
            };
        }

        public override void Interact(Character reviver)
        {
            if (reviver == character || _coroutine != null) return;
            _coroutine = StartCoroutine(Progress()); 
            return;
            
            IEnumerator Progress()
            {
                Audio.PlaySfx(AudioClipID.Item);
                int progress = 0; 
                while (true)
                {
                    yield return new WaitForSeconds(0.05f);
                    if (Vector3.Distance(character.transform.position, transform.position) < 1.5f && reviver.CurrentHealth.Value > 0)
                    {
                        progress++;
                        _text.text = progress + "%"; 
                        if (progress == 100)
                        {
                            _coroutine = null;
                            character.ChangeHealthServerRpc(1);
                            _text.text = "";
                            break;
                        }
                    }
                    else
                    {
                        _coroutine = null;
                        _text.text = "";
                        break;
                    }
                } 
            }
        }
    }
}