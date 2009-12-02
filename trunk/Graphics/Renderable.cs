using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Renderer.Graphics
{
    public struct Light
    {
        public Vector3 Direction;
        public Vector4 DiffuseColor;
        public Vector4 AmbientColor;
    }

    class Renderable : DrawableGameComponent
    {
        #region Fields
        public float scale { get; set; }
        public float rotation { get; set; }
        public Vector3 rotAxis { get; set; }
        public Vector3 position { get; set; }
        public Matrix worldMat { get; set; }

        protected BoundingBox boundingBox;

        protected Model model;
        public string modelAssetName { get; set; }

        protected Effect effect;
        public string effectAssetName { get; set; }

        protected bool isLightingEnabled;
        #endregion

		#region Properties
        public Vector3 Center
        {
            get
            {
                Vector3 min = Vector3.Transform(boundingBox.Min, worldMat);
                Vector3 max = Vector3.Transform(boundingBox.Max, worldMat);

                return (min + max) * 0.5f;
            }
        }

        public Model Model
        {
            get
            {
                return model;
            }
        }

        public Effect Effect
        {
            get
            {
                return effect;
            }
        }
		#endregion

        #region Methods
        public Renderable(Game game)
            : base(game)
        {
            position = Vector3.Zero;
            scale = 1.0f;
            rotation = 0.0f;
            rotAxis = Vector3.Up;
            isLightingEnabled = false;
        }

        public override void Initialize()
        {
            worldMat =  Matrix.CreateFromAxisAngle(rotAxis, rotation) *
                        Matrix.CreateScale(scale) *
                        Matrix.CreateTranslation(position);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create and load the effect
            if (effectAssetName != null)
            {
                effect = Game.Content.Load<Effect>(effectAssetName);

                // TODO: setup effect lights here
            }

            if (modelAssetName != null)
            {
                model = Game.Content.Load<Model>(modelAssetName);

                if (model.Tag != null)
                {
                    boundingBox = (BoundingBox)model.Tag;
                }
                else
                {
                    boundingBox = new BoundingBox();
                }
            }

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect != null)
            {
                DrawCustomEffect();
            }
            else
            {
                DrawBasicEffect();
            }

            //base.Draw(gameTime);
        }

        protected virtual void DrawBasicEffect()
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect basicEffect in mesh.Effects)
                {
                    basicEffect.World = boneTransforms[mesh.ParentBone.Index] * worldMat;
                    basicEffect.View = VolumetricRenderer.Game.Camera.viewMat;
                    basicEffect.Projection = VolumetricRenderer.Game.Camera.projectionMat;

                    // TEMP - until we get lights added
                    if (isLightingEnabled)
                    {
                        basicEffect.EnableDefaultLighting();
                        basicEffect.PreferPerPixelLighting = true;
                    }
                    else
                    {
                        basicEffect.LightingEnabled = false;
                    }
                }

                mesh.Draw();
            }
        }

        protected virtual void DrawCustomEffect()
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            effect.Begin(SaveStateMode.None);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix worldRootBoneMat = boneTransforms[mesh.ParentBone.Index] * worldMat;

                Matrix worldRootBoneInverseMat = Matrix.Invert(worldRootBoneMat);
                worldRootBoneInverseMat = Matrix.Transpose(worldRootBoneInverseMat);

                effect.Parameters["World"].SetValue(worldRootBoneMat);
                effect.Parameters["WorldInverseTransform"].SetValue(worldRootBoneInverseMat);
                effect.Parameters["WorldViewProjection"].SetValue(worldRootBoneMat *
                                                VolumetricRenderer.Game.Camera.viewMat *
                                                VolumetricRenderer.Game.Camera.projectionMat);
                effect.Parameters["CameraPosition"].SetValue(VolumetricRenderer.Game.Camera.position);

                VolumetricRenderer.Game.GraphicsDevice.Indices = mesh.IndexBuffer;

                foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
                {
                    effectPass.Begin();
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        VolumetricRenderer.Game.GraphicsDevice.Vertices[0].SetSource(
                            mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);

                        VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = meshPart.VertexDeclaration;

                        VolumetricRenderer.Game.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                            meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                    }
                    effectPass.End();
                }
            }

            effect.End();
        }
        #endregion
    }
}
