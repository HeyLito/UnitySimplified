using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified
{
    [Serializable]
    public sealed partial class VisualStatement
    {
        public enum LogicalOperator
        {
            AND,
            OR
        }

        #region FIELDS
        [SerializeField] private List<Condition> conditions = new List<Condition>();
        [SerializeField] private List<LogicalOperator> logicalOperators = new List<LogicalOperator>();
        #endregion

        #region PROPERTIES
        public int ConditionsCount => conditions.Count;
        public int OperatorsCount => logicalOperators.Count;
        public int Count => ConditionsCount + OperatorsCount;
        #endregion

        #region METHODS
        private bool DoIsValid(out int code, out string message)
        {
            code = -1;
            message = "";

            if (logicalOperators.Count != Mathf.Max(conditions.Count - 1, 0))
            {
                code = 0;
                message = $"Found conflicting difference between logical operators and conditions";
            }
            for (int i = 0; i < conditions.Count; i++) 
            {
                if (!conditions[i].DoIsValid(out int _, out string cMessage))
                {
                    code = 1;
                    message = $"Condition at {i} is not valid. {cMessage}";
                    return false;
                }
            }

            return true;
        }

        public bool IsValid()
        {   return DoIsValid(out _, out _);   }
        public bool GetResult()
        {
            if (!DoIsValid(out int code, out string message))
                throw new Exception($"Error {code}: {message}");

            if (conditions.Count == 1) 
                return conditions[0].DoGetResult();
            else if (conditions.Count > 1)
            {
                bool previousAndResult = false, previousOrResult = false;

                for (int i = 0; i < logicalOperators.Count; i++)
                {
                    switch (logicalOperators[i])
                    {
                        case LogicalOperator.AND:
                            if (i == 0)
                                previousAndResult = conditions[0].DoGetResult() && conditions[i + 1].DoGetResult();
                            else previousAndResult = previousAndResult && conditions[i + 1].GetResult();
                            break;

                        case LogicalOperator.OR:
                            if (i == 0)
                                previousOrResult = conditions[0].DoGetResult() || conditions[i + 1].DoGetResult();
                            else if (previousAndResult || previousOrResult)
                                return true;
                            else previousOrResult = previousOrResult || conditions[i + 1].DoGetResult();
                            break;
                    }
                }
                return previousAndResult || previousOrResult;
            }
            return false;
        }
        #endregion
    }
}