using System.Collections.Generic;
using Crane.Common;
using Crane.Data;
using Crane.Game;
using Crane.Presentation;
using Crane.Score;
using TMPro;
using UnityEngine;

namespace Crane.UI
{
    /// <summary>
    /// スーパーご利益タイム中、ヒットのたびに「ご利益」を飛び出させ、画面内で跳ね回らせる。
    /// </summary>
    /// <remarks>
    /// 飛び出す「ご利益」の大きさは、増えたご利益の量から求めた強さ(<see cref="HitStrength"/>)で決める。
    /// 飛び出す向きは左右交互で、重力で落ちながら画面端で反射する。
    /// </remarks>
    public class BouncingBenefitSpawner : MonoBehaviour
    {
        /// <summary>
        /// 画面内で跳ね回る「ご利益」1 個分の状態。
        /// </summary>
        private sealed class BouncingBenefit
        {
            /// <summary>表示中のオブジェクト。</summary>
            public RectTransform Rect;

            /// <summary>現在の速度(親のローカル単位/秒)。</summary>
            public Vector2 Velocity;
        }

        /// <summary>横方向の速さ。表示範囲の幅に対する比。</summary>
        private const float HorizontalSpeedRatio = 0.32f;

        /// <summary>縦方向の初速の下限。表示範囲の高さに対する比。</summary>
        private const float MinVerticalSpeedRatio = 0.16f;

        /// <summary>縦方向の初速の上限。表示範囲の高さに対する比。</summary>
        private const float MaxVerticalSpeedRatio = 0.28f;

        /// <summary>重力加速度。表示範囲の高さに対する比(/秒²)。</summary>
        private const float GravityRatio = 0.08f;

        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>ご利益の変化を通知するスコア管理。</summary>
        [SerializeField] private ScoreController scoreController;

        /// <summary>強さに応じた拡大率の設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>画面の範囲を求めるためのカメラ。</summary>
        [SerializeField] private Camera gameplayCamera;

        /// <summary>「ご利益」の複製元。</summary>
        [SerializeField] private RectTransform benefitTemplate;

        /// <summary>飛び出す速さの基準にする範囲。</summary>
        [SerializeField] private RectTransform movementBounds;

        /// <summary>跳ね回っている「ご利益」。</summary>
        private readonly List<BouncingBenefit> bouncingBenefits = new List<BouncingBenefit>();

        /// <summary>複製元の表示を切り替える CanvasGroup。</summary>
        private CanvasGroup templateCanvasGroup;

        /// <summary>複製元の拡大率。</summary>
        private Vector3 templateBaseScale;

        /// <summary>スーパーご利益タイム中なら true。</summary>
        private bool isActive;

        /// <summary>直前に通知されたご利益。増えた量を求めるために使う。</summary>
        private int lastBenefit;

        /// <summary>
        /// 必須参照を検証し、複製元を隠す。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            templateCanvasGroup = CanvasGroupUtility.GetOrAdd(benefitTemplate.gameObject);
            templateCanvasGroup.alpha = 0f;
            templateBaseScale = benefitTemplate.localScale;
        }

        /// <summary>
        /// 状態の変化とご利益の変化を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
            scoreController.BenefitChanged += HandleBenefitChanged;
        }

        /// <summary>
        /// 購読を解除し、飛び出させた「ご利益」を消す。
        /// </summary>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
            scoreController.BenefitChanged -= HandleBenefitChanged;
            isActive = false;
            DestroyAll();
        }

        /// <summary>
        /// 跳ね回っている「ご利益」を動かす。
        /// </summary>
        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            MoveAll(Time.unscaledDeltaTime);
        }

        /// <summary>
        /// スーパーご利益タイムに入ったら受付を始め、抜けたら「ご利益」を消す。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            DestroyAll();

            if (next == GameState.BenefitTime)
            {
                isActive = true;
                lastBenefit = scoreController.Benefit;
                return;
            }

            isActive = false;
        }

        /// <summary>
        /// ご利益が増えたら、増えた量に応じた大きさの「ご利益」を飛び出させる。
        /// </summary>
        /// <param name="benefit">変更後のご利益。</param>
        private void HandleBenefitChanged(int benefit)
        {
            if (!isActive)
            {
                return;
            }

            int gainedBenefit = Mathf.Max(0, benefit - lastBenefit);
            lastBenefit = benefit;

            float strength = HitStrength.FromAmount(gainedBenefit, settings);
            float scaleIncrease = HitStrength.ToEffectScale(strength, settings);
            Spawn(scaleIncrease);
        }

        /// <summary>
        /// 「ご利益」を複製して飛び出させる。左右交互の向きに飛ぶ。
        /// </summary>
        /// <param name="scaleIncrease">複製元の大きさに加える拡大率。</param>
        private void Spawn(float scaleIncrease)
        {
            RectTransform instance = Instantiate(benefitTemplate, benefitTemplate.parent, false);
            instance.name = benefitTemplate.name + " (Hit)";
            instance.localScale = templateBaseScale * (1f + scaleIncrease);
            CanvasGroupUtility.GetOrAdd(instance.gameObject).alpha = 1f;

            float horizontalDirection = 1f;
            if (bouncingBenefits.Count % 2 == 0)
            {
                horizontalDirection = -1f;
            }

            Vector2 boundsSize = movementBounds.rect.size;
            BouncingBenefit bouncing = new BouncingBenefit();
            bouncing.Rect = instance;
            bouncing.Velocity = new Vector2(
                boundsSize.x * HorizontalSpeedRatio * horizontalDirection,
                boundsSize.y * Random.Range(MinVerticalSpeedRatio, MaxVerticalSpeedRatio));
            bouncingBenefits.Add(bouncing);
        }

        /// <summary>
        /// すべての「ご利益」を重力で落とし、画面端で反射させる。
        /// </summary>
        /// <param name="deltaTime">経過時間(秒)。</param>
        private void MoveAll(float deltaTime)
        {
            for (int i = bouncingBenefits.Count - 1; i >= 0; i--)
            {
                BouncingBenefit bouncing = bouncingBenefits[i];
                if (bouncing.Rect == null)
                {
                    bouncingBenefits.RemoveAt(i);
                    continue;
                }

                Move(bouncing, deltaTime);
            }
        }

        /// <summary>
        /// 1 個の「ご利益」を動かす。
        /// </summary>
        /// <param name="bouncing">動かす「ご利益」。</param>
        /// <param name="deltaTime">経過時間(秒)。</param>
        private void Move(BouncingBenefit bouncing, float deltaTime)
        {
            Rect screenRect = GetScreenRectInParent(bouncing.Rect);
            bouncing.Velocity += Vector2.down * (screenRect.height * GravityRatio * deltaTime);

            Vector2 position = (Vector2)bouncing.Rect.localPosition + bouncing.Velocity * deltaTime;
            Rect visualExtents = GetScaledVisualExtents(bouncing.Rect);

            float velocityX = bouncing.Velocity.x;
            position.x = ReflectWithinRange(
                position.x,
                screenRect.xMin - visualExtents.xMin,
                screenRect.xMax - visualExtents.xMax,
                screenRect.center.x - visualExtents.center.x,
                ref velocityX);

            float velocityY = bouncing.Velocity.y;
            position.y = ReflectWithinRange(
                position.y,
                screenRect.yMin - visualExtents.yMin,
                screenRect.yMax - visualExtents.yMax,
                screenRect.center.y - visualExtents.center.y,
                ref velocityY);

            bouncing.Velocity = new Vector2(velocityX, velocityY);
            bouncing.Rect.localPosition = new Vector3(position.x, position.y, bouncing.Rect.localPosition.z);
        }

        /// <summary>
        /// 1 軸分の位置を範囲内に収め、範囲外に出ていたら速度を反転する。
        /// </summary>
        /// <param name="position">移動後の位置。</param>
        /// <param name="minimum">範囲の下限。</param>
        /// <param name="maximum">範囲の上限。</param>
        /// <param name="center">範囲が描画範囲より狭い時に置く位置。</param>
        /// <param name="velocity">この軸の速度。反射すると反転し、範囲が狭すぎると 0 になる。</param>
        /// <returns>範囲内に収めた位置。</returns>
        private static float ReflectWithinRange(float position, float minimum, float maximum, float center, ref float velocity)
        {
            if (minimum > maximum)
            {
                velocity = 0f;
                return center;
            }

            if (position < minimum || position > maximum)
            {
                velocity = -velocity;
                return Mathf.Clamp(position, minimum, maximum);
            }

            return position;
        }

        /// <summary>
        /// カメラに映っている画面の範囲を、対象の親のローカル座標で求める。
        /// </summary>
        /// <param name="target">対象の「ご利益」。</param>
        /// <returns>親のローカル座標での画面の範囲。</returns>
        private Rect GetScreenRectInParent(RectTransform target)
        {
            RectTransform parent = (RectTransform)target.parent;
            Camera eventCamera = GetCanvasCamera(target);
            Rect screenRect = gameplayCamera.pixelRect;

            Vector2 minimum;
            Vector2 maximum;
            bool isMinimumConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenRect.min, eventCamera, out minimum);
            bool isMaximumConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenRect.max, eventCamera, out maximum);
            if (!isMinimumConverted || !isMaximumConverted)
            {
                return parent.rect;
            }

            return Rect.MinMaxRect(minimum.x, minimum.y, maximum.x, maximum.y);
        }

        /// <summary>
        /// 対象が属する Canvas の描画カメラを返す。Screen Space - Overlay なら null。
        /// </summary>
        /// <param name="target">対象。</param>
        /// <returns>描画カメラ。</returns>
        private static Camera GetCanvasCamera(RectTransform target)
        {
            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return null;
            }

            Canvas rootCanvas = canvas.rootCanvas;
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return rootCanvas.worldCamera;
        }

        /// <summary>
        /// 実際に描画される範囲を、対象の位置を原点とした拡大率込みの矩形で求める。
        /// </summary>
        /// <param name="target">対象の「ご利益」。</param>
        /// <returns>対象の localPosition からの相対範囲(親のローカル単位)。</returns>
        /// <remarks>
        /// 「ご利益」はフォントサイズに対して RectTransform の枠が小さく、文字が枠から大きくはみ出す。
        /// 枠の大きさで反射させると文字が画面外へ出るため、テキストなら実際の文字の範囲を使う。
        /// 文字の範囲がまだ計算されていない時は、枠の大きさを使う。
        /// </remarks>
        private static Rect GetScaledVisualExtents(RectTransform target)
        {
            Rect localExtents = target.rect;

            TMP_Text text = target.GetComponent<TMP_Text>();
            if (text != null)
            {
                Bounds textBounds = text.textBounds;
                if (textBounds.size.x > 0f && textBounds.size.y > 0f)
                {
                    localExtents = Rect.MinMaxRect(textBounds.min.x, textBounds.min.y, textBounds.max.x, textBounds.max.y);
                }
            }

            Vector3 scale = target.localScale;
            float scaleX = Mathf.Abs(scale.x);
            float scaleY = Mathf.Abs(scale.y);
            return Rect.MinMaxRect(
                localExtents.xMin * scaleX,
                localExtents.yMin * scaleY,
                localExtents.xMax * scaleX,
                localExtents.yMax * scaleY);
        }

        /// <summary>
        /// 飛び出させた「ご利益」をすべて破棄する。
        /// </summary>
        private void DestroyAll()
        {
            foreach (BouncingBenefit bouncing in bouncingBenefits)
            {
                if (bouncing.Rect != null)
                {
                    Destroy(bouncing.Rect.gameObject);
                }
            }

            bouncingBenefits.Clear();
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(scoreController, nameof(scoreController), this);
            isValid &= RequiredReference.IsAssigned(settings, nameof(settings), this);
            isValid &= RequiredReference.IsAssigned(gameplayCamera, nameof(gameplayCamera), this);
            isValid &= RequiredReference.IsAssigned(benefitTemplate, nameof(benefitTemplate), this);
            isValid &= RequiredReference.IsAssigned(movementBounds, nameof(movementBounds), this);
            return isValid;
        }
    }
}
