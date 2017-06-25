using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Components.Editor
{
    [CustomEditor(typeof(RenderPipelineSwitcher))]
    public class RenderPipelineSwitcherEditor : UnityEditor.Editor
    {
        ReorderableList m_List;

        void OnEnable()
        {
            var serializedProperty = serializedObject.FindProperty("pipelines");
            m_List = new ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            m_List.drawElementCallback = DrawElement;
            m_List.drawHeaderCallback = DrawHeader;
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Render Pipelines");
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_List.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("keyCode"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("asset"), GUIContent.none);
            //EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("asset"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
