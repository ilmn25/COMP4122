using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class PuzzleManager : NetworkBehaviour
    {
        [Header("Clock Puzzles")]
        public Clock clock1;
        public float clock1TargetAngle = 90f;
        
        public Clock clock2;
        public float clock2TargetAngle = 270f;
        
        [Header("Bookshelf Settings")]
        public GameObject bookshelf;
        public Vector3 bookshelfTargetOffset = new Vector3(4f, 0f, 0f);
        public float moveDuration = 2f;
        
        private NetworkVariable<bool> isPuzzleSolved = new NetworkVariable<bool>(false);
        private Vector3 bookshelfStartPosition;
        private bool isMoving = false;
        private float moveTimer = 0f;

        private void Start()
        {
            if (bookshelf != null)
            {
                bookshelfStartPosition = bookshelf.transform.position;
            }
            
            // 监听时钟旋转事件（如果Clock类有事件的话）
            // 如果没有事件，我们会在Update中检查
        }

        private void Update()
        {
            if (!IsServer) return;
            
            // 检查谜题是否解决
            if (!isPuzzleSolved.Value && CheckPuzzleSolved())
            {
                isPuzzleSolved.Value = true;
                MoveBookshelfServerRpc();
            }
            
            // 处理书柜移动动画
            if (isMoving)
            {
                moveTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(moveTimer / moveDuration);
                
                Vector3 targetPosition = bookshelfStartPosition + bookshelfTargetOffset;
                bookshelf.transform.position = Vector3.Lerp(bookshelfStartPosition, targetPosition, progress);
                
                if (progress >= 1f)
                {
                    isMoving = false;
                    moveTimer = 0f;
                    Debug.Log("Bookshelf moved to target position");
                }
            }
        }

        private bool CheckPuzzleSolved()
        {
            if (clock1 == null || clock2 == null) return false;
            
            bool clock1Correct = clock1.IsAtAngle(clock1TargetAngle);
            bool clock2Correct = clock2.IsAtAngle(clock2TargetAngle);
            
            if (clock1Correct && clock2Correct)
            {
                Debug.Log($"Puzzle solved! Clock1: {clock1.GetCurrentRotation()}, Clock2: {clock2.GetCurrentRotation()}");
                return true;
            }
            
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveBookshelfServerRpc()
        {
            MoveBookshelfClientRpc();
        }

        [ClientRpc]
        private void MoveBookshelfClientRpc()
        {
            if (bookshelf != null)
            {
                isMoving = true;
                moveTimer = 0f;
                Debug.Log("Starting bookshelf movement");
            }
            else
            {
                Debug.LogError("Bookshelf reference is null!");
            }
        }

        // 重置谜题（可选）
        [ServerRpc(RequireOwnership = false)]
        public void ResetPuzzleServerRpc()
        {
            isPuzzleSolved.Value = false;
            ResetPuzzleClientRpc();
        }

        [ClientRpc]
        private void ResetPuzzleClientRpc()
        {
            if (bookshelf != null)
            {
                bookshelf.transform.position = bookshelfStartPosition;
                isMoving = false;
                moveTimer = 0f;
            }
        }

        // 在编辑器中可视化目标位置
        private void OnDrawGizmosSelected()
        {
            if (bookshelf != null)
            {
                Gizmos.color = Color.green;
                Vector3 startPos = Application.isPlaying ? bookshelfStartPosition : bookshelf.transform.position;
                Vector3 endPos = startPos + bookshelfTargetOffset;
                
                Gizmos.DrawLine(startPos, endPos);
                Gizmos.DrawWireCube(endPos, new Vector3(1f, 2f, 1f));
            }
        }
    }
}