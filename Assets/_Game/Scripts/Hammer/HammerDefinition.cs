using UnityEngine;

namespace Crane.Hammer
{
    /// <summary>
    /// 振り子の先端に付けるハンマー(金槌・木槌)の定義。
    /// </summary>
    /// <remarks>
    /// 質量は振り子の運動そのものを変えるため、見た目やダメージだけの違いではない。
    /// </remarks>
    [CreateAssetMenu(fileName = "HammerDefinition", menuName = "Crane/Hammer Definition")]
    public class HammerDefinition : ScriptableObject
    {
        /// <summary>Setup から物理側へ受け渡す識別子。</summary>
        [Tooltip("Setup から物理側へ受け渡す識別子。")]
        public string Id;

        /// <summary>表示名。</summary>
        [Tooltip("表示名。")]
        public string DisplayName;

        /// <summary>先端(第二リンク)の質量。</summary>
        [Header("Physics")]
        [Tooltip("先端(第二リンク)の質量。")]
        public float Mass = 1f;

        /// <summary>ヒットの威力に掛ける倍率。</summary>
        [Header("Damage")]
        [Tooltip("ヒットの威力に掛ける倍率。")]
        public float DamageBonus = 1f;

        /// <summary>ハンマーの画像。</summary>
        [Header("Presentation")]
        [Tooltip("ハンマーの画像。")]
        public Sprite Sprite;

        /// <summary>鐘に当たった時の効果音。</summary>
        [Tooltip("鐘に当たった時の効果音。")]
        public AudioClip HitSound;
    }
}
