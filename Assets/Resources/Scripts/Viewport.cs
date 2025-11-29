using UnityEngine;

namespace Resources.Scripts
{
    public static class Viewport
    {
        private const float FollowSpeed = 5;
        private static GameObject TargetPlayer => Main.TargetPlayer.gameObject;
        private static GameObject ViewportObject => Main.ViewportObject;
        public static void Update()
        {
            Vector3 targetPosition = Main.TargetPlayer ? TargetPlayer.transform.position : Vector3.back * 1000;
            ViewportObject.transform.position = Vector3.Lerp(ViewportObject.transform.position, targetPosition, FollowSpeed * Time.deltaTime);
        }
    }
}
