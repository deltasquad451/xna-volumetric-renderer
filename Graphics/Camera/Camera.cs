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
        private const float FAR_PLANE_DIST = 100.0f;

        public Vector3 position { get; set; }
        public Vector3 target {get; set; }
        public Matrix viewMat { get; private set; }
        public Matrix projectionMat { get; private set; }
        public float fov { get; private set; }
        public float aspectRatio { get; private set; }
        public int keyflag = 0;
        public float camx = 2.5f;
        public float camy = 2.5f;
        public float camz = -7.0f;

        private KeyboardState currentKeyboardState;

        #endregion

        #region Initialization
        public Camera(Viewport viewport)
        {

            aspectRatio = (float)viewport.Width / (float)viewport.Height;
            fov = MathHelper.ToRadians(60.0f);
            projectionMat = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio,
                                                    NEAR_PLANE_DIST, FAR_PLANE_DIST);
            position = new Vector3(2.5f, 2.5f, -7.0f);
            target = new Vector3(2.5f, 2.5f, 2.5f);
        }
        #endregion

        #region Methods
        public void Update()
        {
            currentKeyboardState = Keyboard.GetState();

            if (keyflag == 0)
            {
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left)){
                camx -= 0.5f;
                camz += 0.5f;
                //if (camx > -3.0f) { camx = -3.0f; }
                if (camx < -30.0f) { camx = -30.0f; }
                if (camz > -3.0f) { camz = -3.0f; }
                if (camz < -30.0f) { camz = -30.0f; }
                position = new Vector3(camx, camy, camz);
                //target = new Vector3(camx, 2.5f, 2.5f);
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
                keyflag = 1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right)){
                camx += 0.5f;
                camz -= 0.5f;
                //if (camx > -3.0f) { camx = -3.0f; }
                if (camx < -30.0f) { camx = -30.0f; }
                if (camz > -3.0f) { camz = -3.0f; }
                if (camz < -30.0f) { camz = -30.0f; }
                position = new Vector3(camx, camy, camz);
                //target = new Vector3(camx, 2.5f, 2.5f);
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
                keyflag = 1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                camz += 0.5f;
                if (camz > -3.0f) { camz = -3.0f; }
                if (camz < -30.0f){camz = -30.0f;}
                position = new Vector3(camx, camy, camz); 
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
                keyflag = 1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                camz -= 0.5f;
                if (camz > -3.0f){camz = -3.0f;}
                if (camz < -30.0f){camz = -30.0f;}
                position = new Vector3(camx, camy, camz); 
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
                keyflag = 1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                camx = 2.5f; camy = 2.5f; camz = -7.0f;
                position = new Vector3(camx, camy, camz);
                target = new Vector3(camx, camy, 2.5f); 
                viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
                keyflag = 1;
            }
        }
        #endregion
    }
}