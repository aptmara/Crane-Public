using Crane.Hammer;
using Crane.Setup;
using NUnit.Framework;
using UnityEngine;

namespace Crane.Tests.EditMode
{
    /// <summary>
    /// <see cref="CranePoseModel"/> の操作ルールのテスト。
    /// </summary>
    public class CranePoseModelTests
    {
        /// <summary>座標の比較の許容誤差。</summary>
        private const float Tolerance = 0.001f;

        /// <summary>
        /// 第一リンクの先端を動かすと、第二リンクは形を保ったまま一緒に動く。
        /// </summary>
        [Test]
        public void MoveUpperLinkTip_MovesLowerLinkRigidly()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());
            Vector2 lowerLinkBefore = model.HammerPosition - model.LowerLinkPivot;

            model.MoveUpperLinkTip(new Vector2(50f, -80f));

            AssertVector(new Vector2(50f, -80f), model.LowerLinkPivot);
            AssertVector(lowerLinkBefore, model.HammerPosition - model.LowerLinkPivot);
        }

        /// <summary>
        /// 第二リンクの先端はポインター位置へ動く。
        /// </summary>
        [Test]
        public void MoveLowerLinkTip_MovesHammerToPointer()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());

            model.MoveLowerLinkTip(new Vector2(0f, -250f));

            AssertVector(new Vector2(0f, -250f), model.HammerPosition);
        }

        /// <summary>
        /// リンクは最小長より短くならず、同じ方向で最小長の位置まで押し出される。
        /// </summary>
        [Test]
        public void MoveLowerLinkTip_KeepsMinimumLength()
        {
            CranePoseSettings settings = CreateSettings();
            settings.MinimumLinkLength = 20f;
            CranePoseModel model = new CranePoseModel(settings);

            model.MoveLowerLinkTip(model.LowerLinkPivot + new Vector2(0f, -5f));

            AssertVector(model.LowerLinkPivot + new Vector2(0f, -20f), model.HammerPosition);
        }

        /// <summary>
        /// ポインターが回転軸より左ならブームは反時計回りに回り、先の振り子も一緒に動く。
        /// </summary>
        [Test]
        public void RotateBoomToward_PointerOnLeft_RotatesCounterClockwise()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());
            float angleBefore = model.BoomAngleDeg;
            Vector2 upperLinkBefore = model.LowerLinkPivot - model.UpperLinkPivot;

            model.RotateBoomToward(model.BoomPivot.x - 1f, 0.1f);

            Assert.AreEqual(angleBefore + 4.5f, model.BoomAngleDeg, Tolerance);
            AssertVector(upperLinkBefore, model.LowerLinkPivot - model.UpperLinkPivot);
        }

        /// <summary>
        /// ブームの角度は上限を超えない。
        /// </summary>
        [Test]
        public void RotateBoomToward_ClampsToMaximumAngle()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());

            model.RotateBoomToward(model.BoomPivot.x - 1f, 100f);

            Assert.AreEqual(150f, model.BoomAngleDeg, Tolerance);
        }

        /// <summary>
        /// 相対角の上限を有効にすると、第二リンクは第一リンクに対して上限角までしか開かない。
        /// </summary>
        [Test]
        public void MoveLowerLinkTip_WithRelativeAngleLimit_ClampsAngle()
        {
            CranePoseSettings settings = CreateSettings();
            settings.UseLowerLinkRelativeAngleLimit = true;
            settings.LowerLinkMaxRelativeAngleDeg = 20f;
            CranePoseModel model = new CranePoseModel(settings);

            model.MoveLowerLinkTip(model.LowerLinkPivot + new Vector2(0f, 100f));

            float relativeAngle = Mathf.DeltaAngle(model.UpperLinkAngleDeg, model.LowerLinkAngleDeg);
            Assert.AreEqual(20f, relativeAngle, Tolerance);
            Assert.AreEqual(100f, Vector2.Distance(model.LowerLinkPivot, model.HammerPosition), Tolerance);
        }

        /// <summary>
        /// 操作判定の中にある部位を見つけ、どれにも入っていなければ None を返す。
        /// </summary>
        [Test]
        public void FindPartAt_ReturnsPartUnderPointer()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());

            Assert.AreEqual(CranePosePart.LowerLink, model.FindPartAt(model.HammerPosition));
            Assert.AreEqual(CranePosePart.UpperLink, model.FindPartAt(model.LowerLinkPivot));
            Assert.AreEqual(CranePosePart.Boom, model.FindPartAt((model.BoomPivot + model.UpperLinkPivot) * 0.5f));
            Assert.AreEqual(CranePosePart.None, model.FindPartAt(new Vector2(900f, 900f)));
        }

        /// <summary>
        /// スナップショットには現在の姿勢と選択したハンマーが入る。
        /// </summary>
        [Test]
        public void CreateSnapshot_CapturesCurrentPose()
        {
            CranePoseModel model = new CranePoseModel(CreateSettings());
            model.MoveLowerLinkTip(new Vector2(10f, -300f));

            HammerDefinition hammer = ScriptableObject.CreateInstance<HammerDefinition>();
            CraneSetupSnapshot snapshot = model.CreateSnapshot(hammer);

            AssertVector(model.UpperLinkPivot, snapshot.UpperLinkPivot);
            AssertVector(model.LowerLinkPivot, snapshot.LowerLinkPivot);
            AssertVector(model.HammerPosition, snapshot.HammerPosition);
            Assert.AreSame(hammer, snapshot.Hammer);

            Object.DestroyImmediate(hammer);
        }

        /// <summary>
        /// テスト用の設定を作る。
        /// </summary>
        /// <returns>
        /// ブームは原点から 135 度方向へ長さ 200、第一リンクは真下へ 100、第二リンクは右へ 100 の姿勢。
        /// ブームの回転速度は 45 度/秒、角度範囲は 120〜150 度。
        /// </returns>
        private static CranePoseSettings CreateSettings()
        {
            Vector2 upperLinkPivot = new Vector2(-141.42136f, 141.42136f);
            Vector2 lowerLinkPivot = upperLinkPivot + new Vector2(0f, -100f);

            CranePoseSettings settings = new CranePoseSettings();
            settings.BoomPivot = Vector2.zero;
            settings.UpperLinkPivot = upperLinkPivot;
            settings.LowerLinkPivot = lowerLinkPivot;
            settings.HammerPosition = lowerLinkPivot + new Vector2(100f, 0f);
            settings.BoomMinAngleDeg = 120f;
            settings.BoomMaxAngleDeg = 150f;
            settings.BoomMaxAngularSpeedDeg = 45f;
            settings.BoomHitThickness = 40f;
            settings.UpperLinkHandleSize = new Vector2(30f, 30f);
            settings.LowerLinkHandleSize = new Vector2(30f, 30f);
            settings.MinimumLinkLength = 1f;
            settings.UseLowerLinkRelativeAngleLimit = false;
            settings.LowerLinkMaxRelativeAngleDeg = 21.961f;
            return settings;
        }

        /// <summary>
        /// 2 つのベクトルが許容誤差内で等しいことを確認する。
        /// </summary>
        /// <param name="expected">期待値。</param>
        /// <param name="actual">実際の値。</param>
        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
        }
    }
}
