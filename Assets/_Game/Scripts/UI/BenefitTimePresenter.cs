using Crane.Common;
using Crane.Game;
using Crane.Presentation;
using UnityEngine;

namespace Crane.UI
{
    /// <summary>
    /// スーパーご利益タイムの帯・BGM・背景の点滅を行う。
    /// </summary>
    /// <remarks>
    /// 煩悩を祓い切った後、夜から明るい年明けへ雰囲気を大きく変える演出。
    /// <list type="bullet">
    /// <item><description>「スーパーご利益タイム」の帯が右から入って中央で止まり、左へ抜ける。</description></item>
    /// <item><description>BGM を流し、その拍に合わせて背景を黄色と白で交互に点滅させる。</description></item>
    /// </list>
    /// ヒットのたびに飛び出す「ご利益」は <see cref="BouncingBenefitSpawner"/> が担当する。
    /// </remarks>
    public class BenefitTimePresenter : MonoBehaviour
    {
        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>背景の色を変える担当。</summary>
        [SerializeField] private BackgroundTint backgroundTint;

        /// <summary>「スーパーご利益タイム」の帯。</summary>
        [SerializeField] private RectTransform superBenefitBanner;

        /// <summary>帯の出入りの距離の基準にする範囲。</summary>
        [SerializeField] private RectTransform movementBounds;

        /// <summary>スーパーご利益タイムの BGM。</summary>
        [SerializeField] private AudioClip benefitMusic;

        /// <summary>BGM のテンポ(BPM)。背景の点滅をこの拍に合わせる。</summary>
        [SerializeField] private float benefitMusicBeatsPerMinute = 130f;

        /// <summary>帯が右から中央へ入るのにかける時間(秒)。</summary>
        [SerializeField] private float bannerEnterSeconds = 0.9f;

        /// <summary>帯が中央で止まる時間(秒)。</summary>
        [SerializeField] private float bannerCenterSeconds = 0.45f;

        /// <summary>帯が中央から左へ抜けるのにかける時間(秒)。</summary>
        [SerializeField] private float bannerExitSeconds = 0.9f;

        /// <summary>背景の点滅色。</summary>
        [SerializeField] private Color beatColor = Color.yellow;

        /// <summary>帯の表示を切り替える CanvasGroup。</summary>
        private CanvasGroup bannerCanvasGroup;

        /// <summary>BGM を流す AudioSource。</summary>
        private AudioSource musicSource;

        /// <summary>帯が中央に止まる時の位置。</summary>
        private Vector2 bannerCenterPosition;

        /// <summary>帯のアニメーション開始からの経過時間(秒)。</summary>
        private float bannerElapsedSeconds;

        /// <summary>前回の拍からの経過時間(秒)。</summary>
        private float beatElapsedSeconds;

        /// <summary>背景が点滅色なら true、白なら false。</summary>
        private bool isBeatColorShown;

        /// <summary>スーパーご利益タイムの演出中なら true。</summary>
        private bool isActive;

        /// <summary>
        /// 必須参照を検証し、BGM 用の AudioSource を用意して、帯を隠す。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;

            bannerCanvasGroup = CanvasGroupUtility.GetOrAdd(superBenefitBanner.gameObject);
            bannerCenterPosition = superBenefitBanner.anchoredPosition;
            bannerCanvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 状態の変化を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// 購読を解除し、演出を止める。
        /// </summary>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
            End();
        }

        /// <summary>
        /// 演出中なら、帯と背景の点滅を更新する。
        /// </summary>
        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            UpdateBanner(deltaTime);
            UpdateBeat(deltaTime);
        }

        /// <summary>
        /// スーパーご利益タイムに入ったら演出を始め、抜けたら止める。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.BenefitTime)
            {
                Begin();
                return;
            }

            End();
        }

        /// <summary>
        /// 演出を始める。
        /// </summary>
        private void Begin()
        {
            isActive = true;
            bannerElapsedSeconds = 0f;
            beatElapsedSeconds = 0f;
            isBeatColorShown = true;

            bannerCanvasGroup.alpha = 1f;
            superBenefitBanner.anchoredPosition = GetBannerRightPosition();
            backgroundTint.Apply(beatColor);

            if (benefitMusic != null)
            {
                musicSource.clip = benefitMusic;
                musicSource.Play();
            }
        }

        /// <summary>
        /// 演出を止めて元の見た目へ戻す。演出中でなければ何もしない。
        /// </summary>
        /// <remarks>
        /// 演出中でない時に背景を戻すと、結果画面が設定した背景色を上書きしてしまうため、何もしない。
        /// </remarks>
        private void End()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;
            bannerCanvasGroup.alpha = 0f;
            backgroundTint.Restore();
            musicSource.Stop();
        }

        /// <summary>
        /// 帯を「右から入る → 中央で止まる → 左へ抜ける → 消える」の順に動かす。
        /// </summary>
        /// <param name="deltaTime">経過時間(秒)。</param>
        private void UpdateBanner(float deltaTime)
        {
            bannerElapsedSeconds += deltaTime;

            float enterSeconds = Mathf.Max(bannerEnterSeconds, Mathf.Epsilon);
            float centerSeconds = Mathf.Max(0f, bannerCenterSeconds);
            float exitSeconds = Mathf.Max(bannerExitSeconds, Mathf.Epsilon);

            if (bannerElapsedSeconds < enterSeconds)
            {
                float progress = SmoothStep(bannerElapsedSeconds / enterSeconds);
                superBenefitBanner.anchoredPosition = Vector2.LerpUnclamped(GetBannerRightPosition(), bannerCenterPosition, progress);
                return;
            }

            if (bannerElapsedSeconds < enterSeconds + centerSeconds)
            {
                superBenefitBanner.anchoredPosition = bannerCenterPosition;
                return;
            }

            float exitElapsed = bannerElapsedSeconds - enterSeconds - centerSeconds;
            if (exitElapsed < exitSeconds)
            {
                float progress = SmoothStep(exitElapsed / exitSeconds);
                superBenefitBanner.anchoredPosition = Vector2.LerpUnclamped(bannerCenterPosition, GetBannerLeftPosition(), progress);
                return;
            }

            bannerCanvasGroup.alpha = 0f;
        }

        /// <summary>
        /// BGM の拍ごとに背景を点滅色と白で切り替える。
        /// </summary>
        /// <param name="deltaTime">経過時間(秒)。</param>
        /// <remarks>
        /// フレームが大きく飛んで複数拍を一度に過ぎた場合も、拍数の偶奇で色を決めて拍とずれないようにする。
        /// </remarks>
        private void UpdateBeat(float deltaTime)
        {
            float secondsPerBeat = 60f / benefitMusicBeatsPerMinute;
            beatElapsedSeconds += deltaTime;
            if (beatElapsedSeconds < secondsPerBeat)
            {
                return;
            }

            int elapsedBeats = Mathf.FloorToInt(beatElapsedSeconds / secondsPerBeat);
            beatElapsedSeconds -= elapsedBeats * secondsPerBeat;

            bool isOddBeatCount = elapsedBeats % 2 != 0;
            if (isOddBeatCount)
            {
                isBeatColorShown = !isBeatColorShown;
            }

            if (isBeatColorShown)
            {
                backgroundTint.Apply(beatColor);
            }
            else
            {
                backgroundTint.Apply(Color.white);
            }
        }

        /// <summary>帯が入ってくる前の、右の画面外の位置。</summary>
        /// <returns>anchoredPosition。</returns>
        private Vector2 GetBannerRightPosition()
        {
            return bannerCenterPosition + Vector2.right * movementBounds.rect.width;
        }

        /// <summary>帯が抜けた後の、左の画面外の位置。</summary>
        /// <returns>anchoredPosition。</returns>
        private Vector2 GetBannerLeftPosition()
        {
            return bannerCenterPosition + Vector2.left * movementBounds.rect.width;
        }

        /// <summary>
        /// 始まりと終わりが滑らかな補間係数にする。
        /// </summary>
        /// <param name="value">0〜1 の進行度。範囲外は丸める。</param>
        /// <returns>補間係数。</returns>
        private static float SmoothStep(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(backgroundTint, nameof(backgroundTint), this);
            isValid &= RequiredReference.IsAssigned(superBenefitBanner, nameof(superBenefitBanner), this);
            isValid &= RequiredReference.IsAssigned(movementBounds, nameof(movementBounds), this);
            return isValid;
        }
    }
}
