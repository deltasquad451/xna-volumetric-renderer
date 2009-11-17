using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Renderer.Graphics.Camera
{
    public class Camera
    {
        #region Fields
        private const float NEAR_PLANE_DIST = 0.1f;
        private const float FAR_PLANE_DIST = 1000.0f;

        public Vector3 position { get; set; }
        public Vector3 target {get; set; }
        public Matrix viewMat { get; private set; }
        public Matrix projectionMat { get; private set; }
        public float fov { get; private set; }
        public float aspectRatio { get; private set; }
        #endregion

        #region Initialization
        public Camera(Viewport viewport)
        {
            aspectRatio = (float)viewport.Width / (float)viewport.Height;
            fov = MathHelper.ToRadians(90.0f);
            projectionMat = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio,
                                                    NEAR_PLANE_DIST, FAR_PLANE_DIST);
            position = new Vector3(2.5f, 2.5f, -3.0f);
            target = new Vector3(2.5f, 2.5f, 2.5f);
        }
        #endregion

        #region Methods
        public void Update()
        {
            viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
        }
        #endregion
    }
}