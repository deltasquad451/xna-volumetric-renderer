using System;

namespace Graphics
{
    static class MainEntry
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (VolumetricRenderer volRenderer = new VolumetricRenderer())
            {
                volRenderer.Run();
            }
        }
    }
}

