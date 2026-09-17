using Crane.Common;
using UnityEngine;

namespace Crane.Bell
{
    /// <summary>
    /// 鐘へのヒット時に、ハンマーごとの効果音を鳴らす。
    /// </summary>
    /// <remarks>
    /// 効果音はスコア計測中かどうかに関係なく、物理的に当たれば必ず鳴らす。
    /// </remarks>
    [RequireComponent(typeof(AudioSource))]
    public class BellHitAudioPlayer : MonoBehaviour
    {
        /// <summary>ヒットを通知する検出器。</summary>
        [SerializeField] private BellHitDetector bellHitDetector;

        /// <summary>効果音を鳴らす AudioSource。</summary>
        private AudioSource audioSource;

        /// <summary>
        /// 必須参照を検証し、AudioSource を 2D の効果音用に設定する。
        /// </summary>
        private void Awake()
        {
            if (!RequiredReference.IsAssigned(bellHitDetector, nameof(bellHitDetector), this))
            {
                enabled = false;
                return;
            }

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        /// <summary>
        /// ヒットの通知を購読する。
        /// </summary>
        private void OnEnable()
        {
            bellHitDetector.Hit += HandleHit;
        }

        /// <summary>
        /// ヒットの通知の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            bellHitDetector.Hit -= HandleHit;
        }

        /// <summary>
        /// ハンマーの効果音を鳴らす。
        /// </summary>
        /// <param name="hitData">ヒット情報。</param>
        private void HandleHit(BellHitData hitData)
        {
            if (hitData.HitSound == null)
            {
                return;
            }

            audioSource.PlayOneShot(hitData.HitSound);
        }
    }
}
