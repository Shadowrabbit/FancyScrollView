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
        public bool enable;
        public bool elastic;
        public float duration;
        public EasingFunction easingFunction;
        public float startTime;
        public float endPosition;
        public Action onComplete;

        public void Reset()
        {
            enable = false;
            elastic = false;
            duration = 0f;
            startTime = 0f;
            easingFunction = Easing.Get(Ease.OutCubic);
            endPosition = 0f;
            onComplete = null;
        }

        public void Complete()
        {
            onComplete?.Invoke();
            Reset();
        }
    }
}