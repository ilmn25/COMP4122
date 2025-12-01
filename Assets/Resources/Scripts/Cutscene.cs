using System;
using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Cutscene : NetworkBehaviour
    {
        [NonSerialized] public static readonly NetworkVariable<int> Scene = new(0);
        private readonly NetworkVariable<int> _waiting = new ();

        [ServerRpc (RequireOwnership = false)]
        private void AddWaitingServerRpc() { _waiting.Value++; } 
        
        private void Start()
        {
            Scene.OnValueChanged += (value, newValue) =>
            {
                if (newValue == 1) StartCoroutine(Opening()); 
                else if (newValue == 2) {
                    Debug.Log("a");
                    StartCoroutine(Ending(new DialogueData
                    {
                        Text = "I am dead",
                        Next = new Dictionary<string, DialogueData>
                        {
                            {
                                "", new DialogueData
                                {
                                    Text = "We are all dead",
                                    Next = new Dictionary<string, DialogueData>
                                    {
                                        { "", new DialogueData { Text = "The end." } }
                                    }
                                }
                            }
                        }
                    }));
                    
                }
                else if (newValue == 3) StartCoroutine(Ending(new DialogueData
                {
                    Text = "30th December, 2027",
                    Next = new Dictionary<string, DialogueData>
                    {
                        {
                            "", new DialogueData
                            {
                                Text = "Everyone escaped.",
                                Next = new Dictionary<string, DialogueData>
                                {
                                    { "", new DialogueData { Text = "The end." } }
                                }
                            }
                        }
                    }
                })); 
            };
        }

        private void Update()
        {
            if (!IsServer || !Main.Begun || Scene.Value != 0) return;
            int alive = 0;
            foreach (Character character in Main.Players)
            {
                if (character.CurrentHealth.Value > 0) alive++;
            }
            if (alive == 0) Scene.Value = 2;
        }

        private IEnumerator Opening()
        {
            Environment.SetEnvironment(EnvPreset.BlackScreen);
            yield return new WaitForSeconds(3);
            Main.CanMove = false;
            yield return new WaitForSeconds(1);
            Dialogue.Run(new DialogueData
            {
                Text = "29th December, 2027",
                Next = new Dictionary<string, DialogueData>
                {
                    { "", new DialogueData {
                            Text = "Packing your bags, you and your friends set out on a mission to explore\nthe renowned Chungking Mansion", 
                            Sprite = Cache.LoadSprite("PlayerDialogue"),
                            Next = new Dictionary<string, DialogueData>
                            {
                                { "", new DialogueData { 
                                    Text = "However... it seems you stumbled upon something that you shouldn't have.", 
                                    Sprite = Cache.LoadSprite("PlayerDialogue"),
                                    Next = new Dictionary<string, DialogueData>
                                    {
                                        { "", new DialogueData { 
                                            Text = "Try to escape the building.",
                                            Sprite = Cache.LoadSprite("PlayerDialogue")    
                                        }}
                                    }
                                }
                            }}
                        } 
                    }
                }
            }, () => _ = new CoroutineTask(OnDialogueEnd()));
            
            IEnumerator OnDialogueEnd()
            {
                AddWaitingServerRpc();
                while (Scene.Value != 0)
                {
                    // if (NetworkManager.Singleton.IsHost) Debug.Log(_waiting.Value + " | " + NetworkManager.Singleton.ConnectedClients.Count);
                    if (NetworkManager.Singleton.IsHost && _waiting.Value >= NetworkManager.Singleton.ConnectedClients.Count) Scene.Value = 0; 
                    yield return null;
                }
                Environment.SetEnvironment(EnvPreset.Night);
                Main.TargetPlayer.transform.position = new Vector3(-51.5f, 2f, 0f);;
                Main.CanMove = true;
                HUD.UpdateHealth(1, 1);
                HUD.Inst.gameObject.SetActive(true);
                Main.Begun = true;
                yield return new WaitForSeconds(3); 
            }
        } 
        
        private IEnumerator Ending(DialogueData dialogueData)
        {
            yield return new WaitForSeconds(2);
            Environment.SetEnvironment(EnvPreset.BlackScreen);
            yield return new WaitForSeconds(3);
            Main.CanMove = false;
            yield return new WaitForSeconds(1);
            Dialogue.Run(dialogueData, () => _ = new CoroutineTask(OnDialogueEnd()));
            IEnumerator OnDialogueEnd()
            {
                AddWaitingServerRpc();
                while (Scene.Value != -1)
                {
                    // if (NetworkManager.Singleton.IsHost) Debug.Log(_waiting.Value + " | " + NetworkManager.Singleton.ConnectedClients.Count);
                    if (NetworkManager.Singleton.IsHost && _waiting.Value >= NetworkManager.Singleton.ConnectedClients.Count) Scene.Value  = -1; 
                    yield return null;
                } 
                Application.Quit();
            }
        }
    }
}