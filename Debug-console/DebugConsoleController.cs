


using Microsoft.Win32;
using System;
using UnityEngine;

namespace MuonhoryoLibrary.Unity.Debug
{
    public sealed class DebugConsoleController : MonoBehaviour, ISingltone<DebugConsoleController>
    {
        [SerializeField] private string ButtonName_ConsoleActivation;
        [SerializeField] private string ButtonName_ExecuteCommand;
        [SerializeField] private string ButtonName_DeleteLastSym;
        [SerializeField] private string ButtonName_ConsoleDeactivation;

        public static DebugConsoleController Instance_ { get; private set; }
        DebugConsoleController ISingltone<DebugConsoleController>.Singltone
        {
            get => Instance_;
            set => Instance_ = value;
        }
        void ISingltone<DebugConsoleController>.Destroy() => Destroy(this);

        public event Action ConsoleControllerEnabledEvent = delegate { };
        public event Action ConsoleControllerDisabledEvent = delegate { };

        public bool IsActive_ { get; private set; } = false;

        private void ActivationAction()
        {
            IsActive_ = true;
            ConsoleControllerEnabledEvent();
        }
        private void DeactivationAction()
        {
            IsActive_ = false;
            ConsoleControllerDisabledEvent();
        }

        private void Awake()
        {
            SingltoneInitializations.InitializationForFirstExample(this, () =>
            {
                DontDestroyOnLoad(gameObject);
            });
        }

        private void InactiveUpdate()
        {
            if (Input.GetButtonUp(ButtonName_ConsoleActivation))
            {
                ActivationAction();
            }
        }
        private void ActiveUpdate()
        {
            if (Input.GetButtonDown(ButtonName_ExecuteCommand))
            {
                DebugConsole.RunCommand();
                DeactivationAction();
            }
            else if (Input.GetButtonDown(ButtonName_DeleteLastSym))
            {
                DebugConsole.RemoveLast();
            }
            else if (Input.GetButtonDown(ButtonName_ConsoleDeactivation))
            {
                DebugConsole.ResetConsole();
                DeactivationAction();
            }
            else
            {
                DebugConsole.AddKeyboardInput(Input.inputString);
            }
        }
        private void Update()
        {
            if (IsActive_)
                ActiveUpdate();
            else
                InactiveUpdate();
        }
    }
}
