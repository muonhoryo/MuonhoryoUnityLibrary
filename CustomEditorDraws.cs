using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MuonhoryoLibrary.Serialization;

namespace MuonhoryoLibrary.UnityEditor
{
    public interface IDictionaryEditorDrawHelper<TKey, TValue>
    {
        bool isShowingList { get; set; }
        string SerializationPath { get; }
        TKey NewKey { get; set; }
        TValue CurrentValue { get; set; }
        string NewKeyPropertyName { get; }
        string CurrentValuePropertyName { get; }
        int CurrentEditIndex { get; set; }
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
        public abstract string NewKeyPropertyName { get; }
        public abstract string CurrentValuePropertyName { get; }
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
    public class InterfaceDrawer<TInterfaceType> where TInterfaceType:class
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
            ISerializator serializator,string inspectorLabelText = "")
        {
            if (dictHelper.isShowingList = EditorGUILayout.BeginFoldoutHeaderGroup(dictHelper.isShowingList, inspectorLabelText))
            {
                showingDictionary = Serialization.Serialization.DictionarySerializator.
                    Read<TKey,TValue>(dictHelper.SerializationPath, serializator);
                bool isChanged = false;
                TKey[] keyArray = new TKey[showingDictionary.Count];
                showingDictionary.Keys.CopyTo(keyArray, 0);
                //Show elements
                {
                    void ShowHeader(int index,bool isShowingHeader=false)
                    {
                        if (EditorGUILayout.BeginToggleGroup(keyArray[index].ToString(),isShowingHeader))
                        {
                            dictHelper.CurrentEditIndex = index;
                            ShowSelected(index);
                        }
                        else if (isShowingHeader)
                        {
                            dictHelper.CurrentEditIndex = -1;
                        }
                        EditorGUILayout.EndToggleGroup();
                    }
                    void ShowSelected(int index)
                    {
                        dictHelper.CurrentValue = showingDictionary[keyArray[index]];
                        EditorGUILayout.PropertyField(
                            serializedObj.FindProperty(dictHelper.CurrentValuePropertyName));
                        serializedObj.ApplyModifiedProperties();
                        if (GUILayout.Button("Remove"))
                        {
                            showingDictionary.Remove(keyArray[index]);
                            isChanged = true;
                            dictHelper.CurrentEditIndex = -1;
                        }
                        else if (!dictHelper.CurrentValue.Equals(showingDictionary[keyArray[index]]))
                        {
                            showingDictionary[keyArray[index]] = dictHelper.CurrentValue;
                            isChanged = true;
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
                EditorGUILayout.PropertyField(serializedObj.FindProperty(dictHelper.NewKeyPropertyName),
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
                    Serialization.Serialization.DictionarySerializator.Write(dictHelper.SerializationPath,
                        showingDictionary,serializator);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public static void DrawDictionary<TKey, TValue>(Dictionary<TKey, TValue> showingDictionary,
           SerializedObject serializedObj, IDictionaryEditorDrawHelper<TKey, TValue> dictHelper,
           string inspectorLabelText = "")
        {
            DrawDictionary(showingDictionary, serializedObj, dictHelper,UnityJsonSerializer.Instance,
                inspectorLabelText);
        }
        public static void DrawDictionary<TKey, TValue>(DictionaryUnityEditor<TKey, TValue> editor,
            string inspectorLabelText = "")
        {
            DrawDictionary(editor,UnityJsonSerializer.Instance,inspectorLabelText);
        }
        public static void DrawDictionary<TKey,TValue>(DictionaryUnityEditor<TKey,TValue> editor,
            ISerializator serializator,string inspectorLabelText = "")
        {
            DrawDictionary(editor.DrawableDictionary,editor.serializedObject, editor,serializator,
                inspectorLabelText);
        }
    }
}
