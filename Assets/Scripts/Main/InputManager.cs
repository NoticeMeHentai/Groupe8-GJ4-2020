using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CustomInput
{
    public enum ControllerButtons { X, Circle, Square, Triangle, L1, R1, Options, Share, L2, R2, Right, Left, Up, Down, L3, R3 }
    public enum ControllerAxis { LeftJoystickHor, LeftJoystickVer, RightJoystickHor, RightJoystickVer, LR2 }

    [System.Serializable]
    public class InputPair
    {
        public string m_Action;
        public bool m_IsJoystick;
        public KeyCode m_KeyboardInput;
        public KeyCode m_NegativeKeyboardInput;
        public ControllerButtons m_GamepadButton;
        public ControllerAxis m_GamepadAxis;
    }


    public static class InputManager 
    {
        static InputManager()
        {
            UpdateCaller.AddUpdateCallback(Update);
        }
        public static List<InputPair> sInputsList = new List<InputPair>();

        private static void Update()
        {
            Debug.Log("Top kek");
            for(int i = 0; i < sInputsList.Count; i++)
            {
                InputPair current = sInputsList[i];
                if (!current.m_IsJoystick && (Input.GetKeyDown(current.m_KeyboardInput))) Debug.Log("Hit! ! !");
            }
        }


    }
}
