using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerParameter : IControllerIdentifiable
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_identifier")]
        private string identifier;
        [SerializeField]
        [FormerlySerializedAs("_nameKeyword")]
        private KeywordReference nameKeyword;
        [SerializeField, SerializeReference]
        [FormerlySerializedAs("_parameterReference")]
        private ParameterReference parameterReference;

        internal ControllerParameter(SpriteAnimatorController controller, Type parameterType)
        {
            var parameter = (Parameter)Activator.CreateInstance(parameterType, new KeywordReference("NULL")) ?? throw new NullReferenceException();
            identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            nameKeyword = new KeywordReference($"New {parameterType.Name}");
            parameterReference = parameter.GetReference();
            parameterReference.Value = parameter.LhsDefaultValue;
        }

        public string Name => nameKeyword;
        public KeywordReference NameKeyword => nameKeyword;
        public Type ParameterType => parameterReference.Type;
        public Type ParameterValueType => parameterReference.ValueType;
        public object ParameterValue => parameterReference.Value;

        public string GetIdentifier() => identifier;
    }
}