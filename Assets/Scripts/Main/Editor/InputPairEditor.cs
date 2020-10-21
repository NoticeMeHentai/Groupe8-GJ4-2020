using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomInput;
[CustomPropertyDrawer(typeof(InputPair))]
public class InputPairEditor : PropertyDrawer
{
    private static float lineSpace = 3;
    private static float usualSpace = 16;
    private static float manualIndent = 10;

    private static SerializedProperty m_Action;
    private static SerializedProperty m_IsJoystick;
    private static SerializedProperty m_KeyboardInput;
    private static SerializedProperty m_NegativeKeyboardInput;
    private static SerializedProperty m_GamepadButton;
    private static SerializedProperty m_GamepadAxis;
    private static bool m_IsInitialized = false;

    private static void Initialize(SerializedProperty property)
    {
        if (!m_IsInitialized)
        {
            m_Action = property.FindPropertyRelative(nameof(m_Action));
            m_IsJoystick = property.FindPropertyRelative(nameof(m_IsJoystick));
            m_KeyboardInput = property.FindPropertyRelative(nameof(m_KeyboardInput));
            m_NegativeKeyboardInput = property.FindPropertyRelative(nameof(m_NegativeKeyboardInput));
            m_GamepadButton = property.FindPropertyRelative(nameof(m_GamepadButton));
            m_GamepadAxis = property.FindPropertyRelative(nameof(m_GamepadAxis));
            m_IsInitialized = true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        Initialize(property);
        EditorGUI.BeginProperty(position, label, property);

        Rect quad = new Rect(position);
        quad.height = GetPropertyHeight(property, label);
        EditorGUI.DrawRect(quad, new Color(0.2f, 0.2f, 0.2f, 0.2f));

        position = EditorGUI.PrefixLabel(position, new GUIContent("Input:" + m_Action.stringValue));
        position.height = usualSpace;

        CreateSpace(ref position);

        m_Action.stringValue = EditorGUI.TextField(position, new GUIContent("Input's name"), m_Action.stringValue);


        CreateSpace(ref position);
        position.height = usualSpace;
        m_IsJoystick.boolValue = EditorGUI.Toggle(position,new GUIContent("Is this a Joystick input?"), m_IsJoystick.boolValue);
        position.y += usualSpace;

        //Joystick Option
        if (m_IsJoystick.boolValue)
        {
            m_GamepadAxis.intValue = (int)(ControllerAxis)EditorGUI.EnumFlagsField(position, new GUIContent("Gamepad axis:"), (ControllerAxis)m_GamepadAxis.intValue);
            CreateSpace(ref position);
            m_KeyboardInput.intValue = (int)(KeyCode)EditorGUI.EnumFlagsField(position, new GUIContent("Keyboard positive:"), (KeyCode)m_KeyboardInput.intValue);
            CreateSpace(ref position);
            m_NegativeKeyboardInput.intValue = (int)(KeyCode)EditorGUI.EnumFlagsField(position, new GUIContent("Keyboard negative:"), (KeyCode)m_NegativeKeyboardInput.intValue);
        }
        else
        {
            m_GamepadButton.intValue = (int)(ControllerButtons)EditorGUI.EnumFlagsField(position, new GUIContent("Gamepad button:"), (ControllerButtons)m_GamepadButton.intValue);
            CreateSpace(ref position);
            m_KeyboardInput.intValue = (int)(KeyCode)EditorGUI.EnumFlagsField(position, new GUIContent("Keyboard button:"), (KeyCode)m_KeyboardInput.intValue);
        }

        EditorGUI.EndProperty();

    }

    private void CreateSpace(ref Rect rect)
    {
        rect.y += usualSpace;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Initialize(property);

        return usualSpace * (m_IsJoystick.boolValue ? 6 : 5) + usualSpace*0.5f;
    }
}
