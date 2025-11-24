using UnityEngine;

namespace Resources.Scripts
{
    public static class Movement
    {
        public static void Update()
        {
            if (Main.TargetPlayer)
            {
                Vector2 direction = Vector2.zero;
                if (Input.GetKey(KeyCode.W))
                    direction += Vector2.up;
                if (Input.GetKey(KeyCode.S)) 
                    direction += Vector2.down;
                if (Input.GetKey(KeyCode.A))
                    direction += Vector2.left;
                if (Input.GetKey(KeyCode.D))
                    direction += Vector2.right;
            
                Main.TargetPlayer.Direction = direction;
            } 
        }
    }
}