using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MuonhoryoLibrary.Serialization;

namespace MuonhoryoLibrary.UnityEditor
{
    /// <summary>
    /// Source of properties with tag "Inspector field" must be public field/private field with [SerializeField] attribute
    /// and named by {property name}_{property type}.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IDictionaryEditorDrawHelper<TKey, TValue>
    {
        public bool isShowingList { get; set; }
        public string SerializationPath { get; }
        /// <summary>
        /// Inspector field
        /// </summary>
        public TKey NewKey { get; set; }
        /// <summary>
        /// Inspector field
        /// </summary>
        public TValue CurrentValue { get; set; }
        public int CurrentEditIndex { get; set; }
    }
    public abstract class DictionaryUnityEditor<TKey, TValue> : Editor,IDictionaryEditorDrawHelper<TKey,TValue>
    {
        public abstract Dictionary<TKey,TValue> DrawableDictionary { get; }
        public string serializationPath { get; protected set; }
        private bool IsShowingList;
        bool IDictionaryEditorDrawHelper<TKey, TValue>.isShowingList
        {
            get => IsShowingList;
            set => IsShowingList = value;
        }
        string IDictionaryEditorDrawHelper<TKey, TValue>.SerializationPath => serializationPath;
        public abstract TKey NewKey { get; set; }
        public abstract TValue CurrentValue { get; set; }
        private int currentEditIndex;
        int IDictionaryEditorDrawHelper<TKey, TValue>.CurrentEditIndex
        {
            get => currentEditIndex;
            set => currentEditIndex = value;
        }
    }
    /// <summary>
    /// Help to draw interface.
    /// </summary>
    /// <typeparam name="TInterfaceType"></typeparam>
    public abstract class InterfaceDrawer<TInterfaceType> where TInterfaceType:class
    {
        public TInterfaceType DrawedInterface;
        public MonoBehaviour InterfaceComponent;
        /// <summary>
        /// Translate InterfaceComponent as TInterfaceType and set his in DrawedInterface.
        /// </summary>
        public void InitializeInterface()
        {
            if (DrawedInterface == null)
            {
                DrawedInterface=InterfaceComponent as TInterfaceType;
            }
        }
    }
    public static class EditorDraws
    {
        /// <summary>
        /// Show interface field in inspector. Interface must be inherted by the component.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <param name="serializableInterface"></param>
        /// <param name="sourceComponent"></param>
        /// <param name="inspectorLabelText"></param>
        public static void DrawInterface<TInterfaceType>(ref TInterfaceType serializableInterface,
            ref MonoBehaviour sourceComponent,string inspectorLabelText = "")
            where TInterfaceType : class
        {
            TInterfaceType oldInterface = serializableInterface;
            sourceComponent = EditorGUILayout.ObjectField(inspectorLabelText,
                sourceComponent, typeof(MonoBehaviour), true) as MonoBehaviour;
            if (sourceComponent != null)
            {
                serializableInterface = sourceComponent as TInterfaceType;
                if (serializableInterface == null && !sourceComponent.TryGetComponent(out serializableInterface))
                {
                    sourceComponent = null;
                }
                else
                {
                    sourceComponent = serializableInterface as MonoBehaviour;
                }
            }
            if (oldInterface != serializableInterface)
            {
                EditorUtility.SetDirty(sourceComponent);
            }
        }
        public static void DrawInterface<TInterfaceType>(this InterfaceDrawer<TInterfaceType> interfaceDrawer,
            string inspectorLabelText = "") where TInterfaceType : class
        {
            DrawInterface(ref interfaceDrawer.DrawedInterface,ref interfaceDrawer.InterfaceComponent, inspectorLabelText);
        }
        /// <summary>
        /// Show dictionary in inspector. Provides the ability to edit collection.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="showingDictionary"></param>
        /// <param name="serializedObj"></param>
        /// <param name="dictHelper"></param>
        /// <param name="inspectorLabelText"></param>
        public static void DrawDictionary<TKey, TValue>(Dictionary<TKey, TValue> showingDictionary,
            SerializedObject serializedObj, IDictionaryEditorDrawHelper<TKey, TValue> dictHelper,
            string inspectorLabelText = "")
        {
            if (dictHelper.isShowingList = EditorGUILayout.BeginFoldoutHeaderGroup(dictHelper.isShowingList, inspectorLabelText))
            {
                showingDictionary = Serialization.Serialization.DictionarySerializator.
                    Read<TKey,TValue>(dictHelper.SerializationPath, UnityJsonSerializer.Instance);
                bool isChanged = false;
                TKey[] keyArray = new TKey[showingDictionary.Count];
                showingDictionary.Keys.CopyTo(keyArray, 0);
                //Show elements
                {
                    void ShowHeader(int index,bool isShowingHeader=false)
                    {
                        if (EditorGUILayout.BeginFoldoutHeaderGroup(isShowingHeader, keyArray[index].ToString()))
                        {
                            dictHelper.CurrentEditIndex = index;
                            ShowSelected(index);
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    }
                    void ShowSelected(int index)
                    {
                        dictHelper.CurrentValue = showingDictionary[keyArray[index]];
                        EditorGUILayout.PropertyField(serializedObj.FindProperty("CurrentValue_" + typeof(TValue).Name));
                        if (GUILayout.Button("Remove"))
                        {
                            showingDictionary.Remove(keyArray[index]);
                            isChanged = true;
                            dictHelper.CurrentEditIndex = -1;
                        }
                    }
                    void ShowHeaderAndSelected(int index)
                    {
                        ShowHeader(index, index == dictHelper.CurrentEditIndex);
                    }
                    if (dictHelper.CurrentEditIndex >= 0)
                    {
                        if (dictHelper.CurrentEditIndex >= showingDictionary.Count)
                        {
                            dictHelper.CurrentEditIndex = -1;
                            for (int i = 0; i < showingDictionary.Count; i++)
                            {
                                ShowHeader(i);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < showingDictionary.Count; i++)
                            {
                                ShowHeaderAndSelected(i);
                                /*
                                dictHelper.TemporalValue = showingDictionary[keyArray[i]];
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.
                                EditorGUILayout.PropertyField(serialiabledObj.FindProperty("temporalValue_" + typeof(TValue).Name),
                                    new GUIContent(keyArray[i].ToString()));
                                serialiabledObj.ApplyModifiedProperties();
                                if (GUILayout.Button("Remove"))
                                {
                                    showingDictionary.Remove(keyArray[i]);
                                    isChanged = true;
                                }
                                else if (!dictHelper.TemporalValue.Equals(showingDictionary[keyArray[i]]))
                                {
                                    showingDictionary[keyArray[i]] = dictHelper.TemporalValue;
                                    isChanged = true;
                                }
                                EditorGUILayout.EndHorizontal();*/
                            }
                        }
                    }
                    else
                    {
                        for(int i = 0; i < showingDictionary.Count; i++)
                        {
                            ShowHeader(i);
                        }
                    }
                }
                //Show adding menu
                EditorGUILayout.PropertyField(serializedObj.FindProperty("NewKey_" + typeof(TKey).Name),
                    new GUIContent("New element"));
                serializedObj.ApplyModifiedProperties();
                if (GUILayout.Button("Add") && dictHelper.NewKey != null && 
                    !showingDictionary.ContainsKey(dictHelper.NewKey))
                {
                    showingDictionary.Add(dictHelper.NewKey, default);
                    dictHelper.NewKey = default;
                    isChanged = true;
                }
                //Update dictionary
                if (isChanged)
                {
                    Serialization.Serialization.DictionarySerializator.Write(dictHelper.SerializationPath, showingDictionary,
                    UnityJsonSerializer.Instance);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public static void DrawDictionary<TKey,TValue>(DictionaryUnityEditor<TKey,TValue> editor,
            string inspectorLabelText = "")
        {
            DrawDictionary(editor.DrawableDictionary, new SerializedObject(editor), editor, inspectorLabelText);
        }
    }
}
