using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Maths.Numbers
{
    [CreateAssetMenu(fileName = "AdvancedFloatAsset", menuName = "Raccoons/Maths/AdvancedFloatAsset")]
    public class AdvancedFloatAsset : ScriptableObject, ICollection<FloatModificator>
    {
        [SerializeField]
        private AdvancedFloat advancedFloat;

        [SerializeField]
        private AdvancedFloat.LoggingFlags loggingFlags = AdvancedFloat.LoggingFlags.None;

        [SerializeField]
        private List<FloatModificator> modificators;

        [SerializeField]
        private float value;


        public AdvancedFloat AdvancedFloat { get => advancedFloat; }

        public int Count => ((ICollection<FloatModificator>)modificators).Count;

        public bool IsReadOnly => ((ICollection<FloatModificator>)modificators).IsReadOnly;

        private void OnEnable()
        {
            if (AdvancedFloat == null)
            {
                advancedFloat = new AdvancedFloat();
            }
            advancedFloat.SetLoggingSettings(name, loggingFlags);
            AdvancedFloat.OnValueChanged += AdvancedFloat_OnValueChanged;
        }

        private void AdvancedFloat_OnValueChanged(object sender, float e)
        {
            value = e;
        }

        private void OnValidate()
        {
            advancedFloat.SetLoggingSettings(name, loggingFlags);
            AdvancedFloat.RemoveAllModificators();
            modificators.ForEach(modificator => AdvancedFloat.AddModificator(modificator, false));
            AdvancedFloat.Recalculate();
        }

        public void Add(FloatModificator item)
        {
            ((ICollection<FloatModificator>)modificators).Add(item);
        }

        public void Clear()
        {
            ((ICollection<FloatModificator>)modificators).Clear();
        }

        public bool Contains(FloatModificator item)
        {
            return ((ICollection<FloatModificator>)modificators).Contains(item);
        }

        public void CopyTo(FloatModificator[] array, int arrayIndex)
        {
            ((ICollection<FloatModificator>)modificators).CopyTo(array, arrayIndex);
        }

        public bool Remove(FloatModificator item)
        {
            return ((ICollection<FloatModificator>)modificators).Remove(item);
        }

        public IEnumerator<FloatModificator> GetEnumerator()
        {
            return ((IEnumerable<FloatModificator>)modificators).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)modificators).GetEnumerator();
        }
    }
}