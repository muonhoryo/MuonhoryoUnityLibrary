using UnityEngine;
using System;
using System.Security.Permissions;

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
            if (direction.x < 0)
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
        /// <summary>
        /// x;y -> x=x;0;z=y
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 ConvertHorDirToGlobalDir(this Vector2 horDir)=>
            new Vector3(horDir.x, 0, horDir.y);
        public static Vector3 ConvertHorDirToGlobalDir(this Vector3 horDir) =>
            ((Vector2)horDir).ConvertHorDirToGlobalDir();
        public static Vector2 ConvertGlobalDirToHorDir(this Vector3 globalDir)=>
             new Vector2(globalDir.x, globalDir.z);
        public static Vector3 DirectionFromStereoRotation(this Vector2 rotation)
        {
            float xRadAngle = rotation.x * Mathf.Deg2Rad;
            float yRadAngle = rotation.y * Mathf.Deg2Rad;
            return new Vector3
                (Mathf.Sin(yRadAngle) * Mathf.Cos(xRadAngle),
                 -Mathf.Sin(xRadAngle),
                Mathf.Cos(yRadAngle) * Mathf.Cos(xRadAngle));
        }
        public static Vector3 DirectionFromStereoRotation(this Vector3 rotation) =>
            ((Vector2)rotation).DirectionFromStereoRotation();
        /// <summary>
        /// Convert sound attenuation in decibels to volume level (from 0 to 1)
        /// </summary>
        /// <param name="DB"></param>
        /// <returns></returns>
        public static float DBToVolumeLevel(this float attenuation)
        {
            if (attenuation > 0)
                throw new ArgumentOutOfRangeException("attenuation must be less or equal zero.");

            if (attenuation <= -23)
                return 0;

            float belAtt=attenuation/10;
            float volumeLevel=Mathf.Pow(10,belAtt);
            return volumeLevel;
        }
        /// <summary>
        /// Convert volume level (from 0 to 1) to sound attenuation in decibels (min attenuation reached
        /// at volume level = 1)
        /// </summary>
        /// <param name="volumeLevel"></param>
        /// <returns></returns>
        public static float VolumeLevelToDB(this float volumeLevel)
        {
            if (volumeLevel < 0 || volumeLevel > 1)
                throw new ArgumentOutOfRangeException("volumeLevel must be in range [0;1].");

            float outLevel = Mathf.Log10(volumeLevel) * 10;
            if (outLevel <= -23) outLevel = -80;

            return outLevel;
        }
    }
}
