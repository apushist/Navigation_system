using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic.Data
{
	[System.Serializable]
	public class FuzzySetDef
	{
		public string Name = "NewSet";
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
		public string Name = "Rule Name"; 
		public List<FuzzyAntecedentDef> Antecedents = new List<FuzzyAntecedentDef>();
		public string ConsequentSetName;
		[Range(0,1)] public float Weight = 1f;
	}

	// <--- Новый класс для группы
	[System.Serializable]
	public class FuzzyRuleGroupDef
	{
		public string GroupName = "New Group";
		public List<FuzzyRuleDef> Rules = new List<FuzzyRuleDef>();
	}

	[CreateAssetMenu(fileName = "FuzzyInferenceSystem", menuName = "Fuzzy Logic/Inference System")]
	public class FuzzyInferenceSystemSO : ScriptableObject
	{
		public List<FuzzyVariableDef> InputVariables = new List<FuzzyVariableDef>();
		public FuzzyVariableDef OutputVariable = new FuzzyVariableDef();
        
		public List<FuzzyRuleGroupDef> RuleGroups = new List<FuzzyRuleGroupDef>();
	}
}