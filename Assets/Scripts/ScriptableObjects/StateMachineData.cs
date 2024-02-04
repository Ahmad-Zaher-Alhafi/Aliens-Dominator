using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects {
    public abstract class StateMachineData<TType> : ScriptableObject where TType : Enum {
        public List<StateData> statesData;

        [Serializable]
        public class StateData {
            public TType originStateType;
            public List<TransitionData> transitionsData;
            [EnumFlags]
            public TType statesSyncedWithMask;
            [EnumFlags]
            public TType interruptStatesMask;
            [Tooltip("Set to true if you want this state to be checked even when the automation disabled")]
            public bool checkedWhenAutomationDisabled;

            [Serializable]
            public class TransitionData {
                public TType destinationStateType;
            }
        }
    }

    public class EnumFlags : PropertyAttribute { }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlags))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }
    #endif
}