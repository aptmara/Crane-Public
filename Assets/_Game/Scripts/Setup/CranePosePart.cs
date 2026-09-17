namespace Crane.Setup
{
    /// <summary>
    /// Setup 画面でドラッグ操作できる部位。
    /// </summary>
    public enum CranePosePart
    {
        /// <summary>どの部位も操作していない。</summary>
        None,

        /// <summary>ブーム。ドラッグ中、ポインターのある側へ一定速度で回転する。</summary>
        Boom,

        /// <summary>第一リンクの先端。第二リンクごと移動する。</summary>
        UpperLink,

        /// <summary>第二リンクの先端(ハンマー)。</summary>
        LowerLink,
    }
}
