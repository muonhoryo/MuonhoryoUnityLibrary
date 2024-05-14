
using System;
using UnityEngine;

namespace MuonhoryoLibrary.Unity.COM
{
    public abstract class ModuleSelector<TSelectedModule> : MonoBehaviour,IActiveModule
        where TSelectedModule : class
    {
        public event Action DeactivateModuleEvent = delegate { };
        public event Action ActivateModuleEvent = delegate { };
        public event Action<TSelectedModule> SelectModuleEvent = delegate { };

        [SerializeField] private MonoBehaviour[] SelectedModules;
        private TSelectedModule[] ParsedSelectedModules;
        [SerializeField]private int CurrentModuleIndex=0;
        public int CurrentModuleIndex_ { get; private set; } = -1;
        public TSelectedModule CurrentModule_ { get; private set; }
        private bool IsActive = false;

        public bool IsActive_
        {
            get => IsActive;
            set
            {
                if (IsActive != value)
                {
                    IsActive = value;
                    enabled = value;
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = value;
                    if (value)
                    {
                        ActivateModuleEvent();
                    }
                    else
                    {
                        DeactivateModuleEvent();
                    }
                }
            }
        }
        bool IActiveModule.IsActive { get => IsActive_; set => IsActive_ = value; }

        public void SelectModule(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= ParsedSelectedModules.Length)
                throw new ArgumentException("startModuleIndex must be index of input modules's array.");

            if (moduleIndex != CurrentModuleIndex_)
            {
                if (CurrentModule_ != null)
                {
                    UnsubscribeFromModuleEvents(CurrentModule_);
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = false;
                }
                CurrentModule_ = ParsedSelectedModules[moduleIndex];
                if (CurrentModule_ != null)
                {
                    SubscribeOnModulesEvents(CurrentModule_);
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = true;
                }
                CurrentModuleIndex_ = moduleIndex;

                SelectModuleEvent(CurrentModule_);
            }
        }
        protected abstract void SubscribeOnModulesEvents(TSelectedModule module);
        protected abstract void UnsubscribeFromModuleEvents(TSelectedModule module);

        //Unity API

        private void OnEnable()
        {
            if (!IsActive)
                enabled = true;
        }
        private void OnDisable()
        {
            if(IsActive)
                enabled = false;
        }
        private void Awake()
        {
            if (SelectedModules == null || SelectedModules.Length == 0)
                throw new ArgumentNullException("Haven't modules for selection.");
            if (CurrentModuleIndex < 0 || CurrentModuleIndex > SelectedModules.Length - 1)
                throw new ArgumentException("Haven't module by index = " + CurrentModuleIndex);

            ParsedSelectedModules=new TSelectedModule[SelectedModules.Length];
            for(int i = 0; i < SelectedModules.Length; i++)
            {
                if (SelectedModules[i] == null)
                    continue;

                ParsedSelectedModules[i] = SelectedModules[i] as TSelectedModule;
                if (ParsedSelectedModules[i] == null)
                    throw new ArgumentNullException
                        ($"Cant parsed {SelectedModules[i].name} to {typeof(TSelectedModule)}.");
            }
            SelectedModules = null;
            enabled = false;
            SelectModule(CurrentModuleIndex);
        }
    }
}
