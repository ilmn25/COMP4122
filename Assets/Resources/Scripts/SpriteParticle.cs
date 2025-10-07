using UnityEngine;

namespace Resources.Scripts
{
    public class SpriteParticle : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        { 
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            { 
                _animator.Play(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
                ObjectPool.ReturnObject(gameObject);
            }
        }
    }
}