using System.Collections;
using System.Collections.Generic;
using Crane.Common;
using Crane.Data;
using Crane.Presentation;
using Crane.Score;
using UnityEngine;

namespace Crane.Bell
{
    /// <summary>
    /// 鐘へのヒット時に、エフェクト画像と「○煩悩」「○ご利益」の数値を表示する。
    /// </summary>
    /// <remarks>
    /// 表示内容はスコアへ反映済みの <see cref="HitResult"/> から決め、
    /// 煩悩とご利益のどちらかといった判断はここでは行わない。
    /// </remarks>
    public class HitEffectPresenter : MonoBehaviour
    {
        /// <summary>エフェクト画像を表示する時間(秒)。</summary>
        private const float EffectDisplaySeconds = 0.15f;

        /// <summary>エフェクト画像の向きの補正(度)。画像が斜め 135 度方向を向いて描かれているため。</summary>
        private const float EffectDirectionOffsetDeg = 135f;

        /// <summary>エフェクトの大きさと数値テキストの設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>エフェクト画像。</summary>
        [SerializeField] private Sprite effectSprite;

        /// <summary>数値テキストのフォント。未設定なら数値を表示しない。</summary>
        [SerializeField] private Font hitTextFont;

        /// <summary>エフェクト画像の回転と拡大の中心。鐘の中心に置く。</summary>
        private Transform effectPivot;

        /// <summary>エフェクト画像の SpriteRenderer。</summary>
        private SpriteRenderer effectRenderer;

        /// <summary>表示中の数値テキスト。無効化時にまとめて破棄する。</summary>
        private readonly List<GameObject> activeTexts = new List<GameObject>();

        /// <summary>
        /// 必須参照を検証し、エフェクト画像を非表示の状態で用意する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            GameObject pivotObject = new GameObject("HitEffectPivot");
            pivotObject.transform.SetParent(transform, false);
            effectPivot = pivotObject.transform;

            GameObject visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(effectPivot, false);
            visualObject.transform.localPosition = -effectSprite.bounds.center;

            effectRenderer = visualObject.AddComponent<SpriteRenderer>();
            effectRenderer.sprite = effectSprite;
            effectRenderer.sortingOrder = SortingOrders.HitEffect;
            effectRenderer.enabled = false;
        }

        /// <summary>
        /// 無効化時に、表示中の演出をすべて消す。
        /// </summary>
        private void OnDisable()
        {
            StopAllCoroutines();
            CancelInvoke();

            if (effectRenderer != null)
            {
                effectRenderer.enabled = false;
            }

            foreach (GameObject text in activeTexts)
            {
                if (text != null)
                {
                    Destroy(text);
                }
            }

            activeTexts.Clear();
        }

        /// <summary>
        /// ヒットの演出を鐘の中心に表示する。
        /// </summary>
        /// <param name="hitVelocity">接触点でのハンマーの速度。エフェクトの向きに使う。</param>
        /// <param name="result">スコアへ反映した結果。</param>
        public void Show(Vector3 hitVelocity, HitResult result)
        {
            if (!enabled)
            {
                return;
            }

            float strength = HitStrength.FromPower(result.Power, settings);
            float scale = HitStrength.ToEffectScale(strength, settings);
            Vector3 bellCenter = GetBellCenter();

            effectPivot.position = bellCenter;
            effectPivot.localScale = Vector3.one * scale;
            if (hitVelocity.sqrMagnitude > Mathf.Epsilon)
            {
                float hitAngle = Mathf.Atan2(hitVelocity.y, hitVelocity.x) * Mathf.Rad2Deg;
                effectPivot.rotation = Quaternion.Euler(0f, 0f, hitAngle - EffectDirectionOffsetDeg);
            }

            effectRenderer.enabled = true;
            CancelInvoke(nameof(HideEffect));
            Invoke(nameof(HideEffect), EffectDisplaySeconds);

            if (hitTextFont != null)
            {
                ShowAmountText(bellCenter, scale, result);
            }
        }

        /// <summary>
        /// エフェクト画像を隠す。
        /// </summary>
        private void HideEffect()
        {
            effectRenderer.enabled = false;
        }

        /// <summary>
        /// 鐘の中心を求める。当たり判定があればその中心、なければ自身の位置。
        /// </summary>
        /// <returns>鐘の中心のワールド座標。</returns>
        private Vector3 GetBellCenter()
        {
            Collider bellCollider = GetComponent<Collider>();
            if (bellCollider == null)
            {
                return transform.position;
            }

            return bellCollider.bounds.center;
        }

        /// <summary>
        /// 「○煩悩」「○ご利益」の数値テキストを表示し、上昇させて消す。
        /// </summary>
        /// <param name="position">表示開始位置。</param>
        /// <param name="scale">エフェクトと同じ拡大率。</param>
        /// <param name="result">スコアへ反映した結果。</param>
        private void ShowAmountText(Vector3 position, float scale, HitResult result)
        {
            int labelFontSize = Mathf.Max(1, settings.HitBonnoFontSize);
            int numberFontSize = Mathf.Max(1, Mathf.RoundToInt(labelFontSize * settings.HitBonnoNumberSizeRatio));

            GameObject textObject = new GameObject("HitAmountText");
            textObject.transform.position = position;
            textObject.transform.localScale = Vector3.one * scale;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.font = hitTextFont;
            textMesh.fontSize = labelFontSize;
            textMesh.characterSize = settings.HitBonnoCharacterSize;
            textMesh.richText = true;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.text = "<size=" + numberFontSize + ">" + result.Amount + "</size>"
                + "<size=" + labelFontSize + ">" + GetLabel(result.Kind) + "</size>";
            if (result.Kind == HitResultKind.Benefit)
            {
                textMesh.color = ScoreStyle.BenefitColor;
            }

            MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
            textRenderer.sharedMaterial = hitTextFont.material;
            textRenderer.sortingOrder = SortingOrders.HitText;

            activeTexts.Add(textObject);
            StartCoroutine(RiseAndDestroy(textObject));
        }

        /// <summary>
        /// テキストを一定時間かけて上昇させ、終わったら破棄する。
        /// </summary>
        /// <param name="textObject">対象のテキスト。</param>
        /// <returns>コルーチン。</returns>
        private IEnumerator RiseAndDestroy(GameObject textObject)
        {
            Transform textTransform = textObject.transform;
            Vector3 startPosition = textTransform.position;
            float duration = Mathf.Max(settings.HitBonnoDisplayDurationSeconds, Mathf.Epsilon);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                textTransform.position = startPosition + Vector3.up * (settings.HitBonnoRiseDistance * progress);
                yield return null;
            }

            activeTexts.Remove(textObject);
            Destroy(textObject);
        }

        /// <summary>
        /// ヒット結果の種類に対応するラベルを返す。
        /// </summary>
        /// <param name="kind">ヒット結果の種類。</param>
        /// <returns>「煩悩」または「ご利益」。</returns>
        private static string GetLabel(HitResultKind kind)
        {
            if (kind == HitResultKind.Benefit)
            {
                return ScoreStyle.BenefitLabel;
            }

            return ScoreStyle.BonnoLabel;
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(settings, nameof(settings), this);
            isValid &= RequiredReference.IsAssigned(effectSprite, nameof(effectSprite), this);
            return isValid;
        }
    }
}
