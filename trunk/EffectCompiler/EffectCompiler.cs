using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.  
using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.EffectContent;
using TOutput = Microsoft.Xna.Framework.Graphics.CompiledEffect;

namespace WindowsEffectCompiler
{
    /// <summary>  
    /// This class will be instantiated by the XNA Framework Content Pipeline  
    /// to apply custom processing to content data, converting an object of  
    /// type TInput to TOutput. The input and output types may be the same if  
    /// the processor wishes to alter data without changing its type.  
    ///  
    /// This should be part of a Content Pipeline Extension Library project.  
    ///  
    /// TODO: change the ContentProcessor attribute to specify the correct  
    /// display name for this processor.  
    /// </summary>  
    [ContentProcessor(DisplayName = "Custom Effect Compiler")]
    public class EffectCompiler : EffectProcessor
    {
        private static bool initialized = false;
        private static string FXCName;

        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            if (!initialized)
            {
                findFXC(input, context);
                initialized = true;
            }

            if (context.TargetPlatform == TargetPlatform.Windows)
            {
                Process p = new Process();
                p.EnableRaisingEvents = false;
                p.StartInfo.FileName = FXCName;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //p.StartInfo.RedirectStandardOutput = true;
                //p.StartInfo.UseShellExecute = false;

                string x = "Shaders/" + Path.GetFileNameWithoutExtension(input.Identity.SourceFilename) + "_fxc";
#if DEBUG
                string opt = "/Zi /Od";
#else  
                string opt = "/Zi /O3";
#endif
                p.StartInfo.Arguments = String.Format("/T fx_2_0 {3} /Fo \"{0}\" /Fe \"{1}\" \"{2}\"", x + ".fxo", x + ".err", input.Identity.SourceFilename, opt);
                p.Start();
                if (!p.WaitForExit(200000))
                {
                    throw new TimeoutException("FXC took more than 20 seconds to compile!");
                }

                // Display returned console information if any
                //StreamReader srOut = p.StandardOutput;
                //string sOutput = srOut.ReadToEnd();
                //srOut.Close();

                //FileStream file = new FileStream(@"C:\Documents and Settings\KMan\My Documents\Visual Studio 2008\Projects\VolumeRayCasting\out.txt", FileMode.Create);
                //StreamWriter writer = new StreamWriter(file);
                //writer.WriteLine(sOutput);
                //writer.Close();
                //file.Close();

                if (p.ExitCode != 0)
                {
                    Trace.WriteLine("Exit code: " + p.ExitCode.ToString());
                    context.Logger.LogWarning("", input.Identity, "Exit code: {0}", p.ExitCode);
                }

                byte[] bytes = File.ReadAllBytes(x + ".fxo");
                string text = File.ReadAllText(x + ".err");
                if (p.ExitCode != 0 || text.Contains("error"))
                {
                    throw new InvalidContentException(text, input.Identity);
                }
                else
                {
                    //print warnings
                    context.Logger.LogWarning("", input.Identity, text);
                }

                return new CompiledEffect(bytes, text);
            }
            else
            {
                return base.Process(input, context);
            }
        }

        private static void findFXC(TInput input, ContentProcessorContext context)
        {
            string[] tests = new string[] { "Utilities\\bin\\x86\\fxc.exe", "Utilities\\bin\\x64\\fxc.exe" };
            foreach (string path in new string[] { "C:\\Program Files", "C:\\Program Files (x86)" })
            {
                string[] dirs = Directory.GetDirectories(path, "Microsoft DirectX*");
                foreach (string dir in dirs)
                {
                    foreach (string test in tests)
                    {
                        string str = Path.Combine(dir, test);
                        if (File.Exists(str))
                        {
                            FXCName = str;
                            Trace.WriteLine("Found FXC.EXE: " + FXCName);
                            context.Logger.LogMessage("FOUND FXC: " + str);
                            return;
                        }
                    }
                }
            }
            FXCName = "FXC.EXE";

            context.Logger.LogWarning("", input.Identity, "FXC NOT FOUND!!!!!!!!!!!!");
        }
    }
}
