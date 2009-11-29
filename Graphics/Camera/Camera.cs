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
        private const float SPEED = 4.0f;
        private const float ANGULAR_SPEED = (float)Math.PI/10.0f;
        private const float MIN_DIST = 5.0f;
        private const float MAX_DIST = 30.0f;

        public Vector3 position { get; set; }
        public Vector3 target {get; set; }
        public Matrix viewMat { get; private set; }
        public Matrix projectionMat { get; private set; }
        public float fov { get; private set; }
        public float aspectRatio { get; private set; }
        public Quaternion rotQuat { get; private set; }

        private KeyboardState currentKeyboardState;

        #endregion

        #region Initialization
        public Camera(Viewport viewport)
        {

            aspectRatio = (float)viewport.Width / (float)viewport.Height;
            fov = MathHelper.ToRadians(60.0f);

            //projectionMat = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio,
            //                                        NEAR_PLANE_DIST, FAR_PLANE_DIST);

            Matrix proj;
            proj.M11 = 1 / ((float)Math.Tan(fov / 2.0f) * aspectRatio);
            proj.M12 = proj.M13 = proj.M14 = 0.0f;
            proj.M21 = proj.M23 = proj.M24 = 0.0f;
            proj.M22 = 1 / ((float)Math.Tan(fov / 2.0f));
            proj.M31 = proj.M32 = 0.0f;
            proj.M33 = FAR_PLANE_DIST / (FAR_PLANE_DIST - NEAR_PLANE_DIST);
            proj.M34 = 1.0f;
            proj.M41 = proj.M42 = proj.M44 = 0.0f;
            proj.M43 = -NEAR_PLANE_DIST * FAR_PLANE_DIST / (FAR_PLANE_DIST - NEAR_PLANE_DIST);

            projectionMat = proj;
            position = new Vector3(2.5f, 2.5f, -7.0f);
            target = new Vector3(2.5f, 2.5f, 2.5f);
            rotQuat = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Left)){
                Matrix rotationY;
                Matrix.CreateRotationY((float)gameTime.ElapsedGameTime.TotalSeconds * ANGULAR_SPEED, out rotationY);
                Vector4 targetToPos = new Vector4(position - target, 0);
                targetToPos = Vector4.Transform(targetToPos, rotationY);
                position = new Vector3(target.X + targetToPos.X, target.Y + targetToPos.Y, target.Z + targetToPos.Z);
                rotQuat = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(rotationY) * rotQuat);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right)){
                Matrix rotationY;
                Matrix.CreateRotationY((float)gameTime.ElapsedGameTime.TotalSeconds * -ANGULAR_SPEED, out rotationY);
                Vector4 targetToPos = new Vector4(position - target, 0);
                targetToPos = Vector4.Transform(targetToPos, rotationY);
                position = new Vector3(target.X + targetToPos.X, target.Y + targetToPos.Y, target.Z + targetToPos.Z);
                rotQuat = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(rotationY) * rotQuat);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                Vector3 targetToPos = position - target;
                float newLength = targetToPos.Length() - ((float)gameTime.ElapsedGameTime.TotalSeconds * SPEED);
                if (newLength < MIN_DIST)
                {
                    newLength = MIN_DIST;
                }
                targetToPos.Normalize();
                position = target + Vector3.Multiply(targetToPos, newLength);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                Vector3 targetToPos = position - target;
                float newLength = targetToPos.Length() + ((float)gameTime.ElapsedGameTime.TotalSeconds * SPEED);
                if (newLength > MAX_DIST)
                {
                    newLength = MAX_DIST;
                }
                targetToPos.Normalize();
                position = target + Vector3.Multiply(targetToPos, newLength);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                position = new Vector3(2.5f, 2.5f, -7.0f);
                target = new Vector3(2.5f, 2.5f, 2.5f);
                rotQuat = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
            }

            //viewMat = Matrix.CreateLookAt(position, target, Vector3.Up);
            viewMat = Matrix.Invert(
                        Matrix.CreateFromQuaternion(rotQuat) *
                        Matrix.CreateTranslation(position)
                      );
        }
        #endregion
    }
}