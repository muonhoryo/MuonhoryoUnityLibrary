using UnityEngine;

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
    }
}
