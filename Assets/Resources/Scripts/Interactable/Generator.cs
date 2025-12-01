using System.Collections;
using TMPro;
using UnityEngine;

namespace Resources.Scripts
{
    public class Generator : Interactable
    {
        private TextMeshPro _text;
        private Coroutine _coroutine;
        
        private void Start()
        {
            _text = transform.Find("Text").GetComponent<TextMeshPro>();
        }

        public override void Interact(Character character)
        {
            if (_coroutine != null) return;
            
            _coroutine = StartCoroutine(Progress()); 
            
            IEnumerator Progress()
            {
                Audio.PlaySfx(AudioClipID.Item);
                int progress = 0; 
                
                while (true)
                {
                    yield return new WaitForSeconds(0.1f);
                    
                    // 检查玩家是否还在交互范围内
                    if (Vector3.Distance(character.transform.position, transform.position) < 2)
                    {
                        progress++;
                        _text.text = progress + "%"; 
                        
                        if (progress == 100)
                        {
                            _coroutine = null;
                            _text.text = "ACTIVATED";
                            Debug.Log("Generator activated!");
                            break;
                        }
                    }
                    else
                    {
                        // 玩家离开范围，重置
                        _coroutine = null;
                        _text.text = "";
                        break;
                    }
                } 
            }
        }
    }
}