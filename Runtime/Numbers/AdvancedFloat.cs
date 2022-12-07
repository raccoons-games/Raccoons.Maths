using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Raccoons.Maths.Numbers
{
    [System.Serializable]
    public class AdvancedFloat
    {
        [Flags]
        public enum LoggingFlags
        {
            None = 0,
            Modificators = 1,
            Value = 2,
            InitialValue = 4
        }

        private const int MAX_MODIFICATOR_ORDER = 9;

        [SerializeField]
        private float initialValue;

        [SerializeField]
        private bool addModificatorOnceOnly = true;

        private List<FloatModificator>[] _groupedModificators = new List<FloatModificator>[MAX_MODIFICATOR_ORDER];
        private List<FloatModificator> _earlyModificators = new List<FloatModificator>();
        private List<FloatModificator> _lateModificators = new List<FloatModificator>();

        private float _cachedValue = float.NaN;
        private bool _earlyModificatorsNeedSort = false;
        private bool _lateModificatorsNeedSort = false;

        private string _loggingTag;
        private LoggingFlags _logging;

        public AdvancedFloat()
        {

        }

        public AdvancedFloat(float initialValue, bool addModificatorOnceOnly = true)
        {
            this.initialValue = initialValue;
            AddModificatorOnceOnly = addModificatorOnceOnly;
        }

        public event EventHandler<float> OnValueChanged;
        public event EventHandler<FloatModificator> OnModificatorAdded;
        public event EventHandler<FloatModificator> OnModificatorRemoved;

        public bool NeedsRecalculation { get; private set; } = false;
        public float Value => float.IsNaN(_cachedValue) ? InitialValue : _cachedValue;
        public float InitialValue { get => initialValue; }
        public bool AddModificatorOnceOnly { get => addModificatorOnceOnly; set => addModificatorOnceOnly = value; }
        public string LoggingTag { get => string.IsNullOrEmpty(_loggingTag)?"":$"[{_loggingTag}]"; }
        public LoggingFlags Logging { get => _logging; }

        public IEnumerable<FloatModificator> GetAllOrderedModificators()
        {
            var result = new List<FloatModificator>();
            System.Array.ForEach(_groupedModificators, group => result.AddRange(group));
            return result;
        }

        public IEnumerable<FloatModificator> GetEarlyModificators()
        {
            return _earlyModificators;
        }

        public IEnumerable<FloatModificator> GetLateModificators()
        {
            return _lateModificators;
        }

        public IEnumerable<FloatModificator> GetAllModificators()
        {
            return new List<FloatModificator>().Concat(GetEarlyModificators()).Concat(GetAllOrderedModificators()).Concat(GetEarlyModificators());
        }

        public IEnumerable<FloatModificator> GetOrderedModificators(int order)
        {
            return _groupedModificators[order];
        }

        private void SetDirty(bool willRecalculate)
        {
            NeedsRecalculation = true;
            if (willRecalculate)
            {
                Recalculate();
            }
        }

        public void SetInitialValue(float newValue, bool autoRecalculate = true)
        {
            initialValue = newValue;
            LogInitialValue();
            SetDirty(autoRecalculate);
        }
        
        private bool IsAllowedToAdd(IEnumerable<FloatModificator> group, FloatModificator floatModificator)
        {
            return !AddModificatorOnceOnly || !group.Contains(floatModificator);
        }

        public void AddModificator(FloatModificator modificator, bool autoRecalculate = true)
        {
            if (modificator.Order >= 0 && modificator.Order <= MAX_MODIFICATOR_ORDER)
            {
                if (_groupedModificators[modificator.Order] == null)
                {
                    _groupedModificators[modificator.Order] = new List<FloatModificator>();
                }
                List<FloatModificator> modificatorGroup = _groupedModificators[modificator.Order];
                if (IsAllowedToAdd(modificatorGroup, modificator))
                {
                    modificatorGroup.Add(modificator);
                }
                else
                {
                    return;
                }
            }
            else if (modificator.Order < 0)
            {
                if (IsAllowedToAdd(_earlyModificators, modificator))
                {
                    _earlyModificators.Add(modificator);
                    _earlyModificatorsNeedSort = true;
                }
                else
                {
                    return;
                }
            }
            else if (modificator.Order > MAX_MODIFICATOR_ORDER)
            {
                if (IsAllowedToAdd(_lateModificators, modificator))
                {
                    _lateModificators.Add(modificator);
                    _lateModificatorsNeedSort = true;
                }
                else
                {
                    return;
                }
            }
            LogModificatorAction(modificator, "added");
            OnModificatorAdded?.Invoke(this, modificator);
            SetDirty(autoRecalculate);
        }

        public void RemoveModificator(FloatModificator modificator, bool autoRecalculate = true)
        {
            if (modificator.Order >= 0 && modificator.Order <= MAX_MODIFICATOR_ORDER)
            {
                List<FloatModificator> modificatorGroup = _groupedModificators[modificator.Order];
                if (modificatorGroup == null || !modificatorGroup.Contains(modificator))
                {
                    return;
                }
                modificatorGroup.Remove(modificator);

            }
            else if (modificator.Order < 0)
            {
                if (_earlyModificators.Contains(modificator))
                {
                    _earlyModificators.Remove(modificator);
                }
                else
                {
                    return;
                }
            }
            else if (modificator.Order > MAX_MODIFICATOR_ORDER)
            {
                if (_lateModificators.Contains(modificator))
                {
                    _lateModificators.Remove(modificator);
                }
                else
                {
                    return;
                }
            }
            LogModificatorAction(modificator, "removed");
            OnModificatorRemoved?.Invoke(this, modificator);
            SetDirty(autoRecalculate);
        }

        public void Recalculate()
        {
            if (!NeedsRecalculation) return;
            if (_earlyModificatorsNeedSort)
            {
                _earlyModificators.Sort(FloatModificator.OrderComparison);
                _earlyModificatorsNeedSort = false;
            }
            if (_lateModificatorsNeedSort)
            {
                _lateModificators.Sort(FloatModificator.OrderComparison);
                _lateModificatorsNeedSort = false;
            }
            float newValue = InitialValue;
            _earlyModificators.ForEach(modificator =>
            {
                newValue = FloatModificator.ApplyModificators(newValue, modificator);
            });

            System.Array.ForEach(_groupedModificators, modificatorGroup =>
            {
                if (modificatorGroup!= null)
                {
                    newValue = FloatModificator.ApplyModificators(newValue, modificatorGroup);
                } 
            });

            _lateModificators.ForEach(modificator =>
            {
                newValue = FloatModificator.ApplyModificators(newValue, modificator);
            });

            NeedsRecalculation = false;
            if (Value != newValue)
            {
                _cachedValue = newValue;
                LogValue();
                OnValueChanged?.Invoke(this, Value);
            }
        }

        public void RemoveAllModificators(bool autoRecalculate = true)
        {
            _groupedModificators = new List<FloatModificator>[MAX_MODIFICATOR_ORDER + 1];
            _earlyModificators = new List<FloatModificator>();
            _lateModificators = new List<FloatModificator>();
            SetDirty(autoRecalculate);
        }

        public void SetLoggingSettings(string loggingTag, LoggingFlags loggingFlags)
        {
            _loggingTag = loggingTag;
            _logging = loggingFlags;
        }

        private void LogInfo(string message)
        {
            Debug.Log($"[{GetType().Name}]{LoggingTag} {message}");
        }

        private void LogModificatorAction(FloatModificator floatModificator, string action)
        {
            if (!_logging.HasFlag(LoggingFlags.Modificators)) return;
            StringBuilder stringBuilder = new StringBuilder("Modificator ")
                .Append(action)
                .Append(": ")
                .Append(floatModificator.ToString());
            LogInfo(stringBuilder.ToString());
        }

        private void LogValue()
        {
            if (!_logging.HasFlag(LoggingFlags.Value)) return;
            LogInfo($"Value: {Value}");
        }

        private void LogInitialValue()
        {
            if (!_logging.HasFlag(LoggingFlags.InitialValue)) return;
            LogInfo($"Initial Value: {InitialValue}");
        }
    }
}
