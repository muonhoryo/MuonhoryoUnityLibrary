using System;
using UnityEngine;

namespace MuonhoryoLibrary.Unity.Debug
{
    public sealed class DebugConsole:MonoBehaviour,ISingltone<DebugConsole>
    {
        public static DebugConsole Instance_ { get; private set; }
        DebugConsole ISingltone<DebugConsole>.Singltone
        {
            get => Instance_;
            set=>Instance_ = value;
        }

        void ISingltone<DebugConsole>.Destroy() => Destroy(this);

        public static event Action<string> InputChangedEvent = delegate { };
        public static event Action<string> ConsoleCommandExecutedEvent = delegate { };

        private static string ConsoleInput_ = "";
        public static string ConsoleInput 
        { 
            get=>ConsoleInput_;
            private set 
            {
                ConsoleInput_ = value;
                InputChangedEvent(ConsoleInput_);
            } 
        }

        public static void AddKeyboardInput(string input)
        {
            ConsoleInput = ConsoleInput + input;
        }
        public static void RemoveLast()
        {
            if (ConsoleInput.Length > 0)
            {
                int newL = ConsoleInput.Length - 1;
                ConsoleInput = ConsoleInput.Substring(0, newL);
            }
        }
        public static void ResetConsole()
        {
            ConsoleInput = "";
        }
        public static void RunCommand()
        {
            ConsoleCommandExecutedEvent(ConsoleInput);
            ConsoleInput = "";
        }

        private void Awake()
        {
            SingltoneInitializations.InitializationForFirstExample(this, () => { });
        }
    }
}
