using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTReplayConverter.ConsoleApp;

namespace VTReplayConverter
{
    internal class Program
    {
        public static string VTReplaysPath;
        public static string AcmiSavePath;

        public static bool ProgramRunning = true;
        static void Main(string[] args)
        {

            Console.WriteLine("Initializing Replay Recorder");
            ReplayRecorder recorder = new ReplayRecorder();
            recorder.Awake();

            SetUpFilePaths();

            CommandHandler.SetupCommands();
            Task.Run(() => CommandHandler.ReadCommands());


            while (ProgramRunning) { }

        }

        private static void SetUpFilePaths()
        {
            VTReplaysPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            VTReplaysPath = Path.Combine(VTReplaysPath, "Boundless Dynamics, LLC\\VTOLVR\\SaveData\\Replays");

            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            AcmiSavePath = Path.Combine(strWorkPath, "TACVIEW");

            if (!Directory.Exists(AcmiSavePath))
            {
                Console.WriteLine($"Creating TACVIEW Folder at: {AcmiSavePath}");
                Directory.CreateDirectory(AcmiSavePath);
            }

        }
        public static void EndProgram()
        {
            ProgramRunning = false;
        }
    }
}
