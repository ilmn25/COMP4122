using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Resources.Scripts
{
    public class Phone1 : Interactable
    {
        [SerializeField] public GameObject phoneUI;

        // Show the phone UI when phone is interacted with
        public override void Interact(Character character)
        {
            if(phoneUI && phoneUI.activeSelf == false)
            {
                phoneUI.SetActive(true);
                Main.CanMove = false;
                Audio.PlaySfx(AudioClipID.Item);
            }
        }
    }
}