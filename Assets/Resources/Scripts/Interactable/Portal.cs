using Unity.VisualScripting;

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


    public class Door : Interactable
    {
        public override void Interact(Character character)
        {
            if (character.Inventory.Contains((int)ItemID.Card))
            {
                character.Inventory.Remove((int)ItemID.Card);
            }
            Audio.PlaySfx(AudioClipID.Item);
        }
    }
}