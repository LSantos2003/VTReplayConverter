using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Windows.Forms;
using static System.Environment;

namespace VTReplayConverter
{
    internal class Program
    {
        public const bool ConsoleMode = false;
        public const float BulletPollRate = 0.35f;
        public const bool ConvertBullets = false;
        public static string VTReplaysPath;
        public static string AcmiSavePath;
        public static string TacviewTerrainPath;
        public static string TacviewMeshPath;
        public static string LocalMeshPath;

        public static string ObjectConverterPath;

        public static bool ProgramRunning = true;

        public static bool ConvertingFile = false;

        [STAThreadAttribute]
        static void Main(string[] args)
        {

            SetUpFilePaths();
            SetUpMeshes();

            ACMIObjects.InitilizeUnitDict();

            if (ConsoleMode)
            {
                CommandHandler.SetupCommands();
                Task.Run(() => CommandHandler.ReadCommands());

                Console.WriteLine("VTOL VR Tactical Replay to TACVIEW Converter");
                Console.WriteLine("Type in \"Help\" for a list of commands!");
            }
            else
            {
                //System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new VTRConverterForm());
            }
           
            while (ProgramRunning && ConsoleMode) { }

        }

        private static void SetUpFilePaths()
        {
            VTReplaysPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            VTReplaysPath = Path.Combine(VTReplaysPath, "Boundless Dynamics, LLC\\VTOLVR\\SaveData\\Replays");

            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            AcmiSavePath = Path.Combine(strWorkPath, "TACVIEW");

            ObjectConverterPath = Path.Combine(strWorkPath, "units.txt");
            if (!Directory.Exists(AcmiSavePath))
            {
                Console.WriteLine($"Creating TACVIEW Folder at: {AcmiSavePath}");
                Directory.CreateDirectory(AcmiSavePath);
            }

            string porgramDataFolderPath = Environment.GetFolderPath(SpecialFolder.CommonApplicationData);
            TacviewTerrainPath = Path.Combine(porgramDataFolderPath, "Tacview\\Data\\Terrain\\Custom");
            TacviewMeshPath = Path.Combine(porgramDataFolderPath, "Tacview\\Data\\Meshes");

            LocalMeshPath = Path.Combine(strWorkPath, "Meshes");
        }

        private static void SetUpMeshes()
        {
            if (!Directory.Exists(TacviewMeshPath))
            {
                return;
            }

            string[] meshPaths = Directory.GetFiles(LocalMeshPath, "*.*obj", SearchOption.AllDirectories);

            foreach(string meshPath in meshPaths)
            {
                string meshName = Path.GetFileName(meshPath);
                string tacviewMeshPath = Path.Combine(TacviewMeshPath, meshName);
                if (File.Exists(tacviewMeshPath))
                    continue;
                
                File.Copy(meshPath, tacviewMeshPath, true);

            }

        }

        public static void EndProgram()
        {
            ProgramRunning = false;
        }
    }
}
