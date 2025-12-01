using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class DarkRoom : MonoBehaviour
    { 
        private Vector2 _colliderOffset;
        private Vector2 _colliderSize;
        private bool _isInside;

        private void Start()
        {
            BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
            _colliderOffset = boxCollider2D.offset;
            _colliderSize = boxCollider2D.size;
            StartCoroutine(Update());

            IEnumerator Update()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    bool skipWhile = false;
                    int hitCount = Physics2D.OverlapBoxNonAlloc(
                        transform.position + new Vector3(_colliderOffset.x, _colliderOffset.y, 0),
                        _colliderSize,
                        0,
                        Main.ColliderArray,
                        LayerMask.GetMask("Player")
                    );

                    for (int i = 0; i < hitCount; i++)
                    {
                        if (Main.ColliderArray[i].GetComponent<Character>() == Main.TargetPlayer)
                        {
                            if (!_isInside)
                            {
                                _isInside = true;
                                Environment.SetEnvironment(EnvPreset.BlackScreen);
                            }

                            skipWhile = true;
                            break;
                        }
                    }

                    if (skipWhile) continue;

                    if (_isInside)
                    {
                        _isInside = false;
                        Environment.SetEnvironment(EnvPreset.Night);
                    }
                }
            }
        }
    }
}