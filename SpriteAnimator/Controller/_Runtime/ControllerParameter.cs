using System;
using UnityEngine;
using UnitySimplified.VariableReferences;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerParameter : IControllerIdentifiable
    {
        [SerializeField, HideInInspector]
        private string _identifier;
        [SerializeField]
        private KeywordReference _nameKeyword;
        [SerializeField, SerializeReference]
        private ParameterReference _parameterReference;

        public string Name => _nameKeyword;
        public KeywordReference NameKeyword => _nameKeyword;

        public Type ParameterType => _parameterReference.Type;
        public Type ParameterValueType => _parameterReference.ValueType;
        public object ParameterValue => _parameterReference.Value;


        internal ControllerParameter(SpriteAnimatorController controller, Type parameterType)
        {
            var parameter = (Parameter)Activator.CreateInstance(parameterType, new KeywordReference("NULL"));
            if (parameter == null)
                throw new NullReferenceException();
            
            _identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            _nameKeyword = new KeywordReference($"New {parameterType.Name}");
            _parameterReference = parameter.GetReference();
            _parameterReference.Value = parameter.LhsDefaultValue;
        }



        public string GetIdentifier() => _identifier;
    }
}