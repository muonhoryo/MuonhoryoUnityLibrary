using UnityEngine;
using MuonhoryoLibrary.UnityEditor;

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
        /// Translate InterfaceComponent as TInterfaceType and set it in DrawedInterface.
        /// </summary>
        public static void InitInterface<TInterfaceType>(this IInterfaceDrawer<TInterfaceType> drawer)
            where TInterfaceType : class
        {
            if (drawer.DrawedInterface == null)
            {
                drawer.DrawedInterface =drawer.InterfaceComponent as TInterfaceType;
            }
        }
    }
}
