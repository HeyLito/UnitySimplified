using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerCondition
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_parameterIdentifier")]
        private string parameterIdentifier = "";
        [SerializeField, SerializeReference]
        [FormerlySerializedAs("_parameterReference")]
        private ParameterReference parameterReference;
        [SerializeField]
        [FormerlySerializedAs("_parameterComparer")]
        private ParameterComparer parameterComparer;

        internal ControllerCondition(ControllerParameter controllerParameter)
        {
            var parameter = (Parameter)Activator.CreateInstance(controllerParameter.ParameterType, new KeywordReference("NULL"));
            if (parameter == null)
                return;
            parameterIdentifier = controllerParameter.GetIdentifier();
            parameterReference = parameter.GetReference();
            parameterReference.Value = parameter.RhsDefaultValue;
            parameterComparer = parameter.Comparer;
        }

        public bool TryGetCondition(SpriteAnimatorController controller, out AnimationCondition condition)
        {
            condition = null;
            if (string.IsNullOrEmpty(parameterIdentifier) || parameterComparer == null || parameterReference == null)
                return false;
            if (!controller.TryGetParameterFromIdentifier(parameterIdentifier, out var controllerParameter))
                return false;
            if (controllerParameter.ParameterValueType != parameterReference.ValueType)
                return false;

            BindingFlags parameterConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var parameterType = controllerParameter.ParameterType;
            var parameterNameKeyword = controllerParameter.NameKeyword;
            var parameterLhsValue = controllerParameter.ParameterValue;
            var parameterRhsValue = parameterReference.Value;
            ConstructorInfo parameterConstructorInfo = null;
            foreach (var constructorInfo in parameterType.GetConstructors(parameterConstructorFlags))
            {
                var constructorParameters = constructorInfo.GetParameters();
                if (constructorParameters.Length != 3 ||
                    constructorParameters[0].ParameterType != parameterNameKeyword.GetType() ||
                    constructorParameters[1].ParameterType != parameterLhsValue.GetType() ||
                    constructorParameters[2].ParameterType != parameterRhsValue.GetType())
                    continue;
                parameterConstructorInfo = constructorInfo;
                break;
            }

            if (parameterConstructorInfo == null)
                throw new MissingMemberException($"{parameterType} does not contain a constructor with params [{parameterNameKeyword.GetType()}, {parameterLhsValue.GetType()}, {parameterRhsValue.GetType()}]");

            condition = Activator.CreateInstance(parameterType, parameterConstructorFlags, null, new object[] { parameterNameKeyword, parameterLhsValue, parameterRhsValue }, null) as Parameter;
            return true;

        }
    }
}