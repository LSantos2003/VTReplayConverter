using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTReplayConverter
{
    public class VTRC
    {
        public static async Task OpenFileFromPath(string folderPath, string fileName, bool openInTacview, bool convert)
        {
            if (Program.ConvertingFile)
                return;

            Program.ConvertingFile = true;

            ConvertMapFromPath(folderPath, fileName);

            string tacviewSavePath = Path.Combine(Program.AcmiSavePath, $"{fileName}.acmi");

            if (!File.Exists(tacviewSavePath) || convert)
            {
                await ConvertTrackFromPath(folderPath, fileName);
            }

            Program.ConvertingFile = false;

            if (openInTacview)
                System.Diagnostics.Process.Start(tacviewSavePath);
        }

        public static async void OpenFileFromPath(string folderPath, string fileName, bool openInTacview, bool convert, Button replayButton, bool changeButtonColor)
        {
            if (Program.ConvertingFile)
                return;

            if (changeButtonColor)
                replayButton.BackColor = VTRConverterForm.ReplayNotConvertedColor;

            await OpenFileFromPath(folderPath, fileName, openInTacview, convert);
            replayButton.BackColor = VTRConverterForm.ReplayConvertedColor;
        }

        public static void ConvertMapFromPath(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Path does not exist");
                Console.Write(folderPath);
                return;
            }

            Console.WriteLine("-----------------------");
            string heightMapPath = Path.Combine(folderPath, $"heightmap.pngb");
            string configPath = Path.Combine(folderPath, $"info.cfg");
            if (!File.Exists(heightMapPath))
            {
                Console.WriteLine($"File does not exist at {heightMapPath}");
                return;
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"File does not exist at {configPath}");
                return;
            }

            Console.WriteLine("Converting Map File");
            HeightMapGeneration.ConvertHeightMap(heightMapPath, configPath);
            Console.WriteLine("Map File Converted!");

        }

        public static async Task ConvertTrackFromPath(string folderPath, string fileName)
        {
            Console.WriteLine("-----------------------");
            string readPath = Path.Combine(folderPath, $"replay.vtr");
            bool isVFM = fileName.Contains(".vrb");
            //VFM Detection
            if (isVFM)
            {
                readPath = Path.Combine(folderPath);
            }
            string savePath = Path.Combine(Program.AcmiSavePath, $"{fileName}.acmi");
            if (!File.Exists(readPath))
            {
                Console.WriteLine($"File does not exist at {readPath}");
                return;
            }

            Console.WriteLine("Converting VTR File");
            await Task.Run(() => VTACMI.ConvertToACMI(readPath, savePath, isVFM));
            Console.WriteLine("File converted!");
        }

        public static async void ConvertAll(Dictionary<string, Button> replayButtonDict, bool reConvert)
        {
            if (Program.ConvertingFile)
                return;
            Program.ConvertingFile = true;

            if (!Directory.Exists(Program.VTReplaysPath))
            {
                Console.WriteLine("Cannot find VT Replays path. Do you have any replays?");
                Console.WriteLine(Program.VTReplaysPath);
                return;
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<string> replayPaths = Directory.GetFiles(Program.VTReplaysPath, "*.*vtr", SearchOption.AllDirectories).ToList();
            replayPaths.AddRange(Directory.GetFiles(Program.VFMReplaysPath, "*.vrb", SearchOption.AllDirectories));

            // Console.WriteLine("-----------------------");
            // Console.WriteLine("Converting all VTR files\n");

            int staggerMilliseconds = 500;
            var tasks = new List<Task>();

            for (int i = 0; i < replayPaths.Count; i++)
            {
                string replayPath = replayPaths[i];
                string folderPath = Path.GetDirectoryName(replayPath);

                //VFM Check
                bool isVFM = replayPath.Contains(".vrb");
                string pathToUse = isVFM ? replayPath : folderPath;

                if (!reConvert && ACMIUtils.IsReplayConverted(folderPath))
                    continue;



                int delay = i * staggerMilliseconds;

                tasks.Add(Task.Run(async () =>
                {
                    // Console.WriteLine("-----------------------");
                    replayButtonDict[pathToUse].BackColor = VTRConverterForm.ReplayNotConvertedColor;
                    string folderName = Path.GetFileName(folderPath);
                    string savePath = Path.Combine(Program.AcmiSavePath, $"{folderName}.acmi");
                    await VTACMI.ConvertToACMIAsync(replayPath, savePath, isVFM);
                    replayButtonDict[pathToUse].BackColor = VTRConverterForm.ReplayConvertedColor;
                    replayButtonDict[pathToUse].Enabled = true;
                }));
            }

            if (tasks.Count > 0)
                await Task.WhenAll(tasks);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Console.WriteLine($"{vtrPaths.Length} Files converted!");
            // Console.WriteLine($"Total conversion time: {elapsedMs / 1000f} seconds\n");
            ACMILoadingBar.ResetBar();
            Program.ConvertingFile = false;
        }

    }
}
