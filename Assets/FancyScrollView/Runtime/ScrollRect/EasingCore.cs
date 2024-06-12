/*
 * EasingCore (https://github.com/setchi/EasingCore)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/EasingCore/blob/master/LICENSE)
 */

using System.Collections.Generic;
using UnityEngine;

namespace EasingCore
{
    public enum Ease
    {
        Linear,
        InBack,
        InBounce,
        InCirc,
        InCubic,
        InElastic,
        InExpo,
        InQuad,
        InQuart,
        InQuint,
        InSine,
        OutBack,
        OutBounce,
        OutCirc,
        OutCubic,
        OutElastic,
        OutExpo,
        OutQuad,
        OutQuart,
        OutQuint,
        OutSine,
        InOutBack,
        InOutBounce,
        InOutCirc,
        InOutCubic,
        InOutElastic,
        InOutExpo,
        InOutQuad,
        InOutQuart,
        InOutQuint,
        InOutSine,
    }

    public delegate float EasingFunction(float t);

    public static class Easing
    {
        private static Dictionary<Ease, EasingFunction> _easeType2Func =
            new Dictionary<Ease, EasingFunction>
            {
                { Ease.Linear, Linear },
                { Ease.InBack, InBack },
                { Ease.InBounce, InBounce },
                { Ease.InCirc, InCirc },
                { Ease.InCubic, InCubic },
                { Ease.InElastic, InElastic },
                { Ease.InExpo, InExpo },
                { Ease.InQuad, InQuad },
                { Ease.InQuart, InQuart },
                { Ease.InQuint, InQuint },
                { Ease.InSine, InSine },
                { Ease.OutBack, OutBack },
                { Ease.OutBounce, OutBounce },
                { Ease.OutCirc, OutCirc },
                { Ease.OutCubic, OutCubic },
                { Ease.OutElastic, OutElastic },
                { Ease.OutExpo, OutExpo },
                { Ease.OutQuad, OutQuad },
                { Ease.OutQuart, OutQuart },
                { Ease.OutQuint, OutQuint },
                { Ease.OutSine, OutSine },
                { Ease.InOutBack, InOutBack },
                { Ease.InOutBounce, InOutBounce },
                { Ease.InOutCirc, InOutCirc },
                { Ease.InOutCubic, InOutCubic },
                { Ease.InOutElastic, InOutElastic },
                { Ease.InOutExpo, InOutExpo },
                { Ease.InOutQuad, InOutQuad },
                { Ease.InOutQuart, InOutQuart },
                { Ease.InOutQuint, InOutQuint },
                { Ease.InOutSine, InOutSine },
            };

        /// <summary>
        /// Gets the easing function
        /// </summary>
        /// <param name="type">Ease type</param>
        /// <returns>Easing function</returns>
        public static EasingFunction Get(Ease type)
        {
            if (_easeType2Func.ContainsKey(type))
            {
                return _easeType2Func[type];
            }

            return Linear;
        }

        private static float Linear(float t) => t;

        private static float InBack(float t) => t * t * t - t * Mathf.Sin(t * Mathf.PI);

        private static float OutBack(float t) => 1f - InBack(1f - t);

        private static float InOutBack(float t) =>
            t < 0.5f
                ? 0.5f * InBack(2f * t)
                : 0.5f * OutBack(2f * t - 1f) + 0.5f;

        private static float InBounce(float t) => 1f - OutBounce(1f - t);

        private static float OutBounce(float t) =>
            t < 4f / 11.0f ? (121f * t * t) / 16.0f :
            t < 8f / 11.0f ? (363f / 40.0f * t * t) - (99f / 10.0f * t) + 17f / 5.0f :
            t < 9f / 10.0f ? (4356f / 361.0f * t * t) - (35442f / 1805.0f * t) + 16061f / 1805.0f :
            (54f / 5.0f * t * t) - (513f / 25.0f * t) + 268f / 25.0f;

        private static float InOutBounce(float t) =>
            t < 0.5f
                ? 0.5f * InBounce(2f * t)
                : 0.5f * OutBounce(2f * t - 1f) + 0.5f;

        private static float InCirc(float t) => 1f - Mathf.Sqrt(1f - (t * t));

        private static float OutCirc(float t) => Mathf.Sqrt((2f - t) * t);

        private static float InOutCirc(float t) =>
            t < 0.5f
                ? 0.5f * (1 - Mathf.Sqrt(1f - 4f * (t * t)))
                : 0.5f * (Mathf.Sqrt(-((2f * t) - 3f) * ((2f * t) - 1f)) + 1f);

        private static float InCubic(float t) => t * t * t;

        private static float OutCubic(float t) => InCubic(t - 1f) + 1f;

        private static float InOutCubic(float t) =>
            t < 0.5f
                ? 4f * t * t * t
                : 0.5f * InCubic(2f * t - 2f) + 1f;

        private static float InElastic(float t) =>
            Mathf.Sin(13f * (Mathf.PI * 0.5f) * t) * Mathf.Pow(2f, 10f * (t - 1f));

        private static float OutElastic(float t) =>
            Mathf.Sin(-13f * (Mathf.PI * 0.5f) * (t + 1)) * Mathf.Pow(2f, -10f * t) + 1f;

        private static float InOutElastic(float t) =>
            t < 0.5f
                ? 0.5f * Mathf.Sin(13f * (Mathf.PI * 0.5f) * (2f * t)) * Mathf.Pow(2f, 10f * ((2f * t) - 1f))
                : 0.5f * (Mathf.Sin(-13f * (Mathf.PI * 0.5f) * ((2f * t - 1f) + 1f)) *
                    Mathf.Pow(2f, -10f * (2f * t - 1f)) + 2f);

        private static float InExpo(float t) => Mathf.Approximately(0.0f, t) ? t : Mathf.Pow(2f, 10f * (t - 1f));

        private static float OutExpo(float t) => Mathf.Approximately(1.0f, t) ? t : 1f - Mathf.Pow(2f, -10f * t);

        private static float InOutExpo(float v) =>
            Mathf.Approximately(0.0f, v) || Mathf.Approximately(1.0f, v)
                ? v
                : v < 0.5f
                    ? 0.5f * Mathf.Pow(2f, (20f * v) - 10f)
                    : -0.5f * Mathf.Pow(2f, (-20f * v) + 10f) + 1f;

        private static float InQuad(float t) => t * t;

        private static float OutQuad(float t) => -t * (t - 2f);

        private static float InOutQuad(float t) =>
            t < 0.5f
                ? 2f * t * t
                : -2f * t * t + 4f * t - 1f;

        private static float InQuart(float t) => t * t * t * t;

        private static float OutQuart(float t)
        {
            var u = t - 1f;
            return u * u * u * (1f - t) + 1f;
        }

        private static float InOutQuart(float t) =>
            t < 0.5f
                ? 8f * InQuart(t)
                : -8f * InQuart(t - 1f) + 1f;

        private static float InQuint(float t) => t * t * t * t * t;

        private static float OutQuint(float t) => InQuint(t - 1f) + 1f;

        private static float InOutQuint(float t) =>
            t < 0.5f
                ? 16f * InQuint(t)
                : 0.5f * InQuint(2f * t - 2f) + 1f;

        private static float InSine(float t) => Mathf.Sin((t - 1f) * (Mathf.PI * 0.5f)) + 1f;

        private static float OutSine(float t) => Mathf.Sin(t * (Mathf.PI * 0.5f));

        private static float InOutSine(float t) => 0.5f * (1f - Mathf.Cos(t * Mathf.PI));
    }
}