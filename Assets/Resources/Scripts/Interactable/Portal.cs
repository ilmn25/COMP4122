using UnityEngine;

namespace Resources.Scripts
{
    public class Portal : Interactable
    {
        public override void Interact(Character character)
        {
            character.transform.position = transform.Find("Destination").position;
            Audio.PlaySfx(AudioClipID.Item);
        }
    }
}