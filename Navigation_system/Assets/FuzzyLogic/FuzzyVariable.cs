using System.Collections.Generic;
using UnityEngine;

namespace FuzzyLogic
{
    /// <summary>
    /// Хранит список FuzzySet.
    /// Пример: Переменная "FrontDistance" содержит сеты ["Near", "Medium", "Far"].
    /// Метод: Fuzzify(float value) — преобразует четкое число во входные значения для правил.
    ///
    /// Переменная, содержащая список FuzzySet.
    /// min/max задают диапазон четкой переменной для нормализации в 0..1.
    /// </summary>
    public class FuzzyVariable
    {
        public string Name;
        public float Min;
        public float Max;
        private List<FuzzySet> _sets = new List<FuzzySet>();

        public IReadOnlyList<FuzzySet> Sets => _sets;

        public FuzzyVariable(string name, float min, float max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public void AddSet(FuzzySet set)
        {
            if (set != null) _sets.Add(set);
        }

        /// <summary>
        /// Возвращает словарь setName -> membership (0..1) для заданного value.
        /// </summary>
        public Dictionary<string, float> Fuzzify(float value)
        {
            Dictionary<string, float> res = new Dictionary<string, float>();
            float denom = Max - Min;
            float norm = denom <= 0f ? 0f : (value - Min) / denom;
            norm = Mathf.Clamp01(norm);

            foreach (var s in _sets)
            {
                res[s.Name] = s.GetMembership(norm);
            }
            return res;
        }

        /// <summary>
        /// Нужен для дефаззификации — преобразует нормализованное x (0..1) к реальному значению.
        /// </summary>
        public float Denormalize(float norm)
        {
            return Mathf.Lerp(Min, Max, Mathf.Clamp01(norm));
        }
    }
}
