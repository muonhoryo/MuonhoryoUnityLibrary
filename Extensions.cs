using UnityEngine;
using System;

namespace MuonhoryoLibrary.Unity
{
    public static class Extensions
    {
        /// <summary>
        /// Convert object's layer to LayerMask.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static int GetLayerMask(this int layer)
        {
            return (int)Mathf.Pow(2, layer);
        }
        /// <summary>
        /// Angle counterclockwise from right
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="degressAngle"></param>
        /// <returns></returns>
        public static Vector2 AngleOffset(this Vector2 offset, float degressAngle)
        {
            float radAngle = degressAngle * Mathf.Deg2Rad;
            return new Vector2
                (offset.x * Mathf.Cos(radAngle) -
                    offset.y * Mathf.Sin(radAngle),
                offset.y * Mathf.Cos(radAngle) +
                    offset.x * Mathf.Sin(radAngle));
        }
        /// <summary>
        /// Angle counterclockwise from right
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="degressAngle"></param>
        /// <returns></returns>
        public static Vector2 DirectionOfAngle(this float degressAngle)
        {
            float radAngle = degressAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
        }
        public static bool IsInLayerMask(this int layer, int layerMask)
        {
            return (layerMask & (int)Math.Pow(2, layer)) != 0;
        }
        public static Vector2 GetBorderlinePoint(this Rect limit, Vector2 point)
        {
            float x = point.x;
            float y = point.y;
            if (x < limit.xMin) x = limit.xMin;
            else if (x > limit.xMax) x = limit.xMax;
            if (y < limit.yMin) y = limit.yMin;
            else if (y > limit.yMax) y = limit.yMax;
            return new Vector2(x, y);
        }
        public static Vector3 GetEulerAngleOfImage(this Vector3 oldEulerRot, float newImageAngle) =>
            new Vector3(oldEulerRot.x, oldEulerRot.y, newImageAngle);
        /// <summary>
        /// Angle counterclockwise from right
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float AngleFromDirection(this Vector2 direction)
        {
            direction = Vector3.Normalize(direction);

            if (direction.x == 0)
            {
                if (direction.y > 0)
                    return 90;
                else
                    return 270;
            }
            float angle = Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg;
            if (direction.y < 0)
            {
                angle += 180;
            }
            return (360 + angle) % 360;
        }
        /// <summary>
        /// Angle counterclockwise from right
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float AngleFromDirection(this Vector3 direction) =>
            AngleFromDirection((Vector2)direction);
    }
}
