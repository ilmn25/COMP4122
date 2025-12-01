using System;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;

namespace Resources.Scripts
{
    public class Lamp : Interactable
    {  
        private Light2D _light;
        private readonly NetworkVariable<bool> _on = new ();

        private void Start()
        { 
            _light = GetComponent<Light2D>();
            SetOnServerRpc(_light.enabled);
            _on.OnValueChanged += (value, newValue) =>
            { 
                _light.enabled = newValue;
            };
        }

        public override void Interact(Character character)
        { 
            SetOnServerRpc(!_on.Value);
            Audio.PlaySfx(AudioClipID.Item);
        }
        
        [ServerRpc (RequireOwnership = false)]
        private void SetOnServerRpc(bool on)
        {
            _on.Value = on;
        }
    }
}