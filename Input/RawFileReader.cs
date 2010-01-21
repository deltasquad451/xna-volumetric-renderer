using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Diagnostics;

/*
 * Example of how to use this:
 * Texture3D texture3D = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, 256, 256, 178, 0,
 *                                     TextureUsage.Linear, SurfaceFormat.Single);
 * Engine.Input.RawFileReader tempFileReader = new Engine.Input.RawFileReader();
 * tempFileReader.Open("BostonTeapot.raw");
 * tempFileReader.GetRawData(texture3D);
 * tempFileReader.Close();
 */

namespace Renderer.Input
{
    public class RawFileReader
    {
        #region Fields
        private FileStream fileStream;
		private int width;
		private int height;
		private int depth;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the open filestream
        /// </summary>
        public FileStream FileStream
        {
            get
            {
                if (fileStream.CanRead)
                    return fileStream;
                else
                    return null;
            }
        }
        #endregion

        #region Methods
		public void Open(String filename, int width, int height, int depth)
        {
            Debug.Assert(File.Exists(filename), String.Format("Unabled to locate file {0}\\{1}", System.Environment.CurrentDirectory, filename));
            fileStream = new FileStream(filename, FileMode.Open);
			this.width = width;
			this.height = height;
			this.depth = depth;
        }

        public void Close()
        {
            fileStream.Close();
        }

        public void GetRawData(out float[] scaledValues)
        {
            Debug.Assert(fileStream.CanRead, "Cannot read from file stream\n");

            BinaryReader binaryReader = new BinaryReader(fileStream);
            //float[] scaledValues;

            if (fileStream.Length > width * height * depth)
            {
                // Assume we have a 16-bit RAW file
                ushort[] dataBuffer = new ushort[width * height * depth];

                for (int i = 0; i < dataBuffer.Length; ++i)
                {
                    dataBuffer[i] = binaryReader.ReadUInt16();
                }

                // Scale the values to a [0,1] range
                scaledValues = new float[dataBuffer.Length];
                for (int i = 0; i < scaledValues.Length; ++i)
                {
					scaledValues[i] = (float)dataBuffer[i] / ushort.MaxValue;
                }
            }
            else
            {
                // We have an 8-bit RAW file
                byte[] dataBuffer = new byte[width * height * depth];
                binaryReader.Read(dataBuffer, 0, sizeof(byte) * dataBuffer.Length);

                // Scale the values to a [0,1] range
                scaledValues = new float[dataBuffer.Length];
                for (int i = 0; i < scaledValues.Length; ++i)
                {
                    scaledValues[i] = (float)dataBuffer[i] / byte.MaxValue;
                }
            }
            binaryReader.Close();

            //texture3D.SetData(scaledValues);
        }
        #endregion
    }
}
