using System;
using System.Reflection;
using UnityEngine;
using UnitySimplified.VariableReferences;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerCondition
    {
        [SerializeField, HideInInspector]
        private string _parameterIdentifier = "";
        [SerializeField, SerializeReference]
        private ParameterReference _parameterReference;
        [SerializeField]
        private ParameterComparer _parameterComparer;


        internal ControllerCondition(ControllerParameter controllerParameter)
        {
            var parameter = (Parameter)Activator.CreateInstance(controllerParameter.ParameterType, new KeywordReference("NULL"));
            if (parameter == null)
                return;
            _parameterIdentifier = controllerParameter.GetIdentifier();
            _parameterReference = parameter.GetReference();
            _parameterReference.Value = parameter.RhsDefaultValue;
            _parameterComparer = parameter.Comparer;
        }



        public bool TryGetCondition(SpriteAnimatorController controller, out AnimationCondition condition)
        {
            condition = null;
            if (!string.IsNullOrEmpty(_parameterIdentifier) && _parameterComparer != null && _parameterReference != null)
                if (controller.TryGetParameterFromIdentifier(_parameterIdentifier, out var controllerParameter))
                    if (controllerParameter.ParameterValueType == _parameterReference.ValueType)
                    {
                        BindingFlags parameterConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                        var parameterType = controllerParameter.ParameterType;
                        var parameterNameKeyword = controllerParameter.NameKeyword;
                        var parameterLhsValue = controllerParameter.ParameterValue;
                        var parameterRhsValue = _parameterReference.Value;
                        ConstructorInfo parameterConstructorInfo = null;
                        foreach (var constructorInfo in parameterType.GetConstructors(parameterConstructorFlags))
                        {
                            var constructorParameters = constructorInfo.GetParameters();
                            if (constructorParameters.Length == 3 &&
                                constructorParameters[0].ParameterType == parameterNameKeyword.GetType() &&
                                constructorParameters[1].ParameterType == parameterLhsValue.GetType() &&
                                constructorParameters[2].ParameterType == parameterRhsValue.GetType())
                            {
                                parameterConstructorInfo = constructorInfo;
                                break;
                            }
                        }
                        if (parameterConstructorInfo != null)
                        {
                            condition = Activator.CreateInstance(parameterType, parameterConstructorFlags, null, new object[] { parameterNameKeyword, parameterLhsValue, parameterRhsValue }, null) as Parameter;
                            return true;
                        }
                        else throw new MissingMemberException($"{parameterType} does not contain a constructor with params [{parameterNameKeyword.GetType()}, {parameterLhsValue.GetType()}, {parameterRhsValue.GetType()}]");
                    }
            return false;
        }
    }
}