using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects {
    public abstract class StateMachineData<T> : ScriptableObject where T : Enum {
        public List<StateData> statesData;

        [Serializable]
        public class StateData {
            public T originStateType;
            public List<TransitionData> transitionsData;
            [EnumFlags]
            public T statesSyncedWith;

            [Serializable]
            public class TransitionData {
                public T destinationStateType;
                public bool canInterrupts;
            }
        }
    }

    public class EnumFlags : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(EnumFlags))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }
}