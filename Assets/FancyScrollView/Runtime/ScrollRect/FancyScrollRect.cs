/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasingCore;

namespace FancyScrollView
{
    /// <summary>
    /// スクロール位置の制御を行うコンポーネント.
    /// </summary>
    public class FancyScrollRect : UIBehaviour, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler,
        IEndDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] public Snap snap = new Snap
        {
            enable = true, velocityThreshold = 0.5f, duration = 0.3f, easing = Ease.InOutCubic
        };

        [SerializeField] public RectTransform viewport; //ビューポート
        [SerializeField] public ScrollDirection scrollDirection = ScrollDirection.Vertical; //スクロール方向
        [SerializeField] public MovementType movementType = MovementType.Elastic; //コンテンツがスクロール範囲を越えて移動するときに使用する挙動.
        [SerializeField] public float elasticity = 0.1f; //コンテンツがスクロール範囲を越えて移動するときに使用する弾力性の量.
        [SerializeField] public float scrollSensitivity = 1f; //端から端まで Drag したときのスクロール位置の変化量.
        [SerializeField] public bool inertia = true; //慣性を使用するかどうか.trueを指定すると慣性が有効に,falseを指定すると慣性が無効になります.
        [SerializeField] public float decelerationRate = 0.03f; //スクロールの減速率.inertiaがtrue の場合のみ有効です.
        [SerializeField] public bool draggable = true; //Drag 入力を受付けるかどうか.
        [SerializeField] public Scrollbar scrollbar; //スクロールバーのオブジェクト
        private readonly AutoScrollState _autoScrollState = new AutoScrollState();
        private Action<float> _onScrollValueChanged;
        private Action<int> _onSelectionChanged;
        private Vector2 _beginDragPointerPosition;
        private float _scrollStartPosition;
        private float _prevPosition;
        private float _curPosition; //現在のスクロール位置.
        private int _totalCount;
        private bool _hold;
        private bool _scrolling;
        private bool _dragging;
        private float _velocity;

        /// <summary>
        /// ビューポートのサイズ.
        /// </summary>
        public float ViewportSize => scrollDirection == ScrollDirection.Horizontal
            ? viewport.rect.size.x
            : viewport.rect.size.y;

        /// <summary>
        /// 現在のスクロール位置.
        /// </summary>
        /// <value></value>
        public float CurPosition
        {
            get => _curPosition;
            set
            {
                _autoScrollState.Reset();
                _velocity = 0f;
                _dragging = false;
                UpdatePosition(value);
            }
        }

        protected override void Start()
        {
            base.Start();
            if (!scrollbar) return;
            scrollbar.onValueChanged.AddListener(x => OnScrollValueChanged(x * (_totalCount - 1f)));
        }

        protected void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;
            CalcPosition(deltaTime);
            //慣性力の計算
            CalcInertia(deltaTime);
            _prevPosition = _curPosition;
            _scrolling = false;
        }

        /// <inheritdoc/>
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _hold = true;
            _velocity = 0f;
            _autoScrollState.Reset();
        }

        /// <inheritdoc/>
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (_hold && snap.enable)
            {
                UpdateSelection(Mathf.Clamp(Mathf.RoundToInt(_curPosition), 0, _totalCount - 1));
                ScrollTo(Mathf.RoundToInt(_curPosition), snap.duration, snap.easing);
            }

            _hold = false;
        }

        /// <inheritdoc/>
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!draggable)
            {
                return;
            }

            var delta = eventData.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            var scrollDelta = scrollDirection == ScrollDirection.Horizontal
                ? Mathf.Abs(delta.y) > Mathf.Abs(delta.x)
                    ? delta.y
                    : delta.x
                : Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? delta.x
                    : delta.y;
            if (eventData.IsScrolling())
            {
                _scrolling = true;
            }

            var position = _curPosition + scrollDelta / ViewportSize * scrollSensitivity;
            if (movementType == MovementType.Clamped)
            {
                position += CalculateOffset(position);
            }

            if (_autoScrollState.enable)
            {
                _autoScrollState.Reset();
            }

            UpdatePosition(position);
        }

        /// <inheritdoc/>
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _hold = false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport, eventData.position, eventData.pressEventCamera, out _beginDragPointerPosition);
            _scrollStartPosition = _curPosition;
            _dragging = true;
            _autoScrollState.Reset();
        }

        /// <inheritdoc/>
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left || !_dragging)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var dragPointerPosition))
            {
                return;
            }

            var pointerDelta = dragPointerPosition - _beginDragPointerPosition;
            var position = (scrollDirection == ScrollDirection.Horizontal ? -pointerDelta.x : pointerDelta.y)
                           / ViewportSize
                           * scrollSensitivity
                           + _scrollStartPosition;

            var offset = CalculateOffset(position);
            position += offset;

            if (movementType == MovementType.Elastic)
            {
                if (offset != 0f)
                {
                    position -= RubberDelta(offset, scrollSensitivity);
                }
            }

            UpdatePosition(position);
        }

        /// <inheritdoc/>
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _dragging = false;
        }

        /// <summary>
        /// スクロール位置が変化したときのコールバックを設定します.
        /// </summary>
        /// <param name="callback">スクロール位置が変化したときのコールバック.</param>
        public void OnValueChanged(Action<float> callback) => _onScrollValueChanged = callback;

        /// <summary>
        /// 選択位置が変化したときのコールバックを設定します.
        /// </summary>
        /// <param name="callback">選択位置が変化したときのコールバック.</param>
        public void OnSelectionChanged(Action<int> callback) => _onSelectionChanged = callback;

        /// <summary>
        /// アイテムの総数を設定します.
        /// </summary>
        /// <remarks>
        /// <paramref name="totalCount"/> を元に最大スクロール位置を計算します.
        /// </remarks>
        /// <param name="totalCount">アイテムの総数.</param>
        public void SetTotalCount(int totalCount) => _totalCount = totalCount;

        /// <summary>
        /// 指定したインデックスの位置までジャンプします.
        /// </summary>
        /// <param name="index">アイテムのインデックス.</param>
        public void JumpTo(int index)
        {
            if (index < 0 || index > _totalCount - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            UpdateSelection(index);
            CurPosition = index;
        }

        /// <summary>
        /// <paramref name="sourceIndex"/> から <paramref name="destIndex"/> に移動する際の移動方向を返します.
        /// スクロール範囲が無制限に設定されている場合は, 最短距離の移動方向を返します.
        /// </summary>
        /// <param name="sourceIndex">移動元のインデックス.</param>
        /// <param name="destIndex">移動先のインデックス.</param>
        /// <returns></returns>
        public MovementDirection GetMovementDirection(int sourceIndex, int destIndex)
        {
            var movementAmount = CalculateMovementAmount(sourceIndex, destIndex);
            return scrollDirection == ScrollDirection.Horizontal
                ? movementAmount > 0
                    ? MovementDirection.Left
                    : MovementDirection.Right
                : movementAmount > 0
                    ? MovementDirection.Up
                    : MovementDirection.Down;
        }

        /// <summary>
        /// 指定した位置まで移動します.
        /// </summary>
        /// <param name="position">スクロール位置. <c>0f</c> ~ <c>totalCount - 1f</c> の範囲.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        public void ScrollTo(float position, float duration, Action onComplete = null) =>
            ScrollTo(position, duration, Ease.OutCubic, onComplete);

        /// <summary>
        /// 指定した位置まで移動します.
        /// </summary>
        /// <param name="position">スクロール位置. <c>0f</c> ~ <c>totalCount - 1f</c> の範囲.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="easing">移動に使用するイージング.</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        public void ScrollTo(float position, float duration, Ease easing, Action onComplete = null) =>
            ScrollTo(position, duration, Easing.Get(easing), onComplete);

        /// <summary>
        /// 指定した位置まで移動します.
        /// </summary>
        /// <param name="position">スクロール位置. <c>0f</c> ~ <c>totalCount - 1f</c> の範囲.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="easingFunction">移動に使用するイージング関数.</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        private void ScrollTo(float position, float duration, EasingFunction easingFunction, Action onComplete = null)
        {
            if (duration <= 0f)
            {
                CurPosition = CircularPosition(position, _totalCount);
                onComplete?.Invoke();
                return;
            }

            _autoScrollState.Reset();
            _autoScrollState.enable = true;
            _autoScrollState.duration = duration;
            _autoScrollState.easingFunction = easingFunction ?? Easing.Get(Ease.OutCubic);
            _autoScrollState.startTime = Time.unscaledTime;
            _autoScrollState.endPosition = _curPosition + CalculateMovementAmount(_curPosition, position);
            _autoScrollState.onComplete = onComplete;
            _velocity = 0f;
            _scrollStartPosition = _curPosition;
            UpdateSelection(Mathf.RoundToInt(CircularPosition(_autoScrollState.endPosition, _totalCount)));
        }

        private float CalculateOffset(float position)
        {
            if (movementType == MovementType.Unrestricted)
            {
                return 0f;
            }

            if (position < 0f)
            {
                return -position;
            }

            if (position > _totalCount - 1)
            {
                return _totalCount - 1 - position;
            }

            return 0f;
        }

        private void UpdatePosition(float position)
        {
            OnScrollValueChanged(position);
            if (!scrollbar)
            {
                return;
            }

            scrollbar.value = Mathf.Clamp01(position / Mathf.Max(_totalCount - 1f, 1e-4f));
        }

        private void OnScrollValueChanged(float position)
        {
            _curPosition = position;
            _onScrollValueChanged?.Invoke(position);
        }

        private void UpdateSelection(int index) => _onSelectionChanged?.Invoke(index);

        private static float RubberDelta(float overStretching, float viewSize) =>
            (1 - 1 / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1)) * viewSize * Mathf.Sign(overStretching);

        private float CalculateMovementAmount(float sourcePosition, float destPosition)
        {
            if (movementType != MovementType.Unrestricted)
            {
                return Mathf.Clamp(destPosition, 0, _totalCount - 1) - sourcePosition;
            }

            var amount = CircularPosition(destPosition, _totalCount) - CircularPosition(sourcePosition, _totalCount);
            if (Mathf.Abs(amount) > _totalCount * 0.5f)
            {
                amount = Mathf.Sign(-amount) * (_totalCount - Mathf.Abs(amount));
            }

            return amount;
        }

        private static float CircularPosition(float p, int size) =>
            size < 1 ? 0 : p < 0 ? size - 1 + (p + 1) % size : p % size;

        /// <summary>
        /// 慣性力の計算
        /// </summary>
        private void CalcInertia(float deltaTime)
        {
            if (!_autoScrollState.enable && (_dragging || _scrolling) && inertia)
            {
                var newVelocity = (_curPosition - _prevPosition) / deltaTime;
                _velocity = Mathf.Lerp(_velocity, newVelocity, deltaTime * 10f);
            }
        }

        private float CalcPositionAutoScroll()
        {
            var alpha = Mathf.Clamp01((Time.unscaledTime - _autoScrollState.startTime) /
                                      Mathf.Max(_autoScrollState.duration, float.Epsilon));
            var position = Mathf.LerpUnclamped(_scrollStartPosition, _autoScrollState.endPosition,
                _autoScrollState.easingFunction(alpha));
            if (Mathf.Approximately(alpha, 1f))
            {
                _autoScrollState.Complete();
            }

            return position;
        }

        private float CalcPositionAutoScrollElastic(float offset, float deltaTime)
        {
            var position = Mathf.SmoothDamp(_curPosition, _curPosition + offset, ref _velocity,
                elasticity, Mathf.Infinity, deltaTime);
            if (Mathf.Abs(_velocity) < 0.01f)
            {
                position = Mathf.Clamp(Mathf.RoundToInt(position), 0, _totalCount - 1);
                _velocity = 0f;
                _autoScrollState.Complete();
            }

            return position;
        }

        private float CalcClamped(float position)
        {
            if (movementType != MovementType.Clamped)
            {
                return position;
            }

            var offset = CalculateOffset(position);
            position += offset;
            if (!Mathf.Approximately(position, 0f) && !Mathf.Approximately(position, _totalCount - 1f))
            {
                return position;
            }

            _velocity = 0f;
            UpdateSelection(Mathf.RoundToInt(position));
            return position;
        }

        private void CalcPosition(float deltaTime)
        {
            var offset = CalculateOffset(_curPosition);
            if (_autoScrollState.enable)
            {
                var pos = _autoScrollState.elastic
                    ? CalcPositionAutoScrollElastic(offset, deltaTime)
                    : CalcPositionAutoScroll();
                UpdatePosition(pos);
                return;
            }

            if (_dragging || _scrolling || (Mathf.Approximately(offset, 0f) && Mathf.Approximately(_velocity, 0f)))
            {
                return;
            }

            var position = CalcPostionScroll(offset, deltaTime);
            if (Mathf.Approximately(_velocity, 0f))
            {
                return;
            }

            var fixPosition = CalcClamped(position);
            UpdatePosition(fixPosition);
        }

        private float CalcPostionScroll(float offset, float deltaTime)
        {
            var position = _curPosition;
            //elastic
            if (movementType == MovementType.Elastic && !Mathf.Approximately(offset, 0f))
            {
                _autoScrollState.Reset();
                _autoScrollState.enable = true;
                _autoScrollState.elastic = true;
                UpdateSelection(Mathf.Clamp(Mathf.RoundToInt(position), 0, _totalCount - 1));
                return position;
            }

            //common
            if (!inertia)
            {
                _velocity = 0f;
                return position;
            }

            //inertia
            _velocity *= Mathf.Pow(decelerationRate, deltaTime);
            if (Mathf.Abs(_velocity) < 0.001f)
            {
                _velocity = 0f;
            }

            position += _velocity * deltaTime;
            if (snap.enable && Mathf.Abs(_velocity) < snap.velocityThreshold)
            {
                ScrollTo(Mathf.RoundToInt(_curPosition), snap.duration, snap.easing);
            }

            return position;
        }
    }
}