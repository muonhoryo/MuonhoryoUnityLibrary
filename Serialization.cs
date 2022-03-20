using UnityEngine;

namespace MuonhoryoLibrary.Serialization
{
    public sealed class UnityJsonSerializer : ISerializator
    {
        private UnityJsonSerializer() { }
        public static readonly UnityJsonSerializer Instance = new UnityJsonSerializer();
        T ISerializator.Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
        string ISerializator.Serialize<T>(T obj)
        {
            return JsonUtility.ToJson(obj,true);
        }
    }
}
