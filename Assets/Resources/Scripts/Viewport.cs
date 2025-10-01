using UnityEngine;

namespace Resources.Scripts
{
    public static class Viewport
    {
        private static readonly Vector3 Offset = new (0, 0, 0);
        private const float FollowSpeed = 5;
        private static GameObject TargetPlayer => Main.TargetPlayer.gameObject;
        private static GameObject ViewportObject => Main.ViewportObject;
        public static void Update()
        {
            if (Main.TargetPlayer)
                ViewportObject.transform.position = Vector3.Lerp(ViewportObject.transform.position, TargetPlayer.transform.position + Offset, FollowSpeed * Time.deltaTime);
        }
    }
}
