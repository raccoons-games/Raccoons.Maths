# Raccoons Maths

### AdvancedFloat

Class that implement advanced float math with modificators and multipliers. It is Serializable, you can put it as inspector field.

**Methods**:

- void AddModificator(FloatModificator modificator, bool autoRecalculate) - adds modificator to AdvancedFloat. autoRecalculate defines should AF recalculate Value after that.
- void RemoveModificator(FloatModificator modificator, bool autoRecalculate) - adds modificator to AdvancedFloat. autoRecalculate defines should AF recalculate Value after that.
- void RemoveAllModificators(bool autoRecalculate) - removes all modificators. autoRecalculate works as always.
- void SetInitialValue(bool autoRecalculate) - sets initial float value for AF.
- void Recalculate() - if NeedsRecalculation, it recalculates Value.
- IEnumerable<FloatModificator> GetXXXXModificators() - various methods to get modificator collections
- void SetLoggingSettings(string loggingTag, LoggingFlags loggingFlags) - apply logging tag and flags for logging AF changes

**Properties**

- float InitialValue - initial AF value. Read-only, use SetInitialValue.
- float Value - calculation result after applying all modificators to InitialValue.
- bool NeedsRecalculation - true, if you performed any action with autoRecalculate=false. if false, Value is not relevant and you should call Recalculate() manually.
- bool AddModificatorOnceOnly - if true, you can add any specific modificator only once. Any other attempts will be ignored until you remove it and try again. If false - you can add this one multiple times.

**Events**

- EventHandler<float> OnValueChanged - any recalculation calls the event with newValue parameter.
- EventHandler<FloatModificator> OnModificatorAdded - when you successfully added modificator.
- EventHandler<FloatModificator> OnModificatorRemoved - when you successfully removed modificator

### FloatModificator

Class for number-modificators. It is Serializable, you can put it as inspector field.

**Methods**

- FloatModificator Clone() - clones modificator

**Properties**

- Type - modification operation. Add - adds Value, Multiply - multiplies value.
- Order - modificator group or order. If you set any value from 0 to 9 - it groups it with another modifactor in this order (example: Multiply modificators 0.3 and 0.2 in the group 2 become 0.5, so on this step the value will be increased on 50%). if you set it as -1, -100, 10, 20000, it doesn't group with any other modificators. (example, 0.3 and 0.5 with order 11 become adds 30% first, and adds 50% to the result of previous operation). If there are two ungrouped modificators with the same order, than first applying modificator is the one who was added first. Otherwise - the smaller order is - the faster it applies.
- Value - value of modificator

### AdvancedFloatAsset

ScriptableObject with path Raccoons/Maths/AdvancedFloatAsset. Use it for testing AdvancedFloat logic, it updates Value in inspector in real-time when you add modificators there.
