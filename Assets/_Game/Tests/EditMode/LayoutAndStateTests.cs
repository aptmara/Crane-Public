using Crane.Data;
using Crane.Game;
using Crane.PhysicsSim;
using Crane.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Crane.Tests.EditMode
{
    /// <summary>
    /// 座標変換(<see cref="IngameLayout"/>, <see cref="SetupToPhysicsConverter"/>)と
    /// 状態マスク(<see cref="GameStateMaskUtility"/>)のテスト。
    /// </summary>
    public class LayoutAndStateTests
    {
        /// <summary>座標の比較の許容誤差。</summary>
        private const float Tolerance = 0.001f;

        /// <summary>テスト用に作ったカメラ。テストごとに破棄する。</summary>
        private GameObject cameraObject;

        /// <summary>
        /// 破棄し忘れを防ぐため、テストで作ったカメラを破棄する。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (cameraObject != null)
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        /// <summary>
        /// Setup Canvas の左下と右上は、Ingame の Viewport 上で Setup 画像の範囲の角になる。
        /// </summary>
        [Test]
        public void NormalizedCanvasToViewport_MapsCornersToSetupImageRect()
        {
            Rect rect = IngameLayout.SetupImageViewportRect;

            Vector2 bottomLeft = IngameLayout.NormalizedCanvasToViewport(Vector2.zero);
            Vector2 topRight = IngameLayout.NormalizedCanvasToViewport(Vector2.one);

            Assert.AreEqual(rect.xMin, bottomLeft.x, Tolerance);
            Assert.AreEqual(rect.yMin, bottomLeft.y, Tolerance);
            Assert.AreEqual(rect.xMax, topRight.x, Tolerance);
            Assert.AreEqual(rect.yMax, topRight.y, Tolerance);
        }

        /// <summary>
        /// Ingame では Setup 画像が拡大表示される(原作の実測値で約 1.44 倍)。
        /// </summary>
        [Test]
        public void GetScale_IsZoomedIn()
        {
            Vector3 scale = IngameLayout.GetScale();

            Assert.Greater(scale.x, 1f);
            Assert.AreEqual(scale.x, scale.y, 0.01f);
        }

        /// <summary>
        /// 変換した点はワールドの z = 0 平面上にあり、Canvas 上の距離の比が保たれる。
        /// </summary>
        [Test]
        public void CanvasToWorldPoint_PreservesRatiosOnWorldPlane()
        {
            Camera camera = CreateOrthographicCamera();
            Vector2 resolution = new Vector2(1920f, 1080f);

            Vector3 origin = SetupToPhysicsConverter.CanvasToWorldPoint(Vector2.zero, resolution, camera);
            Vector3 right = SetupToPhysicsConverter.CanvasToWorldPoint(new Vector2(100f, 0f), resolution, camera);
            Vector3 farRight = SetupToPhysicsConverter.CanvasToWorldPoint(new Vector2(200f, 0f), resolution, camera);

            Assert.AreEqual(0f, origin.z, Tolerance);
            Assert.AreEqual(Vector3.Distance(origin, right) * 2f, Vector3.Distance(origin, farRight), Tolerance);
        }

        /// <summary>
        /// Canvas 上の 1px の大きさは縦横で等しい(16:9 の画面で歪まない)。
        /// </summary>
        [Test]
        public void CanvasToWorldSize_IsUniformOnSixteenByNineScreen()
        {
            Camera camera = CreateOrthographicCamera();

            Vector2 size = SetupToPhysicsConverter.CanvasToWorldSize(Vector2.one, new Vector2(1920f, 1080f), camera);

            Assert.AreEqual(size.x, size.y, 0.0005f);
        }

        /// <summary>
        /// マスクに含めた状態だけが含まれると判定される。
        /// </summary>
        [Test]
        public void GameStateMask_ContainsOnlyIncludedStates()
        {
            GameStateMask mask = GameStateMask.Playing | GameStateMask.BenefitTime;

            Assert.IsTrue(GameStateMaskUtility.Contains(mask, GameState.Playing));
            Assert.IsTrue(GameStateMaskUtility.Contains(mask, GameState.BenefitTime));
            Assert.IsFalse(GameStateMaskUtility.Contains(mask, GameState.Setup));
            Assert.IsFalse(GameStateMaskUtility.Contains(mask, GameState.Result));
            Assert.IsFalse(GameStateMaskUtility.Contains(mask, GameState.BrokenEnding));
        }

        /// <summary>
        /// ゲーム本編と同じ設定の正投影カメラを作る。
        /// </summary>
        /// <returns>サイズ 5.4、位置 (0, 0, -10)、アスペクト比 16:9 のカメラ。</returns>
        private Camera CreateOrthographicCamera()
        {
            cameraObject = new GameObject("TestCamera");
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.aspect = 16f / 9f;
            return camera;
        }
    }
}
