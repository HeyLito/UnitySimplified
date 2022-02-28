using System;
using System.Linq;
using UnityEngine;

namespace UnitySimplified 
{
    public sealed partial class VisualStatement
    {
        [Serializable]
        public sealed class Condition
        {
            public enum RelationalOperator
            {
                EqualTo = default,
                NotEqualTo,
                GreaterThan,
                GreaterThanOrEqualTo,
                LessThan,
                LessThanOrEqualTo,
            }

            #region FIELDS
            [SerializeField] private Operand lhs = new Operand(Operand.ReferenceType.Field);
            [SerializeField] private RelationalOperator relationalOperator;
            [SerializeField] private Operand rhs = new Operand(Operand.ReferenceType.Value);
            #endregion

            #region METHODS
            internal bool DoIsValid(out int code, out string message)
            {
                code = -1;
                message = "";

                if (lhs == null)
                {
                    code = 0;
                    message = "Left operand is NULL";
                    return false;
                }
                if (rhs == null)
                {
                    code = 0;
                    message = "Right operand is NULL";
                    return false;
                }

                if (!lhs.DoIsValid(out int lCode, out string lMessage) | !rhs.DoIsValid(out int rCode, out string rMessage))
                {
                    code = 1;
                    if (lCode != -1)
                        message = lMessage;
                    else if (rCode != -1)
                        message = rMessage;
                    return false;
                }

                if (lhs.ValueType != rhs.ValueType)
                {
                    code = 2;
                    message = "Found type-mismatch between left and right value types";
                    return false;
                }

                if (!AcceptsOperator(relationalOperator))
                {
                    code = 3;
                    message = $"Condition does not accept operator {relationalOperator}";
                    return false;
                }

                return true;
            }
            internal bool DoGetResult()
            {
                switch (relationalOperator)
                {
                    case RelationalOperator.EqualTo:
                        if (lhs.DoGetResult().Equals(rhs.DoGetResult()))
                            return true;
                        return false;

                    case RelationalOperator.NotEqualTo:
                        if (!lhs.DoGetResult().Equals(rhs.DoGetResult()))
                            return true;
                        return false;

                    case RelationalOperator.GreaterThan:
                    case RelationalOperator.GreaterThanOrEqualTo:
                    case RelationalOperator.LessThan:
                    case RelationalOperator.LessThanOrEqualTo:
                        double leftNumeric = ((IConvertible)lhs.DoGetResult()).ToDouble(System.Globalization.CultureInfo.CurrentCulture);
                        double rightNumeric = ((IConvertible)rhs.DoGetResult()).ToDouble(System.Globalization.CultureInfo.CurrentCulture);
                        if (relationalOperator == RelationalOperator.GreaterThan)
                            if (leftNumeric > rightNumeric)
                                return true;
                        if (relationalOperator == RelationalOperator.GreaterThanOrEqualTo)
                            if (leftNumeric >= rightNumeric)
                                return true;
                        if (relationalOperator == RelationalOperator.LessThan)
                            if (leftNumeric < rightNumeric)
                                return true;
                        if (relationalOperator == RelationalOperator.LessThanOrEqualTo)
                            if (leftNumeric <= rightNumeric)
                                return true;
                        return false;

                    default:
                        return false;
                }
            }

            public bool IsValid()
            {   return DoIsValid(out _, out _);   }
            public bool GetResult()
            {
                if (!DoIsValid(out int code, out string message))
                    throw new Exception($"Error {code}: {message}");
                return DoGetResult();
            }
            public bool AcceptsOperator(RelationalOperator relationalOperator)
            {
                switch (relationalOperator)
                {
                    case RelationalOperator.GreaterThan:
                    case RelationalOperator.GreaterThanOrEqualTo:
                    case RelationalOperator.LessThan:
                    case RelationalOperator.LessThanOrEqualTo:
                        Type lhsType = lhs.ValueType, rhsType = rhs.ValueType;
                        if (!lhsType.IsNumericType() || !rhsType.IsNumericType())
                            return false;
                        if (!lhsType.GetInterfaces().Contains(typeof(IConvertible)) || !rhsType.GetInterfaces().Contains(typeof(IConvertible)))
                            return false;
                        break;
                }
                return true;
            }
            #endregion
        }
    }
}