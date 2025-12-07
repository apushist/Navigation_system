using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic.Data
{
    [System.Serializable]
    public class FuzzySetDef
    {
        public string Name = "NewSet";
        // Using AnimationCurve is much more flexible than the 4-point trapezoid
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 1), new Keyframe(0.75f, 1), new Keyframe(1, 0));
    }

    [System.Serializable]
    public class FuzzyVariableDef
    {
        public string Name = "NewVariable";
        public float Min = 0f;
        public float Max = 1f;
        public List<FuzzySetDef> Sets = new List<FuzzySetDef>();
    }

    [System.Serializable]
    public class FuzzyAntecedentDef
    {
        public string VariableName;
        public string SetName;
    }

    [System.Serializable]
    public class FuzzyRuleDef
    {
        public List<FuzzyAntecedentDef> Antecedents = new List<FuzzyAntecedentDef>();
        public string ConsequentSetName;
        public float Weight = 1f;
    }

    [CreateAssetMenu(fileName = "FuzzyInferenceSystem", menuName = "Fuzzy Logic/Inference System")]
    public class FuzzyInferenceSystemSO : ScriptableObject
    {
        public List<FuzzyVariableDef> InputVariables = new List<FuzzyVariableDef>();
        public FuzzyVariableDef OutputVariable = new FuzzyVariableDef();
        public List<FuzzyRuleDef> Rules = new List<FuzzyRuleDef>();
    }
}
