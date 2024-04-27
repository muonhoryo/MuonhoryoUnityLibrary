
using System;
using UnityEngine;
using MuonhoryoLibrary.Collections;

namespace MuonhoryoLibrary.Unity
{
    [Serializable]
    public sealed class CompositeVector3 : CompositeParameter<Vector3>
    {
        public CompositeVector3(Vector3 DefaultValue) : base(DefaultValue) { }

        public event Action<IConstModifier<Vector3>> AddingAddModifierEvent;
        public event Action<IConstModifier<float>> AddingMultiplyModifierEvent;

        private readonly SingleLinkedList<ModifierHandler<Vector3>> AddersList =
            new SingleLinkedList<ModifierHandler<Vector3>>() { };
        private readonly SingleLinkedList<ModifierHandler<float>> MultipliesList =
            new SingleLinkedList<ModifierHandler<float>>() { };

        protected override void RecalculationAction()
        {
            CurrentValue = DefaultValue;
            foreach (var item in AddersList)
            {
                CurrentValue += item.Modifier;
            }
            foreach (var item in MultipliesList)
            {
                CurrentValue *= item.Modifier;
            }
        }
        public IConstModifier<Vector3> AddModifier_Add(Vector3 value)
        {
            return AddModifier(this, value, AddersList, (item) => AddingAddModifierEvent?.Invoke(item));
        }
        public IConstModifier<float> AddModifier_Multiply(float value)
        {
            return AddModifier(this, value, MultipliesList, (item) => AddingMultiplyModifierEvent?.Invoke(item));
        }
    }
}
