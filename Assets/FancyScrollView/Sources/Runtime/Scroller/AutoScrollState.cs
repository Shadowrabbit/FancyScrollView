// ******************************************************************
//       /\ /|       @file       AutoScrollState
//       \ V/        @brief      
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2024-06-09 20:16
//    *(__\_\        @Copyright  Copyright (c) 2024, Shadowrabbit
// ******************************************************************

using System;
using EasingCore;

namespace FancyScrollView
{
    public class AutoScrollState
    {
        public bool Enable;
        public bool Elastic;
        public float Duration;
        public EasingFunction EasingFunction;
        public float StartTime;
        public float EndPosition;
        public Action OnComplete;

        public void Reset()
        {
            Enable = false;
            Elastic = false;
            Duration = 0f;
            StartTime = 0f;
            EasingFunction = Easing.Get(Ease.OutCubic);
            EndPosition = 0f;
            OnComplete = null;
        }

        public void Complete()
        {
            OnComplete?.Invoke();
            Reset();
        }
    }
}