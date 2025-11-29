using System;
using System.Collections.Generic;
using Resources.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public class DialogueData
    {
        public string Text;
        public Sprite Sprite;
        public Dictionary<string, DialogueData> Next;
    }
    
    public class Dialogue : MonoBehaviour
    {
        public static Dialogue Inst;
        public GameObject box;
        public TextMeshProUGUI text;
        public Image image;
        public GameObject imageObject;
        
        private const float EaseSpeed = 0.4f;
        private const float ShowDuration = 0.5f;
        private const float HideDuration = 0.2f;

        private static CoroutineTask _scrollTask;
        private static DialogueData _target;
        private static bool _showing = true;
        private static CoroutineTask _scaleTask;

        private void Start()
        {
            Inst = this;
            Inst.Show(false); 
        }

        public static void Run(DialogueData target)
        {
            _target = target;
            Inst.Show(true); 
        }
        
        private void Show(bool isShow)
        {
            if (isShow)
            {
                if (!_showing)
                {
                    _showing = true;
                    SetDialogue();  
                    _scaleTask?.Stop();
                    _scaleTask = new CoroutineTask(Tween.Scale(true, ShowDuration, gameObject, 
                        0.9f, EaseSpeed)); 
                    gameObject.SetActive(true);
                }
            }
            else
            {
                if (_showing)
                { 
                    _showing = false;
                    SetSprite();
                    _scaleTask?.Stop();
                    _scaleTask = new CoroutineTask(Tween.Scale(false, HideDuration, gameObject, 
                        0, EaseSpeed));
                    _scaleTask.Finished += _ =>
                    {
                        if (_scrollTask != null && _scrollTask.Running) _scrollTask.Stop();
                        gameObject.SetActive(false); 
                    }; 
                }
            }
        }
        public void Update()
        { 
            if (_showing){  
                
                if (Input.GetKeyDown(KeyCode.E))
                { 
                    Audio.PlaySfx(AudioClipID.Text); 
                    
                    if (_scrollTask.Running) _scrollTask.Stop(); 
                    else
                    {
                        if (_target.Next != null)
                        {
                            foreach (KeyValuePair<string, DialogueData> option in _target.Next)
                                if (option.Key == "") _target = option.Value;
                            SetDialogue();
                        }
                        else
                            Show(false);
                    }
                }
            }
        }
        
        private void SetDialogue()
        {
            SetSprite(_target.Sprite);
            text.text = _target.Text;
            _scrollTask = Tween.HandleScroll(text);
        }

        private void SetSprite(Sprite sprite = null)
        {
            if (sprite)
            {  
                image.sprite = _target.Sprite; 
                if (image.transform.position != new Vector3(220, -95, 203))
                    _ = new CoroutineTask(Tween.Slide(true, 0.2f, imageObject, 
                        new Vector3(220, -95, 160), EaseSpeed)); 
            }
            else
            {
                if (image.transform.position != new Vector3(500, -95, 203))
                    _ = new CoroutineTask(Tween.Slide(false, 0.1f, imageObject, 
                        new Vector3(500, -95, 160), EaseSpeed));
            }
        }
    }
}