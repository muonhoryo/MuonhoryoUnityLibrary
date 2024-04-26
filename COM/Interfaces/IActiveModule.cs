
using System;

namespace MuonhoryoLibrary.Unity.COM
{
    public interface IActiveModule
    {
        event Action DeactivateModuleEvent;
        event Action ActivateModuleEvent;
        bool IsActive { get; set; }
    }
}
