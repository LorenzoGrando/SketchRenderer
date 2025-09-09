using UnityEngine;

namespace SketchRenderer.Runtime.Extensions
{
    public static class MathUtilities
    {
        public static float GetAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        public static float GetNormalizedAngle(Vector2 vector)
        {
            float angle = GetAngle(vector);
            if(angle < 0)
                angle += 360;
            return angle / 360f;
        }

        public static Vector2 GetUnitDirection(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}