using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FuzzyLogic
{
    /// <summary>
    /// Хранит список правил.
    /// Метод Calculate(inputs):
    ///		Фаззификация: Получает степени принадлежности для всех входов.
    ///		Агрегация: Прогоняет через правила.
    ///		Дефаззификация: Собирает результаты всех правил и превращает их обратно в одно число (например, методом Центра Тяжести - Centroid).
    ///
    /// Хранит входные переменные, выходную переменную и список правил.
    /// Calculate(inputs) -> возвращает число (дефаззификация методом центроида, samplingResolution точек).
    /// inputs: словарь имя -> значение
    /// </summary>
    public class FuzzyInferenceSystem
    {
        public Dictionary<string, FuzzyVariable> InputVariables = new Dictionary<string, FuzzyVariable>();
        public FuzzyVariable OutputVariable;
        public List<FuzzyRule> Rules = new List<FuzzyRule>();

        public int SamplingResolution = 60; // точек для дефаззификации

        public FuzzyInferenceSystem() { }

        public void AddInput(FuzzyVariable v)
        {
            if (v != null) InputVariables[v.Name] = v;
        }

        public void SetOutput(FuzzyVariable v)
        {
            OutputVariable = v;
        }

        public void AddRule(FuzzyRule r)
        {
            if (r != null) Rules.Add(r);
        }

        /// <summary>
        /// inputs: имя -> значение (четкое)
        /// Возвращает дефаззифицированное значение (float)
        /// </summary>
        public float Calculate(Dictionary<string, float> inputs)
        {
            if (OutputVariable == null)
            {
                Debug.LogError("FIS ERROR: Output variable is NULL!");
                return 0f;
            }

            Debug.Log("\n\n========== FIS CALCULATION START ==========");
            //Debug.Log("Inputs:");
            //foreach (var inp in inputs)
            //    Debug.Log($"  {inp.Key} = {inp.Value}");

            Debug.Log("\n--- 1) FUZZIFICATION ---");

            var memberships = new Dictionary<string, Dictionary<string, float>>();

            foreach (var kv in InputVariables)
            {
                string varName = kv.Key;
                var variable = kv.Value;

                float inputVal = inputs.ContainsKey(varName) ? inputs[varName] : variable.Min;

                Debug.Log($"\nVariable: {varName}, input = {inputVal}, {inputs.ContainsKey(varName)}");

                var fuzz = variable.Fuzzify(inputVal);
                memberships[varName] = fuzz;

                //foreach (var set in fuzz)
                //    Debug.Log($"    Set {set.Key}: μ = {set.Value:F3}");
            }

            Debug.Log("\n--- 2) RULE EVALUATION ---");

            List<(FuzzyRule rule, float strength)> fired = new List<(FuzzyRule, float)>();

            foreach (var r in Rules)
            {
                float s = r.Evaluate(memberships);

                string cond = string.Join(", ",
                    r.Antecedents.Select(a => $"{a.VariableName} is {a.SetName}"));

                if(s>0) Debug.Log($"Rule: IF {cond} THEN {r.ConsequentSetName}  => strength = {s:F3}");

                if (s > 0f)
                    fired.Add((r, s));
            }

            if (fired.Count == 0)
            {
                Debug.LogWarning("NO RULE FIRED! Returning default midpoint value.");
                return (OutputVariable.Min + OutputVariable.Max) * 0.5f;
            }

            Debug.Log($"\nTotal fired rules: {fired.Count}");

            Debug.Log("\n--- 3) DEFUZZIFICATION (centroid) ---");

            float sumWeighted = 0f;
            float sumWeights = 0f;

            int N = Mathf.Max(6, SamplingResolution);

            for (int i = 0; i < N; i++)
            {
                float t = i / (float)(N - 1); // 0..1 normalized position

                float aggregated = 0f;

                foreach (var fr in fired)
                {
                    var outSet = OutputVariable.Sets.FirstOrDefault(s => s.Name == fr.rule.ConsequentSetName);
                    if (outSet == null) continue;

                    float raw = outSet.GetMembership(t);
                    float clipped = Mathf.Min(raw, fr.strength);

                    aggregated = Mathf.Max(aggregated, clipped);
                }

                float realX = OutputVariable.Denormalize(t);

                sumWeighted += aggregated * realX;
                sumWeights += aggregated;
            }

            if (sumWeights <= 1e-6f)
            {
                Debug.LogWarning("AGGREGATION ZERO — returning midpoint!");
                return (OutputVariable.Min + OutputVariable.Max) * 0.5f;
            }

            float result = sumWeighted / sumWeights;

            Debug.Log($"\n=== RESULT: {result:F3} ===");
            Debug.Log("========== FIS CALCULATION END ==========\n\n");

            return result;
        }

    }
}
