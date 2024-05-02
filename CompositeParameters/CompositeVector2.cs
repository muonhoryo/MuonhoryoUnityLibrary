
using System;
using UnityEngine;
using MuonhoryoLibrary.Collections;

namespace MuonhoryoLibrary.Unity
{
    [Serializable]
    public sealed class CompositeVector2 : CompositeParameter<Vector2>
    {
        public CompositeVector2(Vector2 DefaultValue) : base(DefaultValue) { }

        public event Action<IConstModifier<Vector2>> AddingAddModifierEvent;
        public event Action<IConstModifier<float>> AddingMultiplyModifierEvent;

        private readonly SingleLinkedList<ModifierHandler<Vector2>> AddersList =
            new SingleLinkedList<ModifierHandler<Vector2>>() { };
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
        public IConstModifier<Vector2> AddModifier_Add(Vector2 value)
        {
            return AddModifier(this, value, AddersList, (item) => AddingAddModifierEvent?.Invoke(item));
        }
        public IConstModifier<float> AddModifier_Multiply(float value)
        {
            return AddModifier(this, value, MultipliesList, (item) => AddingMultiplyModifierEvent?.Invoke(item));
        }
    }
}
