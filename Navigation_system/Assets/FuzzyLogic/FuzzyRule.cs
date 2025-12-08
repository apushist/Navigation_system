using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic
{
	public class FuzzyRule
	{
		public string Name; 
        
		public struct Antecedent
		{
			public string VariableName;
			public string SetName;
		}

		public List<Antecedent> Antecedents = new List<Antecedent>();
		public string ConsequentSetName;
		public float Weight = 1f;

		public FuzzyRule() { }

		// Обновленный конструктор
		public FuzzyRule(string name, string consequentSetName, float weight = 1f)
		{
			Name = name;
			ConsequentSetName = consequentSetName;
			Weight = Mathf.Clamp01(weight);
		}

		public FuzzyRule AddAntecedent(string variableName, string setName)
		{
			Antecedents.Add(new Antecedent() { VariableName = variableName, SetName = setName });
			return this;
		}

		public float Evaluate(Dictionary<string, Dictionary<string, float>> memberships)
		{
			if (Antecedents.Count == 0) return 0f;
			float min = 1f;
			foreach (var a in Antecedents)
			{
				if (!memberships.TryGetValue(a.VariableName, out var sets))
				{
					min = 0f;
					break;
				}
				if (!sets.TryGetValue(a.SetName, out float mem))
				{
					mem = 0f;
				}
				min = Mathf.Min(min, mem);
				if (min <= 0f) break;
			}
			return Mathf.Clamp01(min * Weight);
		}
	}
}