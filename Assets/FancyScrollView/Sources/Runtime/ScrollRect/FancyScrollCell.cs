/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;

namespace FancyScrollView
{
    /// <summary>
    /// <see cref="FancyScrollView{TItemData,TContext}"/> のセルを実装するための抽象基底クラス.
    /// <see cref="FancyCell{TItemData, TContext}.Context"/> が不要な場合は
    /// 代わりに <see cref="FancyScrollCell{TItemData}"/> を使用します.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <typeparam name="TContext"><see cref="FancyCell{TItemData, TContext}.Context"/> の型.</typeparam>
    public abstract class FancyScrollCell<TItemData, TContext> : FancyCell<TItemData, TContext>
        where TContext : class, IFancyScrollContext, new()
    {
        /// <inheritdoc/>
        public override void UpdatePosition(float position)
        {
            var (scrollSize, reuseMargin) = Context.CalculateScrollSize();
            var normalizedPosition =
                (Mathf.Lerp(0f, scrollSize, position) - reuseMargin) / (scrollSize - reuseMargin * 2f);
            var start = 0.5f * scrollSize;
            var end = -start;
            UpdatePosition(normalizedPosition, Mathf.Lerp(start, end, position));
        }

        /// <summary>
        /// このセルの位置を更新します
        /// </summary>
        /// <param name="normalizedPosition">ビューポートの範囲で正規化されたスクロール位置.</param>
        /// <param name="localPosition">ローカル位置</param>
        protected virtual void UpdatePosition(float normalizedPosition, float localPosition)
        {
            transform.localPosition = Context.ScrollDirection == ScrollDirection.Horizontal
                ? new Vector2(-localPosition, 0)
                : new Vector2(0, localPosition);
        }
    }

    /// <summary>
    /// <see cref="FancyScrollView{TItemData}"/> のセルを実装するための抽象基底クラス.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <seealso cref="FancyScrollCell{TItemData,TContext}"/>
    public abstract class FancyScrollCell<TItemData> : FancyScrollCell<TItemData, FancyScrollContext>
    {
        /// <inheritdoc/>
        public sealed override void SetContext(FancyScrollContext context) => base.SetContext(context);
    }
}