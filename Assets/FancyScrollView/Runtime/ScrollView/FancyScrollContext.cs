/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;

namespace FancyScrollView
{
    /// <summary>
    /// <see cref="FancyScrollView{TItemData,TContext}"/> のコンテキスト基底クラス.
    /// </summary>
    public class FancyScrollContext : IFancyScrollContext
    {
        public ScrollDirection ScrollDirection { get; set; }
        public Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
    }
}