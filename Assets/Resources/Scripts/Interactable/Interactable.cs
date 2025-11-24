using Unity.Netcode;

namespace Resources.Scripts
{
    public abstract class Interactable : NetworkBehaviour
    {
        public abstract void Interact(Character character);
    }
}