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
    public interface IInterfaceDrawer<TInterfaceType> where TInterfaceType : class
    {
        TInterfaceType DrawedInterface { get; set; }
        MonoBehaviour InterfaceComponent { get; set; }
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
    public static class EditorDraws
    {
        /// <summary>
        /// Show interface field in inspector. Interface must be inherted by the component.
        /// </summary>
        /// <typeparam name="TInterfaceType"></typeparam>
        /// <param name="serializableInterface"></param>
        /// <param name="sourceComponent"></param>
        /// <param name="inspectorLabelText"></param>
        public static void DrawInterface<TInterfaceType>(this IInterfaceDrawer<TInterfaceType> interfaceDrawer,
            string inspectorLabelText = "") where TInterfaceType : class
        {
            bool tryGetInterfaceInHierarchy()
            {
                interfaceDrawer.DrawedInterface =
                    interfaceDrawer.InterfaceComponent.GetComponent<TInterfaceType>();
                return interfaceDrawer.DrawedInterface == null;
            }
            TInterfaceType oldInterface = interfaceDrawer.DrawedInterface;
            interfaceDrawer.InterfaceComponent = EditorGUILayout.ObjectField(inspectorLabelText,
                interfaceDrawer.InterfaceComponent, typeof(MonoBehaviour), true) as MonoBehaviour;
            if (interfaceDrawer.InterfaceComponent != null)
            {
                interfaceDrawer.DrawedInterface = interfaceDrawer.InterfaceComponent as TInterfaceType;
                if (interfaceDrawer.DrawedInterface == null && !tryGetInterfaceInHierarchy())
                {
                    interfaceDrawer.InterfaceComponent = null;
                }
                else
                {
                    interfaceDrawer.InterfaceComponent = interfaceDrawer.DrawedInterface as MonoBehaviour;
                }
            }
            if (oldInterface != interfaceDrawer.DrawedInterface)
            {
                EditorUtility.SetDirty(interfaceDrawer as MonoBehaviour);
            }
        }


        public static void ReadOnlyDrawInterface<TInterfaceType>(in TInterfaceType drawedInterface,
            string inspectorLabelText = "") where TInterfaceType : class
        {
            EditorGUILayout.ObjectField(inspectorLabelText, drawedInterface as MonoBehaviour,
                typeof(MonoBehaviour), true);
        }
        public static void ReadOnlyDrawInterface<TInterfaceType>(this IInterfaceDrawer<TInterfaceType> drawer,
            string inspectorLabelText = "") where TInterfaceType : class
        {
            ReadOnlyDrawInterface(drawer.DrawedInterface, inspectorLabelText);
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
            ISerializator serializator,string inspectorLabelText = "",string removeButtonText="",
            string newElementKeyText="",string addButtonText="")
        {
            if (dictHelper.isShowingList = EditorGUILayout.BeginFoldoutHeaderGroup(dictHelper.isShowingList,
                inspectorLabelText))
            {
                showingDictionary = DictionarySerializator.
                    ReadOrCreateNewFile<TKey,TValue>(dictHelper.SerializationPath, serializator);
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
                        if (GUILayout.Button(removeButtonText))
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
                    new GUIContent(newElementKeyText));
                serializedObj.ApplyModifiedProperties();
                if (GUILayout.Button(addButtonText) && dictHelper.NewKey != null && 
                    !showingDictionary.ContainsKey(dictHelper.NewKey))
                {
                    showingDictionary.Add(dictHelper.NewKey, default);
                    dictHelper.NewKey = default;
                    isChanged = true;
                }
                //Update dictionary
                if (isChanged)
                {
                    DictionarySerializator.WriteOrCreateNewFile
                        (dictHelper.SerializationPath,showingDictionary,serializator);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public static void DrawDictionary<TKey, TValue>(Dictionary<TKey, TValue> showingDictionary,
           SerializedObject serializedObj, IDictionaryEditorDrawHelper<TKey, TValue> dictHelper,
           string inspectorLabelText = "",string removeButtonText="",string newElementKeyText="",
           string addButtonText="")
        {
            DrawDictionary(showingDictionary, serializedObj, dictHelper,UnityJsonSerializer.Instance,
                inspectorLabelText,removeButtonText,newElementKeyText,addButtonText);
        }
        public static void DrawDictionary<TKey,TValue>(DictionaryUnityEditor<TKey,TValue> editor,
            ISerializator serializator,string inspectorLabelText = "",string removeButtonText="",
            string newElementKeyText="",string addButtonText="")
        {
            DrawDictionary(editor.DrawableDictionary,editor.serializedObject, editor,serializator,
                inspectorLabelText,removeButtonText,newElementKeyText,addButtonText);
        }
        public static void DrawDictionary<TKey, TValue>(DictionaryUnityEditor<TKey, TValue> editor,
            string inspectorLabelText = "", string removeButtonText = "", string newElementKeyText = "",
            string addButtonText = "")
        {
            DrawDictionary(editor, UnityJsonSerializer.Instance, inspectorLabelText, removeButtonText,
                newElementKeyText, addButtonText);
        }


        public static void ReadOnlyDrawDictionary<TKey, TValue>(Dictionary<TKey, TValue> showingDictionary,
            SerializedObject serializedObj, IDictionaryEditorDrawHelper<TKey, TValue> dictHelper,
            ISerializator serializator, string inspectorLabelText = "")
        {
            if (dictHelper.isShowingList = EditorGUILayout.BeginFoldoutHeaderGroup(dictHelper.isShowingList,
                inspectorLabelText))
            {
                showingDictionary = DictionarySerializator. ReadOrCreateNewFile<TKey, TValue>
                    (dictHelper.SerializationPath, serializator);
                TKey[] keyArray = new TKey[showingDictionary.Count];
                showingDictionary.Keys.CopyTo(keyArray, 0);
                {
                    void ShowHeader(int index, bool isShowingHeader = false)
                    {
                        if (EditorGUILayout.BeginToggleGroup(keyArray[index].ToString(), isShowingHeader))
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
                        for (int i = 0; i < showingDictionary.Count; i++)
                        {
                            ShowHeader(i);
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public static void ReadOnlyDrawDictionary<TKey, TValue>(Dictionary<TKey, TValue> showingDictionary,
            SerializedObject serializedObject, IDictionaryEditorDrawHelper<TKey, TValue> dictHelper,
            string inspectorLabelText = "")
        {
            ReadOnlyDrawDictionary(showingDictionary, serializedObject, dictHelper, UnityJsonSerializer.Instance,
                inspectorLabelText);
        }
        public static void ReadOnlyDrawDictionary<TKey,TValue>(DictionaryUnityEditor<TKey, TValue>editor,
            ISerializator serializator,string inspectorLabelText = "")
        {
            ReadOnlyDrawDictionary(editor.DrawableDictionary, editor.serializedObject, editor, serializator,
                inspectorLabelText);
        }
        public static void ReadOnlyDrawDictionary<TKey,TValue>(DictionaryUnityEditor<TKey,TValue>editor,
            string inspectorLabelText = "")
        {
            ReadOnlyDrawDictionary(editor, UnityJsonSerializer.Instance, inspectorLabelText);
        }
    }
}
