using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTReplayConverter
{
    public class CommandHandler
    {
        static Dictionary<string, ConsoleCommand> commandDict = new Dictionary<string, ConsoleCommand>();

        static List<CommandAttribute> commandAttributeList = new List<CommandAttribute>();

        static bool pauseInput = false;


        public class ConsoleCommand
        {
            public ConsoleCommand(CommandAttribute attribute, Action<string> action)
            {
                this.CommandAttribute = attribute;
                this.action = action;
            }

            public CommandAttribute CommandAttribute;
            public Action<string> action;

        }

        public static void ReadCommands()
        {
            while (Program.ProgramRunning)
            {

                //Console.Write(">>>");
                if (!pauseInput)
                {
                    Console.Write(">>>");
                    string command = Console.ReadLine();
                    OnCommandEntered(command);
                }
                else if (pauseInput && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    pauseInput = false;
                    Console.WriteLine("Resuming input");
                }


            }
        }


        public static void OnCommandEntered(string command)
        {
            string prefix;
            string args;
            try
            {
                prefix = command.Substring(0, command.IndexOf(' '));
                args = command.Substring(command.IndexOf(' ') + 1);

            }
            catch (ArgumentOutOfRangeException ex)
            {
                prefix = command;
                args = " ";
            }

            ConsoleCommand consoleCommand;
            if (commandDict.TryGetValue(prefix.ToLower(), out consoleCommand))
            {
                consoleCommand.action(args.Trim());
                if (consoleCommand.CommandAttribute.LogAfterCall)
                {
                    PauseInput();
                }

                return;
            }

            Console.WriteLine($"\'{command}\' is not a valid command");
            Console.WriteLine($">Prefix: \'{prefix}\'");
            Console.WriteLine($">Args: \'{args}\'");


        }

        public static void SetupCommands()
        {
            MethodInfo[] commands = typeof(CommandHandler).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            foreach (MethodInfo methodInfo in commands)
            {
                CommandAttribute attribute = (CommandAttribute)methodInfo.GetCustomAttribute(typeof(CommandAttribute), true);
                if (attribute != null)
                {
                    var action = (Action<string>)methodInfo.CreateDelegate(typeof(Action<string>));
                    ConsoleCommand consoleCommand = new ConsoleCommand(attribute, action);
                    commandDict.Add(attribute.Prefix.ToLower(), consoleCommand);

                    commandAttributeList.Add(attribute);
                }
            }

            commandAttributeList.Sort((x, y) => string.Compare(x.Prefix, y.Prefix));


        }

        [Command("Help", "I hope you know what this does by now")]
        static void Help(string args)
        {

            string format = "{0, -15} {1, -10}";

            Console.WriteLine();
            foreach (CommandAttribute attribute in commandAttributeList)
            {
                Console.WriteLine(string.Format(format, attribute.Prefix.ToUpper(), attribute.Description));
            }
            Console.WriteLine();

        }

        [Command("Clear", "Clear commands prompt")]
        static void Clear(string args)
        {
            Console.Clear();
            Console.WriteLine("VTOL VR Tactical Replay to TACVIEW Converter");
            Console.WriteLine("Type in \"Help\" for a list of commands!");
        }

        [Command("End", "Ends the program")]
        static void EndProgram(string args)
        {
            Console.WriteLine("Ending program");
            Program.EndProgram();
        }

        [Command("Display", "Displays available VT replays to convert")]
        static void DisplayVT(string args)
        {
            if (!Directory.Exists(Program.VTReplaysPath))
            {
                Console.WriteLine("Cannot find VT Replays path. Do you have any replays?");
                Console.WriteLine(Program.VTReplaysPath);
                return;
            }

            string[] vtrPaths = Directory.GetDirectories(Program.VTReplaysPath);
            foreach (string vtrPath in vtrPaths)
            {
                Console.WriteLine(Path.GetFileName(vtrPath));
            }

        }

        [Command("Open", "Opens existing TACVIEW file with default process")]
        static void OpenFile(string args)
        {
            ConvertMap(args);

            string openPath = Path.Combine(Program.AcmiSavePath, $"{args}.acmi");
            if (!File.Exists(openPath))
            {
                Console.WriteLine("Path does not exist");
                Console.Write(openPath);
                return;
            }

            System.Diagnostics.Process.Start(openPath);
        }

        [Command("Convert", "Converts both Track and Map File")]
        static void ConvertMapAndFile(string args)
        {
            if (Program.ConvertingFile)
                return;
            Program.ConvertingFile = true;
            ConvertMap(args);
            ConvertTrackFile(args);
            Program.ConvertingFile = false;
        }

        [Command("ConvertTrack", "Converts specific VTR File")]
        static void ConvertTrackFile(string args)
        {
            Console.WriteLine("-----------------------");
            string readPath = Path.Combine(Program.VTReplaysPath, $"{args}\\replay.vtr");
            string savePath = Path.Combine(Program.AcmiSavePath, $"{args}.acmi");
            if (!File.Exists(readPath))
            {
                Console.WriteLine($"File does not exist at {readPath}");
                return;
            }

            Console.WriteLine("Converting VTR File");
            VTACMI.ConvertToACMI(readPath, savePath);
            Console.WriteLine("File converted!");

            System.Diagnostics.Process.Start(savePath);
        }

        [Command("ConvertMap", "Converts Map File")]
        static void ConvertMap(string args)
        {
            Console.WriteLine("-----------------------");
            string heightMapPath = Path.Combine(Program.VTReplaysPath, $"{args}\\heightmap.pngb");
            string configPath = Path.Combine(Program.VTReplaysPath, $"{args}\\info.cfg");
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

        [Command("Debug", "Debugs specific VTR File")]
        static void DebugVTR(string args)
        {
            string readPath = Path.Combine(Program.VTReplaysPath, $"{args}\\replay.vtr");
            if (!File.Exists(readPath))
            {
                Console.WriteLine($"File does not exist at {readPath}");
                return;
            }

            VTACMI.DebugVTR(readPath);
        }

        public static void PauseInput()
        {
            Console.WriteLine("Pausing input to display logs from lobby!\n(Press Escape to exit)");
            pauseInput = true;
        }

        public static void WritePausedLine(string line)
        {
            if (pauseInput) Console.WriteLine(line);
        }


    }
}
