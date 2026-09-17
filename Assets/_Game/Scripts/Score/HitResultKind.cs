namespace Crane.Score
{
    /// <summary>
    /// 1 回のヒットが何を増減させたか。
    /// </summary>
    public enum HitResultKind
    {
        /// <summary>残り煩悩を減らした(Playing 中のヒット)。</summary>
        Bonno,

        /// <summary>ご利益を増やした(煩悩を祓い切った後のヒット)。</summary>
        Benefit,
    }
}
