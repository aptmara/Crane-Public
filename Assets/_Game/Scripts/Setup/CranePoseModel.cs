using Crane.Hammer;
using UnityEngine;

namespace Crane.Setup
{
    /// <summary>
    /// Setup 画面のクレーン姿勢(4 つの点)と、その操作ルール。
    /// </summary>
    /// <remarks>
    /// 表示や入力イベントから切り離した純粋な C# クラスで、EditMode テストで検証できる。
    /// 座標はすべて Setup Canvas のローカル座標(px)。
    /// <code>
    ///   BoomPivot ──ブーム── UpperLinkPivot ──第一リンク── LowerLinkPivot ──第二リンク── HammerPosition
    /// </code>
    /// 操作ルールは次の通り。
    /// <list type="bullet">
    /// <item><description>ブーム: ドラッグ中、ポインターが回転軸より左なら反時計回り、右なら時計回りに一定速度で回る。角度は範囲内に制限し、先の振り子は形を保ったまま一緒に動く。</description></item>
    /// <item><description>第一リンク: 先端をポインター位置へ動かす。第二リンクは形を保ったまま一緒に動く。</description></item>
    /// <item><description>第二リンク: 先端(ハンマー)をポインター位置へ動かす。</description></item>
    /// <item><description>どのリンクも最小長より短くはならない。</description></item>
    /// </list>
    /// </remarks>
    public sealed class CranePoseModel
    {
        /// <summary>方向を求められないとみなす長さの二乗。</summary>
        private const float DegenerateSqrLength = 0.0001f;

        /// <summary>初期配置と操作制約。</summary>
        private readonly CranePoseSettings settings;

        /// <summary>ブームの長さ。ブーム回転中も変わらない。</summary>
        private readonly float boomLength;

        /// <summary>ブームの回転軸。</summary>
        public Vector2 BoomPivot { get; private set; }

        /// <summary>振り子全体の支点(ブームの先端、第一リンクの根元)。</summary>
        public Vector2 UpperLinkPivot { get; private set; }

        /// <summary>第一リンクと第二リンクの関節。</summary>
        public Vector2 LowerLinkPivot { get; private set; }

        /// <summary>第二リンクの先端(ハンマーの位置)。</summary>
        public Vector2 HammerPosition { get; private set; }

        /// <summary>ブームの角度(度)。</summary>
        public float BoomAngleDeg
        {
            get
            {
                return GetAngleDeg(UpperLinkPivot - BoomPivot);
            }
        }

        /// <summary>第一リンクの角度(度)。</summary>
        public float UpperLinkAngleDeg
        {
            get
            {
                return GetAngleDeg(LowerLinkPivot - UpperLinkPivot);
            }
        }

        /// <summary>第二リンクの角度(度)。</summary>
        public float LowerLinkAngleDeg
        {
            get
            {
                return GetAngleDeg(HammerPosition - LowerLinkPivot);
            }
        }

        /// <summary>
        /// 設定の初期配置でモデルを作る。
        /// </summary>
        /// <param name="settings">初期配置と操作制約。</param>
        public CranePoseModel(CranePoseSettings settings)
        {
            this.settings = settings;
            BoomPivot = settings.BoomPivot;
            UpperLinkPivot = settings.UpperLinkPivot;
            LowerLinkPivot = settings.LowerLinkPivot;
            HammerPosition = settings.HammerPosition;
            boomLength = Vector2.Distance(BoomPivot, UpperLinkPivot);
        }

        /// <summary>
        /// ポインター位置で操作を開始する部位を探す。
        /// </summary>
        /// <param name="pointer">ポインター位置。</param>
        /// <returns>判定内にある部位のうち、判定の中心に最も近いもの。なければ <see cref="CranePosePart.None"/>。</returns>
        public CranePosePart FindPartAt(Vector2 pointer)
        {
            CranePosePart nearestPart = CranePosePart.None;
            float nearestSqrDistance = float.PositiveInfinity;

            if (IsInsideBar(pointer, BoomPivot, UpperLinkPivot, settings.BoomHitThickness))
            {
                Vector2 boomCenter = (BoomPivot + UpperLinkPivot) * 0.5f;
                float sqrDistance = (pointer - boomCenter).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestPart = CranePosePart.Boom;
                    nearestSqrDistance = sqrDistance;
                }
            }

            if (IsInsideBox(pointer, LowerLinkPivot, settings.UpperLinkHandleSize))
            {
                float sqrDistance = (pointer - LowerLinkPivot).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestPart = CranePosePart.UpperLink;
                    nearestSqrDistance = sqrDistance;
                }
            }

            if (IsInsideBox(pointer, HammerPosition, settings.LowerLinkHandleSize))
            {
                float sqrDistance = (pointer - HammerPosition).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestPart = CranePosePart.LowerLink;
                }
            }

            return nearestPart;
        }

        /// <summary>
        /// ブームをポインターのある側へ 1 フレーム分回転させる。
        /// </summary>
        /// <param name="pointerX">ポインターの X 座標。</param>
        /// <param name="deltaSeconds">経過時間(秒)。</param>
        public void RotateBoomToward(float pointerX, float deltaSeconds)
        {
            float step = settings.BoomMaxAngularSpeedDeg * deltaSeconds;
            float angle = BoomAngleDeg;

            if (pointerX < BoomPivot.x)
            {
                angle = Mathf.Clamp(angle + step, settings.BoomMinAngleDeg, settings.BoomMaxAngleDeg);
            }
            else if (pointerX > BoomPivot.x)
            {
                angle = Mathf.Clamp(angle - step, settings.BoomMinAngleDeg, settings.BoomMaxAngleDeg);
            }

            Vector2 nextUpperLinkPivot = BoomPivot + GetDirection(angle) * boomLength;
            Vector2 movement = nextUpperLinkPivot - UpperLinkPivot;
            UpperLinkPivot = nextUpperLinkPivot;
            LowerLinkPivot += movement;
            HammerPosition += movement;
        }

        /// <summary>
        /// 第一リンクの先端を動かす。第二リンクは形を保ったまま一緒に動く。
        /// </summary>
        /// <param name="pointer">ポインター位置。</param>
        public void MoveUpperLinkTip(Vector2 pointer)
        {
            Vector2 nextLowerLinkPivot = KeepMinimumLength(UpperLinkPivot, pointer, LowerLinkPivot);
            Vector2 movement = nextLowerLinkPivot - LowerLinkPivot;
            LowerLinkPivot = nextLowerLinkPivot;
            HammerPosition += movement;
            ApplyLowerLinkRelativeAngleLimit();
        }

        /// <summary>
        /// 第二リンクの先端(ハンマー)を動かす。
        /// </summary>
        /// <param name="pointer">ポインター位置。</param>
        public void MoveLowerLinkTip(Vector2 pointer)
        {
            HammerPosition = KeepMinimumLength(LowerLinkPivot, pointer, HammerPosition);
            ApplyLowerLinkRelativeAngleLimit();
        }

        /// <summary>
        /// 現在の姿勢を初期条件として確定する。
        /// </summary>
        /// <param name="hammer">選択中のハンマー。</param>
        /// <returns>物理シミュレーションへ渡す初期姿勢。</returns>
        public CraneSetupSnapshot CreateSnapshot(HammerDefinition hammer)
        {
            return new CraneSetupSnapshot(UpperLinkPivot, LowerLinkPivot, HammerPosition, hammer);
        }

        /// <summary>
        /// 第二リンクの相対角(第二リンクの角度 - 第一リンクの角度)が上限を超えていれば、上限の角度まで戻す。
        /// </summary>
        /// <remarks>
        /// 原作で実測できているのは上限側だけなので、下限は制限しない。
        /// </remarks>
        private void ApplyLowerLinkRelativeAngleLimit()
        {
            if (!settings.UseLowerLinkRelativeAngleLimit)
            {
                return;
            }

            float upperAngle = UpperLinkAngleDeg;
            float relativeAngle = Mathf.DeltaAngle(upperAngle, LowerLinkAngleDeg);
            if (relativeAngle <= settings.LowerLinkMaxRelativeAngleDeg)
            {
                return;
            }

            float lowerLinkLength = Vector2.Distance(LowerLinkPivot, HammerPosition);
            float limitedAngle = upperAngle + settings.LowerLinkMaxRelativeAngleDeg;
            HammerPosition = LowerLinkPivot + GetDirection(limitedAngle) * lowerLinkLength;
        }

        /// <summary>
        /// 根元から要求位置までが最小長より短ければ、同じ方向で最小長の位置まで押し出す。
        /// </summary>
        /// <param name="root">リンクの根元。</param>
        /// <param name="requested">要求された先端位置。</param>
        /// <param name="current">現在の先端位置。方向が決まらない時はこの位置を保つ。</param>
        /// <returns>最小長を満たす先端位置。</returns>
        private Vector2 KeepMinimumLength(Vector2 root, Vector2 requested, Vector2 current)
        {
            Vector2 offset = requested - root;
            if (offset.magnitude >= settings.MinimumLinkLength)
            {
                return requested;
            }

            if (offset.sqrMagnitude < DegenerateSqrLength)
            {
                return current;
            }

            return root + offset.normalized * settings.MinimumLinkLength;
        }

        /// <summary>
        /// 点が、線分を中心とした太さ付きの棒の内側にあるかを判定する。
        /// </summary>
        /// <param name="point">判定する点。</param>
        /// <param name="start">棒の始点。</param>
        /// <param name="end">棒の終点。</param>
        /// <param name="thickness">棒の太さ。</param>
        /// <returns>内側なら true。</returns>
        private static bool IsInsideBar(Vector2 point, Vector2 start, Vector2 end, float thickness)
        {
            Vector2 segment = end - start;
            float segmentLength = segment.magnitude;
            if (segmentLength <= Mathf.Epsilon)
            {
                return false;
            }

            Vector2 direction = segment / segmentLength;
            float distanceAlong = Vector2.Dot(point - start, direction);
            if (distanceAlong < 0f || distanceAlong > segmentLength)
            {
                return false;
            }

            Vector2 closestPoint = start + direction * distanceAlong;
            float distanceAcross = Vector2.Distance(point, closestPoint);
            return distanceAcross <= thickness * 0.5f;
        }

        /// <summary>
        /// 点が、中心と大きさで表した軸平行の矩形の内側にあるかを判定する。
        /// </summary>
        /// <param name="point">判定する点。</param>
        /// <param name="center">矩形の中心。</param>
        /// <param name="size">矩形の大きさ。</param>
        /// <returns>内側なら true。</returns>
        private static bool IsInsideBox(Vector2 point, Vector2 center, Vector2 size)
        {
            Vector2 offset = point - center;
            bool isInsideX = Mathf.Abs(offset.x) <= size.x * 0.5f;
            bool isInsideY = Mathf.Abs(offset.y) <= size.y * 0.5f;
            return isInsideX && isInsideY;
        }

        /// <summary>
        /// ベクトルの向きを角度にする。
        /// </summary>
        /// <param name="direction">向き。</param>
        /// <returns>X 軸正方向を 0 とする反時計回りの角度(度)。</returns>
        private static float GetAngleDeg(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 角度から単位ベクトルを作る。
        /// </summary>
        /// <param name="angleDeg">X 軸正方向を 0 とする反時計回りの角度(度)。</param>
        /// <returns>長さ 1 の向き。</returns>
        private static Vector2 GetDirection(float angleDeg)
        {
            float radians = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
