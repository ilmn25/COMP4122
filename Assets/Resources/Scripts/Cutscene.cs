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
        [NonSerialized] public static readonly NetworkVariable<int> Scene = new();
        private readonly NetworkVariable<int> _waiting = new ();

        [ServerRpc (RequireOwnership = false)]
        private void AddWaitingServerRpc() { _waiting.Value++; } 
        
        private void Start()
        {
            Scene.OnValueChanged += (value, newValue) =>
            {
                if (newValue == 1) StartCoroutine(Opening());
                if (newValue == 2) StartCoroutine(EndingEscaped());
            };
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
                            Text = "haily forgot to pay taxes and is going to get arrested for 10 years", 
                            Next = new Dictionary<string, DialogueData>
                            {
                                { "", new DialogueData { Text = "so she's hiding in chognxin daxia" }}
                            }
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
                yield return new WaitForSeconds(3); 
            }
        }
        
        
        private IEnumerator EndingEscaped()
        {
            Environment.SetEnvironment(EnvPreset.BlackScreen);
            yield return new WaitForSeconds(3);
            Main.CanMove = false;
            yield return new WaitForSeconds(1);
            Dialogue.Run(new DialogueData
            {
                Text = "30th December, 2027",
                Next = new Dictionary<string, DialogueData>
                {
                    { "", new DialogueData {
                            Text = "everyone escape blahwdaowhda", 
                            Next = new Dictionary<string, DialogueData>
                            {
                                { "", new DialogueData { Text = "the end." }}
                            }
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
                if (NetworkManager.Singleton.IsHost) NetworkManager.Singleton.Shutdown();
                yield return new WaitForSeconds(3); 
            }
        }
    }
}