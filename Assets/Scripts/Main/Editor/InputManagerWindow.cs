using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomInput;

public class InputManagerWindow : EditorWindow
{
    //[SerializeField] private static List<InputPair> _InputPairs = new List<InputPair>();
    [SerializeField] private InputPair[] InputPairs = new InputPair[3];

    [SerializeField] private InputPair displayTest;

    [MenuItem("Window/Custom Inputs")]
    public static void ShowWindow()
    {
        GetWindow<InputManagerWindow>();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Custom editor");
        ScriptableObject target = this;
        SerializedObject test = new SerializedObject(target);

        SerializedProperty displayTestProperty = test.FindProperty(nameof(displayTest));
        //SerializedProperty testPropertyStatic = test.FindProperty("_InputPairs");
        SerializedProperty testProperty = test.FindProperty(nameof(InputPairs));
        EditorGUILayout.PropertyField(displayTestProperty,true);
        //EditorGUILayout.PropertyField(testPropertyStatic, true);
        EditorGUILayout.PropertyField(testProperty, true);

        test.ApplyModifiedProperties();

    }
}
