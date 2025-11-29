using System;
using UnityEngine;

namespace Resources.Scripts
{
    public class Door : Interactable
    {
        public bool isFaceFront;
        private SpriteRenderer _spriteRenderer;
        
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront? "Door1" : "Door2");
        }

        public override void Interact(Character character)
        {
            if (character.Inventory.Contains((int)ItemID.Card))
            {
                character.Inventory.Remove((int)ItemID.Card);
                _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront? "Door2": "Door1");
                transform.Find("Collider").gameObject.SetActive(false);
            }
            Audio.PlaySfx(AudioClipID.Item);
        }
    }
}