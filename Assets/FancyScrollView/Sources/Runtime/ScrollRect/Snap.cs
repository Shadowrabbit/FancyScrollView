// ******************************************************************
//       /\ /|       @file       Snap
//       \ V/        @brief      
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2024-06-09 20:15
//    *(__\_\        @Copyright  Copyright (c) 2024, Shadowrabbit
// ******************************************************************

using System;
using EasingCore;
using UnityEngine.Serialization;

namespace FancyScrollView
{
    [Serializable]
    public class Snap
    {
        public bool enable; //スナップを有効にすると, 慣性でスクロールが止まる直前に最寄りのセルへ移動します. trueならスナップし, falseならスナップしません.
        public float velocityThreshold;
        public float duration;
        public Ease easing;
    }
}