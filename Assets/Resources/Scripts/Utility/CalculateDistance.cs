using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.Utility
{
    public class CalculateDistance 
    {
        public static float SqrDistance(Vector3 self, Vector3 target)
        {
            Vector3 difference = self - target;
            difference.x = self.x - target.x;
            difference.y = self.y - target.y;
            difference.z = self.z - target.z;

            return difference.sqrMagnitude;
        }
    }
}
