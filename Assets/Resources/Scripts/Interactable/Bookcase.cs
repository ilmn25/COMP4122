using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Bookcase : FurnitureMovable
    {
        public List<Clock> clocks = new ();
        public GameObject wiringPrefab; // 改为 prefab 引用
        public Vector3 wiringPosition = Vector3.zero; // Wiring 的生成位置
        
        private GameObject _wiringInstance; // 存储实例化的对象
        
        protected override void Start()
        {
            base.Start();
        }
        
        private void Update()
        {
            bool correct = true;
            foreach (Clock clock in clocks)
                if (!clock.IsCorrect.Value) correct = false; 
            OpenServerRpc(correct);
        }
        
        // 重写父类的方法来处理 wiring 实例化
        protected override void HandleWiringActivation(bool isOpen)
        {
            if (wiringPrefab == null)
            {
                Debug.LogError("Wiring prefab is not assigned to Bookcase!");
                return;
            }
            
            if (isOpen)
            {
                // 如果还没有实例化，则实例化 wiring
                if (_wiringInstance == null)
                {
                    // 使用指定的位置生成 Wiring
                    _wiringInstance = Instantiate(wiringPrefab, wiringPosition, Quaternion.identity);
                    
                    // 如果需要，可以设置父物体（如果需要的话）
                    // _wiringInstance.transform.SetParent(null); // 确保没有父物体
                    
                    // 如果是网络游戏，可能需要 NetworkObject
                    var networkObject = _wiringInstance.GetComponent<NetworkObject>();
                    if (networkObject != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                    {
                        networkObject.Spawn();
                    }
                    
                    Debug.Log($"Wiring instantiated at position: {_wiringInstance.transform.position}, target position: {wiringPosition}");
                }
                else
                {
                    // 如果已经存在，确保它在正确位置
                    _wiringInstance.transform.position = wiringPosition;
                }
                
                _wiringInstance.SetActive(true);
            }
            else
            {
                // 关闭时隐藏 wiring
                if (_wiringInstance != null)
                {
                    _wiringInstance.SetActive(false);
                }
            }
        }
        
        
        private void OnDestroy()
        {
            if (_wiringInstance != null)
            {
                Destroy(_wiringInstance);
            }
        }
    }

    public abstract class FurnitureMovable : NetworkBehaviour
    {
        private readonly NetworkVariable<bool> _open = new ();
        public Vector2 offset; 
        private Vector2 _startPosition;
        private CoroutineTask _coroutine;
        
        private bool _isInitialized = false;
        
        protected virtual void Start()
        {
            _startPosition = transform.position;
            
            _open.OnValueChanged += OnOpenStateChanged;
        }
        
        private void OnEnable()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                StartCoroutine(InitializeNextFrame());
            }
        }
        
        private IEnumerator InitializeNextFrame()
        {
            yield return null;
            // 初始化时根据当前状态设置 wiring
            HandleWiringActivation(_open.Value);
        }
        
        private void OnOpenStateChanged(bool oldValue, bool newValue)
        {
            _coroutine?.Stop();
            _coroutine = new CoroutineTask(MoveBookcase(newValue ? _startPosition + offset : _startPosition)); 
            
            // 当书柜状态改变时，控制 wiring 的显示
            HandleWiringActivation(newValue);
        }
        
        protected virtual void HandleWiringActivation(bool isOpen)
        {
            // 基类不做任何事，由子类实现
        }

        private IEnumerator MoveBookcase(Vector2 position)
        {
            while (true)
            {
                yield return null;
                transform.position = Vector3.MoveTowards(transform.position, position,
                    3 * Time.deltaTime);
                if (Vector3.Distance(transform.position, position) < 0.01f) break;
            } 
        }
        
        [ServerRpc(RequireOwnership = false)]
        protected void OpenServerRpc(bool open)
        {
            _open.Value = open;
        }
        
        private void OnDestroy()
        {
            _open.OnValueChanged -= OnOpenStateChanged;
        }
    }
}
