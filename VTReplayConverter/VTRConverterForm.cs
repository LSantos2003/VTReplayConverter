using Squirrel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTReplayConverter
{
    public partial class VTRConverterForm : Form
    {
        UpdateManager uManager;

        string progressTextPrefix = "Progress";

        Dictionary<string, Button> replayButtonDict = new Dictionary<string, Button>();

        public static Color ReplayNotConvertedColor = Color.Crimson;
        public static Color ReplayConvertedColor = Color.DarkOliveGreen;

       
        public VTRConverterForm()
        {
            InitializeComponent();
        }


        private async void VTRConverterForm_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;

            progressBar1.Visible = false;
            ProgressText.Visible = false;
            WarningLabel.Visible = false;
            excludeEW.Checked = !VTACMI.IncludeEW;
            excludeBullets.Checked = !VTACMI.IncludeBullets;

            CreateReplayList();
            this.TemplateButton.Visible = false;

            string assemblyVersion = Program.AssemblyVersion;
            this.versionLabel.Text = "Version:" + assemblyVersion;
            try
            {
                this.uManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/LSantos2003/VTReplayConverter");
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (this.uManager != null && this.uManager.CurrentlyInstalledVersion() != null)
            {
                this.CheckForUpdate();
            }

            this.refreshReplaysButton.Visible = Program.IsDebugMode;
        }

        private void ReConvertAll_Click(object sender, EventArgs e)
        {
            if (Program.ConvertingFile)
                return;

            this.progressTextPrefix = "Re -Converting All Replays";
            VTRC.ConvertAll(this.replayButtonDict, true);
        }

        private void ConvertRemaining_Click(object sender, EventArgs e)
        {
            if (Program.ConvertingFile)
                return;

            this.progressTextPrefix = "Converting Unconverted Replays";
            VTRC.ConvertAll(this.replayButtonDict, false);
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
                VTRC.OpenFileFromPath(folderPath, folderName, true, false);
            }
        }

        private void includeEW_CheckedChanged(object sender, EventArgs e)
        {
            VTACMI.IncludeEW = !excludeEW.Checked;
        }

        private void excludeBullets_CheckedChanged(object sender, EventArgs e)
        {
            VTACMI.IncludeBullets = !excludeBullets.Checked;
        }

        private void CreateReplayList()
        {
            if (!Directory.Exists(Program.VTReplaysPath))
            {
                MessageBox.Show("WARNING: No VTOL VR Tactical Replay Files detected. Go play some VTOL VR!");
            }

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

        private void refreshReplaysButton_Click(object sender, EventArgs e)
        {
            foreach(var button in this.replayButtonDict)
            {
                button.Value.BackColor = ACMIUtils.IsReplayConverted(button.Key) ? ReplayConvertedColor : ReplayNotConvertedColor;
            }
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
                VTRC.OpenFileFromPath(folderPath, folderName, true, folderName.Contains("Autosave"), replayButton, false);
            }else if (rightClick)
            {
                this.progressTextPrefix = $"Re-Converting {folderName}";
                VTRC.OpenFileFromPath(folderPath, folderName, false, true, replayButton, true);
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

        private async void CheckForUpdate()
        {
            var updateInfo = await this.uManager.CheckForUpdate();

            if (updateInfo.ReleasesToApply.Count > 0)
            {
                this.updateButton.Visible = true;
            }
            else
            {
                this.updateButton.Visible = false;
            }
        }


        private async void updateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.ConvertingFile)
                    return;

                this.updateButton.Text = "Updating";
                await uManager.UpdateApp();
                MessageBox.Show("Update succesful! Application will shutdown.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void WarningLabel_Click(object sender, EventArgs e)
        {

        }

    
    }
}
