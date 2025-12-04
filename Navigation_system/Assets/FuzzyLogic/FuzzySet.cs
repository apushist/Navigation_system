using System;
using UnityEngine;

namespace FuzzyLogic
{
    /// <summary>
    /// Описывает форму графика (обычно треугольник или трапеция). Может быть AnimationCurve для удобной настройки.
    /// Метод: GetMembership(float value) — возвращает число от 0 до 1 (степень принадлежности).
    /// Пример: Для множества "Близко" дистанция 2 метра может дать 0.8, а 5 метров — 0.0.
    ///
    /// Используется фабрика трапеций
    /// </summary>
    [Serializable]
    public class FuzzySet
    {
        public string Name;
        public AnimationCurve Curve;

        public FuzzySet() { }

        public FuzzySet(string name, AnimationCurve curve)
        {
            Name = name;
            Curve = curve;
        }

        /// <summary>
        /// valueNormalized = (value - varMin) / (varMax - varMin)
        /// проверяет принадлежность значения множеству
        /// </summary>
        public float GetMembership(float valueNormalized)
        {
            if (Curve == null) return 0f;
            // clamp normalized to 0..1
            float x = Mathf.Clamp01(valueNormalized);
            if (x == 1) x -= 0.001f;//костыль, при 1 Curve.Evaluate(x) работает не корректно
            //Debug.Log($"{x} -  clamp val for {Name}, {Curve.Evaluate(x)} - Curve.Evaluate(x)");
            return Mathf.Clamp01(Curve.Evaluate(x));
        }

        /// <summary>
        /// Создаёт трапециевидное множество, входы - 4 точки трапеции, должны быть ограничены (0..1)
        /// </summary>
        public static FuzzySet Trapezoid(string name, float leftZero, float leftOne, float rightOne, float rightZero)
        {
            Keyframe[] kf = new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(leftZero, 0f),
                new Keyframe(leftOne, 1f),
                new Keyframe(rightOne, 1f),
                new Keyframe(rightZero, 0f),
                new Keyframe(1f, 0f)
            };
            AnimationCurve curve = new AnimationCurve(kf);
            return new FuzzySet(name, curve);
        }
    }
}
