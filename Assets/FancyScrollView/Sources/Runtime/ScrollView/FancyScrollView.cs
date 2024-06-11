/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using EasingCore;

namespace FancyScrollView
{
    /// <summary>
    /// ScrollRect スタイルのスクロールビューを実装するための抽象基底クラス.
    /// 無限スクロールおよびスナップには対応していません.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <seealso cref="FancyScrollView{TItemData,TContext}"/>
    public abstract class FancyScrollView<TItemData> : FancyScrollView<TItemData, FancyScrollContext> { }

    /// <summary>
    /// ScrollRect スタイルのスクロールビューを実装するための抽象基底クラス.
    /// 無限スクロールおよびスナップには対応していません.
    /// <see cref="FancyListView{TItemData,TContext}.Context"/> が不要な場合は
    /// 代わりに <see cref="FancyScrollView{TItemData}"/> を使用します.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <typeparam name="TContext"><see cref="FancyListView{TItemData,TContext}.Context"/> の型.</typeparam>
    [RequireComponent(typeof(FancyScrollRect))]
    public abstract class FancyScrollView<TItemData, TContext> : FancyListView<TItemData, TContext>
        where TContext : class, IFancyScrollContext, new()
    {
        /// <summary>
        /// スクロール中にセルが再利用されるまでの余白のセル数.
        /// </summary>
        /// <remarks>
        /// <c>0</c> を指定するとセルが完全に隠れた直後に再利用されます.
        /// <c>1</c> 以上を指定すると, そのセル数だけ余分にスクロールしてから再利用されます.
        /// </remarks>
        [SerializeField] protected float reuseCellMarginCount = 0f;

        /// <summary>
        /// コンテンツ先頭の余白.
        /// </summary>
        [SerializeField] protected float paddingHead = 0f;

        /// <summary>
        /// コンテンツ末尾の余白.
        /// </summary>
        [SerializeField] protected float paddingTail = 0f;

        /// <summary>
        /// スクロール軸方向のセル同士の余白.
        /// </summary>
        [SerializeField] protected float spacing = 0f;

        /// <summary>
        /// セルのサイズ.
        /// </summary>
        protected abstract float CellSize { get; }

        /// <summary>
        /// スクロール可能かどうか.
        /// </summary>
        /// <remarks>
        /// アイテム数が十分少なくビューポート内に全てのセルが収まっている場合は <c>false</c>, それ以外は <c>true</c> になります.
        /// </remarks>
        protected virtual bool Scrollable => MaxScrollPosition > 0f;

        FancyScrollRect _cachedFancyScrollRect;

        /// <summary>
        /// スクロール位置を制御する <see cref="FancyScrollRect"/> のインスタンス.
        /// </summary>
        /// <remarks>
        /// <see cref="FancyScrollRect"/> のスクロール位置を変更する際は必ず <see cref="ToScrollerPosition(float,float)"/> を使用して変換した位置を使用してください.
        /// </remarks>
        protected FancyScrollRect FancyScrollRect =>
            _cachedFancyScrollRect ?? (_cachedFancyScrollRect = GetComponent<FancyScrollRect>());

        float ScrollLength => 1f / Mathf.Max(cellInterval, 1e-2f) - 1f;

        float ViewportLength => ScrollLength - reuseCellMarginCount * 2f;

        float PaddingHeadLength => (paddingHead - spacing * 0.5f) / (CellSize + spacing);

        float MaxScrollPosition => ItemsSource.Count
                                   - ScrollLength
                                   + reuseCellMarginCount * 2f
                                   + (paddingHead + paddingTail - spacing) / (CellSize + spacing);

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            Context.ScrollDirection = FancyScrollRect.scrollDirection;
            Context.CalculateScrollSize = () =>
            {
                var interval = CellSize + spacing;
                var reuseMargin = interval * reuseCellMarginCount;
                var scrollSize = FancyScrollRect.ViewportSize + interval + reuseMargin * 2f;
                return (scrollSize, reuseMargin);
            };

            AdjustCellIntervalAndScrollOffset();
            FancyScrollRect.OnValueChanged(OnScrollerValueChanged);
        }

        /// <summary>
        /// <see cref="FancyScrollRect"/> のスクロール位置が変更された際の処理.
        /// </summary>
        /// <param name="p"><see cref="FancyScrollRect"/> のスクロール位置.</param>
        void OnScrollerValueChanged(float p)
        {
            base.UpdatePosition(Scrollable ? ToFancyScrollViewPosition(p) : 0f);

            if (FancyScrollRect.scrollbar)
            {
                if (p > ItemsSource.Count - 1)
                {
                    ShrinkScrollbar(p - (ItemsSource.Count - 1));
                }
                else if (p < 0f)
                {
                    ShrinkScrollbar(-p);
                }
            }
        }

        /// <summary>
        /// スクロール範囲を超えてスクロールされた量に基づいて, スクロールバーのサイズを縮小します.
        /// </summary>
        /// <param name="offset">スクロール範囲を超えてスクロールされた量.</param>
        void ShrinkScrollbar(float offset)
        {
            var scale = 1f - ToFancyScrollViewPosition(offset) / (ViewportLength - PaddingHeadLength);
            UpdateScrollbarSize((ViewportLength - PaddingHeadLength) * scale);
        }

        /// <inheritdoc/>
        protected override void Refresh()
        {
            AdjustCellIntervalAndScrollOffset();
            RefreshScroller();
            base.Refresh();
        }

        /// <inheritdoc/>
        protected override void Relayout()
        {
            AdjustCellIntervalAndScrollOffset();
            RefreshScroller();
            base.Relayout();
        }

        /// <summary>
        /// <see cref="FancyScrollRect"/> の各種状態を更新します.
        /// </summary>
        protected void RefreshScroller()
        {
            FancyScrollRect.draggable = Scrollable;
            FancyScrollRect.scrollSensitivity = ToScrollerPosition(ViewportLength - PaddingHeadLength);
            FancyScrollRect.CurPosition = ToScrollerPosition(currentPosition);

            if (FancyScrollRect.scrollbar)
            {
                FancyScrollRect.scrollbar.gameObject.SetActive(Scrollable);
                UpdateScrollbarSize(ViewportLength);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateContents(IList<TItemData> items)
        {
            Debug.Assert(Context.CalculateScrollSize != null);

            AdjustCellIntervalAndScrollOffset();
            base.UpdateContents(items);

            FancyScrollRect.SetTotalCount(items.Count);
            RefreshScroller();
        }

        /// <summary>
        /// スクロール位置を更新します.
        /// </summary>
        /// <param name="position">スクロール位置.</param>
        protected new void UpdatePosition(float position)
        {
            FancyScrollRect.CurPosition = ToScrollerPosition(position);
        }

        /// <summary>
        /// 指定したアイテムの位置までジャンプします.
        /// </summary>
        /// <param name="itemIndex">アイテムのインデックス.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        protected virtual void JumpTo(int itemIndex, float alignment = 0.5f)
        {
            FancyScrollRect.CurPosition = ToScrollerPosition(itemIndex, alignment);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="index">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected virtual void ScrollTo(int index, float duration, float alignment = 0.5f, Action onComplete = null)
        {
            FancyScrollRect.ScrollTo(ToScrollerPosition(index, alignment), duration, onComplete);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="index">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="easing">移動に使用するイージング.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected virtual void ScrollTo(int index, float duration, Ease easing, float alignment = 0.5f,
            Action onComplete = null)
        {
            FancyScrollRect.ScrollTo(ToScrollerPosition(index, alignment), duration, easing, onComplete);
        }

        /// <summary>
        /// ビューポートとコンテンツの長さに基づいてスクロールバーのサイズを更新します.
        /// </summary>
        /// <param name="viewportLength">ビューポートのサイズ.</param>
        protected void UpdateScrollbarSize(float viewportLength)
        {
            var contentLength =
                Mathf.Max(ItemsSource.Count + (paddingHead + paddingTail - spacing) / (CellSize + spacing), 1);
            FancyScrollRect.scrollbar.size = Scrollable ? Mathf.Clamp01(viewportLength / contentLength) : 1f;
        }

        /// <summary>
        /// <see cref="FancyScrollRect"/> が扱うスクロール位置を <see cref="FancyScrollView{TItemData,TContext}"/> が扱うスクロール位置に変換します.
        /// </summary>
        /// <param name="position"><see cref="FancyScrollRect"/> が扱うスクロール位置.</param>
        /// <returns><see cref="FancyScrollView{TItemData,TContext}"/> が扱うスクロール位置.</returns>
        protected float ToFancyScrollViewPosition(float position)
        {
            return position / Mathf.Max(ItemsSource.Count - 1, 1) * MaxScrollPosition - PaddingHeadLength;
        }

        /// <summary>
        /// <see cref="FancyScrollView{TItemData,TContext}"/> が扱うスクロール位置を <see cref="FancyScrollRect"/> が扱うスクロール位置に変換します.
        /// </summary>
        /// <param name="position"><see cref="FancyScrollView{TItemData,TContext}"/> が扱うスクロール位置.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <returns><see cref="FancyScrollRect"/> が扱うスクロール位置.</returns>
        protected float ToScrollerPosition(float position, float alignment = 0.5f)
        {
            var offset = alignment * (ScrollLength - (1f + reuseCellMarginCount * 2f))
                         + (1f - alignment - 0.5f) * spacing / (CellSize + spacing);
            var fixOffset = Mathf.Clamp(position - offset, 0f, MaxScrollPosition);
            return (fixOffset + PaddingHeadLength) / MaxScrollPosition * Mathf.Max(ItemsSource.Count - 1, 1);
        }

        /// <summary>
        /// 指定された設定を実現するための
        /// <see cref="FancyListView{TItemData,TContext}.cellInterval"/> と
        /// <see cref="FancyListView{TItemData,TContext}.scrollOffset"/> を計算して適用します.
        /// </summary>
        protected void AdjustCellIntervalAndScrollOffset()
        {
            var totalSize = FancyScrollRect.ViewportSize + (CellSize + spacing) * (1f + reuseCellMarginCount * 2f);
            cellInterval = (CellSize + spacing) / totalSize;
            scrollOffset = cellInterval * (1f + reuseCellMarginCount);
        }

        protected virtual void OnValidate()
        {
            AdjustCellIntervalAndScrollOffset();

            if (loop)
            {
                loop = false;
                Debug.LogError("Loop is currently not supported in FancyScrollRect.");
            }

            var snap = FancyScrollRect.snap;
            if (snap != null && snap.Enable)
            {
                snap.Enable = false;
                Debug.LogError("Snap is currently not supported in FancyScrollRect.");
            }

            if (FancyScrollRect.movementType == MovementType.Unrestricted)
            {
                FancyScrollRect.movementType = MovementType.Elastic;
                Debug.LogError("MovementType.Unrestricted is currently not supported in FancyScrollRect.");
            }
        }
    }
}