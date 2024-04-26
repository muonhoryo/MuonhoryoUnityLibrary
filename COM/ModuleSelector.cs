
using System;

namespace MuonhoryoLibrary.Unity.COM
{
    public abstract class ModuleSelector<TSelectedModule> : IActiveModule
    {
        public event Action DeactivateModuleEvent = delegate { };
        public event Action ActivateModuleEvent = delegate { };
        public event Action<TSelectedModule> SelectModuleEvent = delegate { };

        private ModuleSelector() { }
        public ModuleSelector(TSelectedModule[] SelectedModules, int startModuleIndex = 0)
        {
            if (SelectedModules == null || SelectedModules.Length == 0)
                throw new Exception("Modules's array is empty.");

            this.SelectedModules = SelectedModules;
            SelectModule(startModuleIndex);
        }

        private TSelectedModule[] SelectedModules;
        public int CurrentModuleIndex { get; private set; } = -1;
        public TSelectedModule CurrentModule_ { get; private set; }
        private bool IsActive = false;

        public bool IsActive_
        {
            get => IsActive;
            set
            {
                if (IsActive != value)
                {
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = value;
                    IsActive = value;
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
            if (moduleIndex < 0 || moduleIndex >= SelectedModules.Length)
                throw new ArgumentException("startModuleIndex must be index of input modules's array.");

            if (moduleIndex != CurrentModuleIndex)
            {
                if (CurrentModule_ != null)
                {
                    UnsubscribeFromModuleEvents(CurrentModule_);
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = false;
                }
                CurrentModule_ = SelectedModules[moduleIndex];
                if (CurrentModule_ != null)
                {
                    SubscribeOnModulesEvents(CurrentModule_);
                    if (CurrentModule_ is IActiveModule module)
                        module.IsActive = true;
                }

                SelectModuleEvent(CurrentModule_);
            }
        }
        protected abstract void SubscribeOnModulesEvents(TSelectedModule module);
        protected abstract void UnsubscribeFromModuleEvents(TSelectedModule module);
    }
}
