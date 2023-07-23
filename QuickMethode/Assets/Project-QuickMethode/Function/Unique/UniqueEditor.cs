using QuickMethode;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(Unique))]
public class UniqueEditor : Editor
{
    private Unique m_target;

    private SerializedProperty m_id;

    private void OnEnable()
    {
        m_target = (target as Unique);

        m_id = serializedObject.FindProperty("m_id");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_id);

        if (QGameObject.GetCheckPrefab(m_target.gameObject))
        {
            if (QEditor.SetButton("Refresh"))
                m_target.SetUpdateRefresh();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif