using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTReplayConverter
{
    public partial class VTRConverterForm : Form
    {
        string progressTextPrefix = "Progress";

        Dictionary<string, Button> replayButtonDict = new Dictionary<string, Button>();

        public static Color ReplayNotConvertedColor = Color.Crimson;
        public static Color ReplayConvertedColor = Color.DarkOliveGreen;
        public VTRConverterForm()
        {
            InitializeComponent();
        }

        private void VTRConverterForm_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;

            progressBar1.Visible = false;
            ProgressText.Visible = false;
            WarningLabel.Visible = false;
            includeEW.Checked = !VTACMI.IncludeEW;

            CreateReplayList();
            this.TemplateButton.Visible = false;

        }

        private void ReConvertAll_Click(object sender, EventArgs e)
        {
            if (Program.ConvertingFile)
                return;

            this.progressTextPrefix = "Re-Converting All Replays";
            CommandHandler.ConvertAll(this.replayButtonDict, true);
        }

        private void ConvertRemaining_Click(object sender, EventArgs e)
        {
            if (Program.ConvertingFile)
                return;

            this.progressTextPrefix = "Converting Unconverted Replays";
            CommandHandler.ConvertAll(this.replayButtonDict, false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Visible = Program.ConvertingFile;
            ProgressText.Visible = Program.ConvertingFile;
            WarningLabel.Visible = Program.ConvertingFile;
            int progress = ACMILoadingBar.GetKeyFrameProgress();
            this.progressBar1.Value = progress;
            this.ProgressText.Text = $"{this.progressTextPrefix}\nProgress: {progress}%\nKeyframes: {ACMILoadingBar.currentKeyFrameProgress}/{ACMILoadingBar.maxKeyFrameCount}";
                

        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            if (Program.ConvertingFile)
                return;

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "vtr files (*.vtr)|*.vtr";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(fileDialog.FileName);
                string folderName = Path.GetFileName(folderPath);

                this.progressTextPrefix = $"Converting {folderName}";
                CommandHandler.OpenFileFromPath(folderPath, folderName, true, false);
            }
        }

        private void includeEW_CheckedChanged(object sender, EventArgs e)
        {
            VTACMI.IncludeEW = !includeEW.Checked;
        }

        private void CreateReplayList()
        {
            string[] replayPaths = Directory.GetDirectories(Program.VTReplaysPath);

            int replayButtonCount = 0;
            foreach(string replayPath in replayPaths)
            {

                CreateReplayButton(this.TemplateButton, replayPath, replayButtonCount);
                replayButtonCount++;
            }


        }

        private void CreateReplayButton(Button templateButton, string replayPath, int buttonCount)
        {
            Button button = new Button();
            this.Controls.Add(button);
            string folderName = Path.GetFileName(replayPath);
            button.Text = folderName;
            button.Parent = this.ReplayButtonPanel;
            button.Size = templateButton.Size;
            button.Location = new Point(templateButton.Location.X, templateButton.Location.Y + (templateButton.Size.Height * buttonCount));
            button.ForeColor = templateButton.ForeColor;
            button.BackColor = ACMIUtils.IsReplayConverted(replayPath) ? ReplayConvertedColor : ReplayNotConvertedColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.MouseOverBackColor = templateButton.FlatAppearance.MouseOverBackColor;
            button.FlatAppearance.MouseDownBackColor = templateButton.FlatAppearance.MouseDownBackColor;
            button.Font = templateButton.Font;
            button.BringToFront();

            button.MouseDown += (sender, EventArgs) => { OpenReplay(sender, EventArgs, button, replayPath); };

            this.replayButtonDict[replayPath] = button;
        }

        private void OpenReplay(object sender, MouseEventArgs args, Button replayButton, string replayPath)
        {
            
            string folderPath = replayPath;
            string folderName = Path.GetFileName(folderPath);

            this.progressTextPrefix = $"Converting {folderName}";

            bool leftClick = args.Button == MouseButtons.Left;
            bool rightClick = args.Button == MouseButtons.Right;

            if (leftClick)
            {
                this.progressTextPrefix = $"Opening {folderName}";
                CommandHandler.OpenFileFromPath(folderPath, folderName, true, folderName.Contains("Autosave"), replayButton, false);
            }else if (rightClick)
            {
                this.progressTextPrefix = $"Re-Converting {folderName}";
                CommandHandler.OpenFileFromPath(folderPath, folderName, false, true, replayButton, true);
            }


        }

        private void OpenTacviewFolder_Click(object sender, EventArgs e)
        {
            Process.Start(Program.AcmiSavePath);
        }

        private void OpenReplayFolder_Click(object sender, EventArgs e)
        {
            Process.Start(Program.VTReplaysPath);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void ProgressText_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
