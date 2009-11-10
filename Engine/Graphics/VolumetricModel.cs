using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Graphics
{
    class VolumetricModel
    {
        #region Fields
        private Texture3D volumeTexture;
        private VertexDeclaration vertexDecl; // TEMP
        private Effect effect; // TEMP
        #endregion

        #region Properties
        public Texture3D VolumeTexture
        {
            get { return volumeTexture; }
        }
        #endregion

        #region Methods
        public VolumetricModel()
        {
            // TEMP
            vertexDecl = new VertexDeclaration(VolumetricRenderer.Game.GraphicsDevice,
                                    VertexPositionColor.VertexElements);

            VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
            effect = VolumetricRenderer.Game.Content.Load<Effect>("Shaders/effects");
            // TEMP - end

            // For now, just initialize this to teapot volume values
            volumeTexture = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, 256, 256, 178, 0,
                                    TextureUsage.Linear, SurfaceFormat.Single);
            Engine.Input.RawFileReader tempFileReader = new Engine.Input.RawFileReader();
            tempFileReader.Open("..\\..\\..\\BostonTeapot.raw");
            tempFileReader.GetRawData(volumeTexture);
            tempFileReader.Close();
        }

        public void Update(GameTime gameTime)
        {
            // TODO
        }

        public void Draw(GameTime gameTime)
        {
            // TEMP - draw a triangle to test the camera
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xView"].SetValue(VolumetricRenderer.Game.Camera.viewMat);
            effect.Parameters["xProjection"].SetValue(VolumetricRenderer.Game.Camera.projectionMat);
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            int points = 8;
            VertexPositionColor[] pointList = new VertexPositionColor[points];

            for (int x = 0; x < points / 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    pointList[(x * 2) + y] = new VertexPositionColor(
                        new Vector3(x * 5, y * 5, 2), Color.DeepPink);
                }
            }

            short[] triangleStripIndices = new short[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

            VolumetricRenderer.Game.GraphicsDevice.Clear(Color.Black);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
                VolumetricRenderer.Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    pointList,
                    0,  // vertex buffer offset to add to each element of the index buffer
                    8,  // number of vertices to draw
                    triangleStripIndices,
                    0,  // first index element to read
                    6   // number of primitives to draw
                );
                pass.End();
            }
            effect.End();
            // TEMP - end
        }

        #endregion
    }
}
