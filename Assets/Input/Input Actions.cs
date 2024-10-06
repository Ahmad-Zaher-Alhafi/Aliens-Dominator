//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/Input/Input Actions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @InputActions : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Input Actions"",
    ""maps"": [
        {
            ""name"": ""Shared Actions"",
            ""id"": ""0f2463f9-7aa9-41e3-9c2d-9db2518a0899"",
            ""actions"": [
                {
                    ""name"": ""Primary Action"",
                    ""type"": ""Button"",
                    ""id"": ""94cf9182-9a5d-42ac-b9f2-6e362ed27f5b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Secondary Action"",
                    ""type"": ""Button"",
                    ""id"": ""edf206fb-c1d0-4adc-bf5c-5bf8e43b102d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""FPS View Action"",
                    ""type"": ""Button"",
                    ""id"": ""fcb71ba9-061a-420a-84c3-8ef7f1480236"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Top Down View Action"",
                    ""type"": ""Button"",
                    ""id"": ""c57f88c1-fa09-4e7a-b58c-db9ba8db6439"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4cbf8129-928c-4a29-b5dc-6f1c6bdbd81f"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Primary Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b73aa903-ee3b-429e-a273-57209eeee375"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Secondary Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""19be8f00-888e-4b37-b4f5-4d833e950f94"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FPS View Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4657b5b4-7cfa-4d77-ac3c-0944d68ee729"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Top Down View Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Editor Actions"",
            ""id"": ""433e8586-ef7c-4147-82e2-971136107cad"",
            ""actions"": [
                {
                    ""name"": ""Change Game Window Size"",
                    ""type"": ""Button"",
                    ""id"": ""9c68a32c-2029-40c1-8cb1-20ecb563f869"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""LShift + Space"",
                    ""id"": ""06f69e08-09f9-42f9-8c18-36c5f026444b"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Change Game Window Size"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""dad0daaa-5026-4ecb-a739-2edf7edf6eee"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Change Game Window Size"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""5c53ccf6-d944-4ee6-851c-562896bf983f"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Change Game Window Size"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""UI Actions"",
            ""id"": ""d82cca22-9ff9-4096-9f05-4b0b83328020"",
            ""actions"": [
                {
                    ""name"": ""Escape"",
                    ""type"": ""Button"",
                    ""id"": ""d7006468-aebe-4c55-9881-cd724125f4c5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""cfe3e472-e1c7-426f-a567-29d8a64158a8"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Escape"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""FPS View Actions"",
            ""id"": ""cd746fcf-5d60-46f3-bb56-4c70be8ca521"",
            ""actions"": [
                {
                    ""name"": ""Draw"",
                    ""type"": ""Button"",
                    ""id"": ""d88198b4-ff73-44ef-a776-03abcdb15bad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotation Axis"",
                    ""type"": ""Value"",
                    ""id"": ""ab787ac3-7600-41f6-ab96-2850d4de7a8c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3c8ed1db-a630-4eca-9a17-f018a93781f3"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Draw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2b1a3b1e-0590-4111-9e4c-6a1e9d1494d9"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation Axis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Top Down View Actions"",
            ""id"": ""3f4005a4-e73c-48e5-af2b-10bdf41b8017"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""2202eee9-727b-4be8-b321-1abf18b045ec"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""6d78b36f-90e9-463a-a8fb-f92112f0edde"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Shared Actions
        m_SharedActions = asset.FindActionMap("Shared Actions", throwIfNotFound: true);
        m_SharedActions_PrimaryAction = m_SharedActions.FindAction("Primary Action", throwIfNotFound: true);
        m_SharedActions_SecondaryAction = m_SharedActions.FindAction("Secondary Action", throwIfNotFound: true);
        m_SharedActions_FPSViewAction = m_SharedActions.FindAction("FPS View Action", throwIfNotFound: true);
        m_SharedActions_TopDownViewAction = m_SharedActions.FindAction("Top Down View Action", throwIfNotFound: true);
        // Editor Actions
        m_EditorActions = asset.FindActionMap("Editor Actions", throwIfNotFound: true);
        m_EditorActions_ChangeGameWindowSize = m_EditorActions.FindAction("Change Game Window Size", throwIfNotFound: true);
        // UI Actions
        m_UIActions = asset.FindActionMap("UI Actions", throwIfNotFound: true);
        m_UIActions_Escape = m_UIActions.FindAction("Escape", throwIfNotFound: true);
        // FPS View Actions
        m_FPSViewActions = asset.FindActionMap("FPS View Actions", throwIfNotFound: true);
        m_FPSViewActions_Draw = m_FPSViewActions.FindAction("Draw", throwIfNotFound: true);
        m_FPSViewActions_RotationAxis = m_FPSViewActions.FindAction("Rotation Axis", throwIfNotFound: true);
        // Top Down View Actions
        m_TopDownViewActions = asset.FindActionMap("Top Down View Actions", throwIfNotFound: true);
        m_TopDownViewActions_Newaction = m_TopDownViewActions.FindAction("New action", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Shared Actions
    private readonly InputActionMap m_SharedActions;
    private ISharedActionsActions m_SharedActionsActionsCallbackInterface;
    private readonly InputAction m_SharedActions_PrimaryAction;
    private readonly InputAction m_SharedActions_SecondaryAction;
    private readonly InputAction m_SharedActions_FPSViewAction;
    private readonly InputAction m_SharedActions_TopDownViewAction;
    public struct SharedActionsActions
    {
        private @InputActions m_Wrapper;
        public SharedActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @PrimaryAction => m_Wrapper.m_SharedActions_PrimaryAction;
        public InputAction @SecondaryAction => m_Wrapper.m_SharedActions_SecondaryAction;
        public InputAction @FPSViewAction => m_Wrapper.m_SharedActions_FPSViewAction;
        public InputAction @TopDownViewAction => m_Wrapper.m_SharedActions_TopDownViewAction;
        public InputActionMap Get() { return m_Wrapper.m_SharedActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SharedActionsActions set) { return set.Get(); }
        public void SetCallbacks(ISharedActionsActions instance)
        {
            if (m_Wrapper.m_SharedActionsActionsCallbackInterface != null)
            {
                @PrimaryAction.started -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnPrimaryAction;
                @PrimaryAction.performed -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnPrimaryAction;
                @PrimaryAction.canceled -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnPrimaryAction;
                @SecondaryAction.started -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnSecondaryAction;
                @SecondaryAction.performed -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnSecondaryAction;
                @SecondaryAction.canceled -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnSecondaryAction;
                @FPSViewAction.started -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnFPSViewAction;
                @FPSViewAction.performed -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnFPSViewAction;
                @FPSViewAction.canceled -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnFPSViewAction;
                @TopDownViewAction.started -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnTopDownViewAction;
                @TopDownViewAction.performed -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnTopDownViewAction;
                @TopDownViewAction.canceled -= m_Wrapper.m_SharedActionsActionsCallbackInterface.OnTopDownViewAction;
            }
            m_Wrapper.m_SharedActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @PrimaryAction.started += instance.OnPrimaryAction;
                @PrimaryAction.performed += instance.OnPrimaryAction;
                @PrimaryAction.canceled += instance.OnPrimaryAction;
                @SecondaryAction.started += instance.OnSecondaryAction;
                @SecondaryAction.performed += instance.OnSecondaryAction;
                @SecondaryAction.canceled += instance.OnSecondaryAction;
                @FPSViewAction.started += instance.OnFPSViewAction;
                @FPSViewAction.performed += instance.OnFPSViewAction;
                @FPSViewAction.canceled += instance.OnFPSViewAction;
                @TopDownViewAction.started += instance.OnTopDownViewAction;
                @TopDownViewAction.performed += instance.OnTopDownViewAction;
                @TopDownViewAction.canceled += instance.OnTopDownViewAction;
            }
        }
    }
    public SharedActionsActions @SharedActions => new SharedActionsActions(this);

    // Editor Actions
    private readonly InputActionMap m_EditorActions;
    private IEditorActionsActions m_EditorActionsActionsCallbackInterface;
    private readonly InputAction m_EditorActions_ChangeGameWindowSize;
    public struct EditorActionsActions
    {
        private @InputActions m_Wrapper;
        public EditorActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @ChangeGameWindowSize => m_Wrapper.m_EditorActions_ChangeGameWindowSize;
        public InputActionMap Get() { return m_Wrapper.m_EditorActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(EditorActionsActions set) { return set.Get(); }
        public void SetCallbacks(IEditorActionsActions instance)
        {
            if (m_Wrapper.m_EditorActionsActionsCallbackInterface != null)
            {
                @ChangeGameWindowSize.started -= m_Wrapper.m_EditorActionsActionsCallbackInterface.OnChangeGameWindowSize;
                @ChangeGameWindowSize.performed -= m_Wrapper.m_EditorActionsActionsCallbackInterface.OnChangeGameWindowSize;
                @ChangeGameWindowSize.canceled -= m_Wrapper.m_EditorActionsActionsCallbackInterface.OnChangeGameWindowSize;
            }
            m_Wrapper.m_EditorActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ChangeGameWindowSize.started += instance.OnChangeGameWindowSize;
                @ChangeGameWindowSize.performed += instance.OnChangeGameWindowSize;
                @ChangeGameWindowSize.canceled += instance.OnChangeGameWindowSize;
            }
        }
    }
    public EditorActionsActions @EditorActions => new EditorActionsActions(this);

    // UI Actions
    private readonly InputActionMap m_UIActions;
    private IUIActionsActions m_UIActionsActionsCallbackInterface;
    private readonly InputAction m_UIActions_Escape;
    public struct UIActionsActions
    {
        private @InputActions m_Wrapper;
        public UIActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Escape => m_Wrapper.m_UIActions_Escape;
        public InputActionMap Get() { return m_Wrapper.m_UIActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActionsActions set) { return set.Get(); }
        public void SetCallbacks(IUIActionsActions instance)
        {
            if (m_Wrapper.m_UIActionsActionsCallbackInterface != null)
            {
                @Escape.started -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEscape;
                @Escape.performed -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEscape;
                @Escape.canceled -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEscape;
            }
            m_Wrapper.m_UIActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Escape.started += instance.OnEscape;
                @Escape.performed += instance.OnEscape;
                @Escape.canceled += instance.OnEscape;
            }
        }
    }
    public UIActionsActions @UIActions => new UIActionsActions(this);

    // FPS View Actions
    private readonly InputActionMap m_FPSViewActions;
    private IFPSViewActionsActions m_FPSViewActionsActionsCallbackInterface;
    private readonly InputAction m_FPSViewActions_Draw;
    private readonly InputAction m_FPSViewActions_RotationAxis;
    public struct FPSViewActionsActions
    {
        private @InputActions m_Wrapper;
        public FPSViewActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Draw => m_Wrapper.m_FPSViewActions_Draw;
        public InputAction @RotationAxis => m_Wrapper.m_FPSViewActions_RotationAxis;
        public InputActionMap Get() { return m_Wrapper.m_FPSViewActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(FPSViewActionsActions set) { return set.Get(); }
        public void SetCallbacks(IFPSViewActionsActions instance)
        {
            if (m_Wrapper.m_FPSViewActionsActionsCallbackInterface != null)
            {
                @Draw.started -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnDraw;
                @Draw.performed -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnDraw;
                @Draw.canceled -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnDraw;
                @RotationAxis.started -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnRotationAxis;
                @RotationAxis.performed -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnRotationAxis;
                @RotationAxis.canceled -= m_Wrapper.m_FPSViewActionsActionsCallbackInterface.OnRotationAxis;
            }
            m_Wrapper.m_FPSViewActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Draw.started += instance.OnDraw;
                @Draw.performed += instance.OnDraw;
                @Draw.canceled += instance.OnDraw;
                @RotationAxis.started += instance.OnRotationAxis;
                @RotationAxis.performed += instance.OnRotationAxis;
                @RotationAxis.canceled += instance.OnRotationAxis;
            }
        }
    }
    public FPSViewActionsActions @FPSViewActions => new FPSViewActionsActions(this);

    // Top Down View Actions
    private readonly InputActionMap m_TopDownViewActions;
    private ITopDownViewActionsActions m_TopDownViewActionsActionsCallbackInterface;
    private readonly InputAction m_TopDownViewActions_Newaction;
    public struct TopDownViewActionsActions
    {
        private @InputActions m_Wrapper;
        public TopDownViewActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_TopDownViewActions_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_TopDownViewActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TopDownViewActionsActions set) { return set.Get(); }
        public void SetCallbacks(ITopDownViewActionsActions instance)
        {
            if (m_Wrapper.m_TopDownViewActionsActionsCallbackInterface != null)
            {
                @Newaction.started -= m_Wrapper.m_TopDownViewActionsActionsCallbackInterface.OnNewaction;
                @Newaction.performed -= m_Wrapper.m_TopDownViewActionsActionsCallbackInterface.OnNewaction;
                @Newaction.canceled -= m_Wrapper.m_TopDownViewActionsActionsCallbackInterface.OnNewaction;
            }
            m_Wrapper.m_TopDownViewActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Newaction.started += instance.OnNewaction;
                @Newaction.performed += instance.OnNewaction;
                @Newaction.canceled += instance.OnNewaction;
            }
        }
    }
    public TopDownViewActionsActions @TopDownViewActions => new TopDownViewActionsActions(this);
    public interface ISharedActionsActions
    {
        void OnPrimaryAction(InputAction.CallbackContext context);
        void OnSecondaryAction(InputAction.CallbackContext context);
        void OnFPSViewAction(InputAction.CallbackContext context);
        void OnTopDownViewAction(InputAction.CallbackContext context);
    }
    public interface IEditorActionsActions
    {
        void OnChangeGameWindowSize(InputAction.CallbackContext context);
    }
    public interface IUIActionsActions
    {
        void OnEscape(InputAction.CallbackContext context);
    }
    public interface IFPSViewActionsActions
    {
        void OnDraw(InputAction.CallbackContext context);
        void OnRotationAxis(InputAction.CallbackContext context);
    }
    public interface ITopDownViewActionsActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
}