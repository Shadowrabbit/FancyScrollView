/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using UnityEngine;

namespace FancyScrollView
{
    /// <summary>
    /// <see cref="FancyGridView{TItemData, TContext}"/> のコンテキスト基底クラス.
    /// </summary>
    public class FancyGridViewContext : IFancyGridViewContext
    {
        public ScrollDirection ScrollDirection { get; set; }
        public Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
        public GameObject CellTemplate { get; set; }
        public Func<int> GetGroupCount { get; set; }
        public Func<float> GetStartAxisSpacing { get; set; }
        public Func<float> GetCellSize { get; set; }
    }
}