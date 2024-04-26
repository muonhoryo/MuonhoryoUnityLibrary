
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MuonhoryoLibrary.Unity
{
    public sealed class OnStartInactiveObject:MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
