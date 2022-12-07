using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raccoons.Maths.Numbers
{
    [System.Serializable]
    public class FloatModificator
    {
        [SerializeField]
        private FloatModificatorOperation modificatorType;

        [SerializeField] 
        private int order;

        [SerializeField]
        private float value;

        public FloatModificator(FloatModificatorOperation modificatorType, float value, int order)
        {
            this.modificatorType = modificatorType;
            this.value = value;
            this.order = order;
        }

        public FloatModificatorOperation Type { get => modificatorType; }
        public float Value { get => value; }
        public int Order { get => order; }

        public static float ApplyModificators(float currentValue, params FloatModificator[] modificators)
        {
            return ApplyModificators(currentValue, modificators.AsEnumerable());
        }

        public static float ApplyModificators(float currentValue, IEnumerable<FloatModificator> modificators)
        {
            float multiplier = 0;
            float addition = 0;
            foreach (var modificator in modificators)
            {
                switch (modificator.Type)
                {
                    case FloatModificatorOperation.Add:
                        addition += modificator.Value;
                        break;
                    case FloatModificatorOperation.Multiply:
                        multiplier += modificator.Value;
                        break;
                }
            }
            return currentValue + currentValue * multiplier + addition;
        }

        public static int OrderComparison(FloatModificator value1, FloatModificator value2)
        {
            if (value1.Order > value2.Order)
            {
                return 1;
            }
            else if (value1.Order == value2.Order)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static int InverseOrderComparison(FloatModificator value1, FloatModificator value2)
        {
            return OrderComparison(value2, value1);
        }

        public FloatModificator Clone()
        {
            return new FloatModificator(modificatorType, value, order);
        }

        public override string ToString()
        {
            return $"Order {Order} {Type} {Value}";
        }
    }
}