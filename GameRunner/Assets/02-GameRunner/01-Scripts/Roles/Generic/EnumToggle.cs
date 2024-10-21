using System;
using System.Collections.Generic;
using MathBuddy.Flags;
using UnityEngine;

public class EnumToggle<T> : ICheckPrivilege where T : Enum
{
    /// <summary>
    /// Local value, not value of all checks
    /// </summary>
    public bool HasPrivilege { get { return _localPrivilige; } }
    
    /// <summary>
    /// Have all checks been passed?
    /// </summary>
    public bool Value { get { return _value; } }
    
    public Action<bool> onValueChanged;
        
    protected T _required;
    private FlagExtensions.FlagPositive _checkFor;
    private FlagExtensions.CheckType _check;
    
    //tracks local check value only
    private bool _localPrivilige = true;
    //tracks all checks value
    private bool _value = true;
    private bool _checkMultiple;
    private int _id;

    /// <summary>
    /// Toggles on/off based on the values of the (flags) enums that are passed through the change events.
    /// </summary>
    /// <param name="required">Required value for the check to pass.</param>
    /// <param name="checkFor">Should the enum's value include or exclude the required value to pass?</param>
    /// <param name="check">Type of check to apply before determining whether the check was passed.</param>
    /// <param name="valueChangedEvent">reference to the on changed action so this class can subscribe to it.</param>
    /// <param name="attachedTo">Gameobject, so Id can be used to check if other checks have passed in the case of multiple checks on one object.</param>
    public EnumToggle(T required, FlagExtensions.FlagPositive checkFor, FlagExtensions.CheckType check, ref Action<T> valueChangedEvent, GameObject attachedTo = null)
    {
        _required = required;
        _checkFor = checkFor;
        _check = check;
        valueChangedEvent += CheckValue;

        if (attachedTo != null) {
            //if object bound, check all attached checks, otherwise only check this one type
            //multiple checks only pass if all attached objects actually pass
            _checkMultiple = true;
            _id = attachedTo.GetInstanceID();
            
            if (!ICheckPrivilege.OBJECT_CHECKS.ContainsKey(_id)) {
                ICheckPrivilege.OBJECT_CHECKS.Add(_id, new List<ICheckPrivilege>());    
            }
            ICheckPrivilege.OBJECT_CHECKS[_id].Add(this);
        }
    }

    /// <summary>
    /// Toggles on/off based on the values of the (flags) enums that are passed through the change events.
    /// </summary>
    /// <param name="required">Required value for the check to pass.</param>
    /// <param name="checkFor">Should the enum's value include or exclude the required value to pass?</param>
    /// <param name="check">Type of check to apply before determining whether the check was passed.</param>
    /// <param name="attachedTo">Gameobject, so Id can be used to check if other checks have passed in the case of multiple checks on one object.</param>
    public EnumToggle(T required, FlagExtensions.FlagPositive checkFor, FlagExtensions.CheckType check, GameObject attachedTo = null)
    {
        _required = required;
        _checkFor = checkFor;
        _check = check;

        if (attachedTo != null) {
            //if object bound, check all attached checks, otherwise only check this one type
            //multiple checks only pass if all attached objects actually pass
            _checkMultiple = true;
            _id = attachedTo.GetInstanceID();
            
            if (!ICheckPrivilege.OBJECT_CHECKS.ContainsKey(_id)) {
                ICheckPrivilege.OBJECT_CHECKS.Add(_id, new List<ICheckPrivilege>());    
            }
            ICheckPrivilege.OBJECT_CHECKS[_id].Add(this);
        }
    }

    /// <summary>
    /// Called when flags changed and requirement was met or not met.
    /// </summary>
    /// <param name="hasRequired">new value has required enum.</param>
    protected virtual void ChangeValue(bool hasRequired)
    {
        onValueChanged?.Invoke(hasRequired);
    }
    
    /// <summary>
    /// Check value of T and if matches checktype, call ChangeVisual
    /// </summary>
    /// <param name="value">value which to check against the requirement.</param>
    protected void CheckValue(T value)
    {
        CheckValue(value, false);
    }
    
    /// <summary>
    /// Check value of T and if matches checktype, call ChangeVisual
    /// </summary>
    /// <param name="value">value which to check against the requirement.</param>
    /// <param name="alwaysInvoke">invoke onChange even if value is the same (use for initial check).</param>
    // ReSharper disable once MethodOverloadWithOptionalParameter (used when return value is taken).
    public virtual bool CheckValue(T value, bool alwaysInvoke = false)
    {
        //convert into enum value
        int req = (int) (object) _required;
        int val = (int) (object) value;
        bool result = false;

        switch (_check) {
            case FlagExtensions.CheckType.All:
                if (_checkFor == FlagExtensions.FlagPositive.Contains) {
                    //check if the whole of req is contained in val (val can have more flags)
                    result = (req & val) == req;
                } else if (_checkFor == FlagExtensions.FlagPositive.DoesNotContains) {
                    //check if none of req is contained in val (val can have other flags)
                    result = (req & val) == 0;
                }
                else {
                    throw new Exception($"Missing positive flag {_checkFor}");
                }
                break;
            case FlagExtensions.CheckType.Any:
                if (_checkFor == FlagExtensions.FlagPositive.Contains) {
                    //check if any of req is contained in val (val can have more flags)
                    result = (req & val) != 0;
                } else if (_checkFor == FlagExtensions.FlagPositive.DoesNotContains) {
                    //check if any of req isn't contained in val (val can have other flags)
                    result = (req & val) != req;
                }
                else {
                    throw new Exception($"Missing positive flag {_checkFor}");
                }
                
                break;
            default:
                throw new Exception($"Missing checktype ({_check})");
        }

        if (!alwaysInvoke && _localPrivilige == result)
            return result;
        _localPrivilige = result;
        
        if (!result) {
            _value = false;
            ChangeValue(false);
            return false;
        }

        if (_checkMultiple) {
            foreach (ICheckPrivilege check in ICheckPrivilege.OBJECT_CHECKS[_id]) {
                //if any of the object's visiblity checks has not yet been met, disable object
                if (!check.HasPrivilege) {
                    _value = false;
                    ChangeValue(false);
                    return false;
                }
            }
        }

        _value = true;
        ChangeValue(true);
        return true;
    }
}
