using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic
{
    /// <summary>
    /// Структура: IF (Input1 is A) AND (Input2 is B) THEN (Output is C).
    /// Вычисляет "силу" срабатывания правила (обычно берется минимум из входных значений — Min(A, B)).
    ///
    /// weight: множитель силы правила (0..1)
    /// </summary>
    public class FuzzyRule
    {
        public struct Antecedent
        {
            public string VariableName;
            public string SetName;
        }

        public List<Antecedent> Antecedents = new List<Antecedent>();
        public string ConsequentSetName;
        public float Weight = 1f;

        public FuzzyRule() { }

        public FuzzyRule(string consequentSetName, float weight = 1f)
        {
            ConsequentSetName = consequentSetName;
            Weight = Mathf.Clamp01(weight);
        }

        public FuzzyRule AddAntecedent(string variableName, string setName)
        {
            Antecedents.Add(new Antecedent() { VariableName = variableName, SetName = setName });
            return this;
        }

        /// <summary>
        /// Вычисляет силу правила: min по членствам antecedents * weight.
        /// memberships: словарь variableName -> (setName -> membership)
        /// </summary>
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
