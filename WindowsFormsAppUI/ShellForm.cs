using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsAppUI.Helpers;
using WindowsFormsAppUI.Models;
using Xabe.FFmpeg;

namespace WindowsFormsAppUI
{
    public partial class ShellForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;
        List<FileItem> fileItems = new List<FileItem>();
        double completedFiles = 0;
        bool isCancel = false;

        public ShellForm()
        {
            InitializeComponent();
            InitializeSettings();
            InitializeDataGridView();
            cancellationTokenSource = new CancellationTokenSource();

            this.ResizeBegin += (s, e) =>
            {
                this.Opacity = 0.50;
                this.SuspendLayout();
            };

            this.ResizeEnd += (s, e) =>
            {
                this.Opacity = 1;
                this.ResumeLayout(true);
            };
        }

        private void ShellForm_Load(object sender, EventArgs e)
        {

        }

        private async Task ConvertFile(string inputFile, string inputFormat, int outputFormat, string fileName, DataGridViewRow currentRow, int totalFiles)
        {
            string outputFile = GlobalVariables.iniFile.Read("Output Path", "GeneralSettings");
            bool videoQuality = Convert.ToBoolean(GlobalVariables.iniFile.Read("Video Quality", "VideoSettings"));

            outputFile = Path.Combine(outputFile, fileName) + "." + (Format)outputFormat;

            cancellationTokenSource = new CancellationTokenSource();

            await ConversionHelpers.ConvertMedia(videoQuality, inputFile, outputFile, inputFormat, outputFormat, cancellationTokenSource.Token, currentRow);

            completedFiles++;

            int generalPercent = (int)Math.Round((completedFiles / totalFiles) * 100);
            progressBarControlGeneral.Invoke((MethodInvoker)(() =>
            {
                progressBarControlGeneral.Position = generalPercent;
            }));
        }

        private Dictionary<int, string> GetVideoSupportedFormats()
        {
            Dictionary<int, string> formats = new Dictionary<int, string>()
            {
                {Convert.ToInt32(Format.mp4), Format.mp4.ToString()},
                {Convert.ToInt32(Format.avi), Format.avi.ToString()},
                {169, "mkv"},
                {Convert.ToInt32(Format.mov), Format.mov.ToString()},
                {Convert.ToInt32(Format.flv), Format.flv.ToString()},
                {2, "3gp"},
            };

            return formats;
        }

        private Dictionary<int, string> GetAudioSupportedFormats()
        {
            Dictionary<int, string> formats = new Dictionary<int, string>()
            {
                {Convert.ToInt32(Format.mp3), Format.mp3.ToString()},
                {Convert.ToInt32(Format.mp2), Format.mp2.ToString()},
                {Convert.ToInt32(Format.ogg), Format.ogg.ToString()},
                {Convert.ToInt32(Format.wav), Format.wav.ToString()},
                {Convert.ToInt32(Format.flac), Format.flac.ToString()},
                {Convert.ToInt32(Format.aac), Format.aac.ToString()},
            };

            return formats;
        }

        private string GetFileSizeString(long fileSizeInBytes)
        {
            const int byteConversion = 1024;
            if (fileSizeInBytes >= byteConversion)
            {
                double fileSizeInKB = fileSizeInBytes / byteConversion;
                if (fileSizeInKB >= byteConversion)
                {
                    double fileSizeInMB = fileSizeInKB / byteConversion;
                    return $"{fileSizeInMB:0.##} MB";
                }
                else
                {
                    return $"{fileSizeInKB:0.##} KB";
                }
            }
            else
            {
                return $"{fileSizeInBytes} Bytes";
            }
        }

        private async void buttonConvert_Click(object sender, EventArgs e)
        {
            if (isCancel)
            {
                cancellationTokenSource.Cancel();
            }
            else
            {
                completedFiles = 0;
                buttonOpenFile.Enabled = false;
                buttonOpen.Enabled = false;
                buttonDelete.Enabled = false;

                int selectedFormat = ((KeyValuePair<int, string>)comboBoxFormats.SelectedItem).Key;
                int totalFiles = dataGridViewFiles.SelectedRows.Count;

                foreach (DataGridViewRow row in dataGridViewFiles.SelectedRows)
                {
                    bool isCompleted = (bool)row.Cells["IsCompleted"].Value;

                    if (!isCompleted)
                    {
                        string filePath = row.Cells["FilePath"].Value.ToString();
                        string inputFormat = row.Cells["InputFormat"].Value.ToString();
                        string fileName = row.Cells["FileName"].Value.ToString();

                        await ConvertFile(filePath, inputFormat, selectedFormat, fileName, row, totalFiles);

                        if (progressBarControlGeneral.Position >= 100)
                        {
                            MessageBox.Show("Conversions are complete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                buttonOpenFile.Enabled = true;
                buttonOpen.Enabled = true;
                buttonDelete.Enabled = true;
            }
        }

        public void InitializeSettings()
        {
            if (!File.Exists(Path.Combine(FolderLocations.winFormUIFolderPath, "Settings.ini")))
            {
                return;
            }

            textBoxOutputPath.Text = GlobalVariables.iniFile.Read("Output Path", "GeneralSettings");

            bool videoOutput = Convert.ToBoolean(GlobalVariables.iniFile.Read("Video Output", "VideoSettings"));
            bool videoQuality = Convert.ToBoolean(GlobalVariables.iniFile.Read("Video Quality", "VideoSettings"));
            if (videoOutput)
            {
                radioButtonOnlyVideo.Checked = false;
                radioButtonVideoAndAudioOnly.Checked = true;
            }
            else
            {
                radioButtonOnlyVideo.Checked = true;
                radioButtonVideoAndAudioOnly.Checked = false;
            }

            if (videoQuality)
            {
                radioButtonLowQuality.Checked = false;
                radioButtonHighQuality.Checked = true;
            }
            else
            {
                radioButtonLowQuality.Checked = true;
                radioButtonHighQuality.Checked = false;
            }

            bool channelMP3 = Convert.ToBoolean(GlobalVariables.iniFile.Read("Channel MP3", "AudioSettings"));
            bool channelWAV = Convert.ToBoolean(GlobalVariables.iniFile.Read("Channel WAV", "AudioSettings"));
            comboBoxBitrateMP3.Text = GlobalVariables.iniFile.Read("Bitrate MP3", "AudioSettings");
            comboBoxBitrateWAV.Text = GlobalVariables.iniFile.Read("Bitrate WAV", "AudioSettings");
            comboBoxSampleRateMP3.Text = GlobalVariables.iniFile.Read("Sample Rate MP3", "AudioSettings");
            comboBoxSampleRateWAV.Text = GlobalVariables.iniFile.Read("Sample Rate WAV", "AudioSettings");

            if (channelMP3)
            {
                radioButtonMonoMP3.Checked = false;
                radioButtonStereoMP3.Checked = true;
            }
            else
            {
                radioButtonMonoMP3.Checked = true;
                radioButtonStereoMP3.Checked = false;
            }

            if (channelWAV)
            {
                radioButtonMonoWAV.Checked = false;
                radioButtonStereoWAV.Checked = true;
            }
            else
            {
                radioButtonMonoWAV.Checked = true;
                radioButtonStereoWAV.Checked = false;
            }
        }

        public void InitializeDataGridView()
        {
            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.HeaderText = "";
            idColumn.Name = "Id";
            idColumn.Visible = false;
            dataGridViewFiles.Columns.Add(idColumn);

            DataGridViewTextBoxColumn filePathColumn = new DataGridViewTextBoxColumn();
            filePathColumn.HeaderText = "";
            filePathColumn.Name = "FilePath";
            filePathColumn.Visible = false;
            dataGridViewFiles.Columns.Add(filePathColumn);

            DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn();
            fileNameColumn.HeaderText = "File Name";
            fileNameColumn.Name = "FileName";
            dataGridViewFiles.Columns.Add(fileNameColumn);

            DataGridViewTextBoxColumn fileSizeColumn = new DataGridViewTextBoxColumn();
            fileSizeColumn.HeaderText = "File Size";
            fileSizeColumn.Name = "FileSize";
            dataGridViewFiles.Columns.Add(fileSizeColumn);

            DataGridViewTextBoxColumn inputFormatColumn = new DataGridViewTextBoxColumn();
            inputFormatColumn.HeaderText = "Input Format";
            inputFormatColumn.Name = "InputFormat";
            dataGridViewFiles.Columns.Add(inputFormatColumn);

            DataGridViewTextBoxColumn outputFormatColumn = new DataGridViewTextBoxColumn();
            outputFormatColumn.HeaderText = "Output Format";
            outputFormatColumn.Name = "OutputFormat";
            dataGridViewFiles.Columns.Add(outputFormatColumn);

            DataGridViewProgressColumn progressColumn = new DataGridViewProgressColumn();
            progressColumn.HeaderText = "Progress";
            progressColumn.Name = "Progress";
            dataGridViewFiles.Columns.Add(progressColumn);

            DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn();
            statusColumn.HeaderText = "Status";
            statusColumn.Name = "Status";
            dataGridViewFiles.Columns.Add(statusColumn);

            DataGridViewTextBoxColumn isCompletedColumn = new DataGridViewTextBoxColumn();
            isCompletedColumn.HeaderText = "";
            isCompletedColumn.Name = "IsCompleted";
            isCompletedColumn.Visible = false;
            dataGridViewFiles.Columns.Add(isCompletedColumn);
        }

        public void AddRowGridControl(List<FileItem> fileItems)
        {
            dataGridViewFiles.Rows.Clear();

            foreach (FileItem fileItem in fileItems)
            {
                dataGridViewFiles.Rows.Add(fileItem.Id, fileItem.FilePath, fileItem.FileName, fileItem.FileSize, fileItem.InputFormat, fileItem.OutputFormat, fileItem.Progress, fileItem.Status, false);
            }

            dataGridViewFiles.ClearSelection();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video Dosyaları|*.mp4;*.avi;*.mkv;*.mov;*.flv;*3gp|Ses Dosyaları|*.mp3;*.mp2;*.ogg;*.wav;*.flac;*.aac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int id = fileItems.Count;
                foreach (string fileName in openFileDialog.FileNames)
                {
                    FileInfo fileInfo = new FileInfo(fileName);
                    string fileSize = GetFileSizeString(fileInfo.Length);

                    FileItem fileItem = new FileItem
                    {
                        Id = id,
                        FilePath = fileName,
                        FileName = fileInfo.Name,
                        FileSize = fileSize,
                        InputFormat = fileInfo.Extension.Remove(0, 1),
                        OutputFormat = "",
                        Progress = 0,
                        Status = "Ready"
                    };

                    var checkFile = fileItems.Where(x => x.FilePath == fileItem.FilePath).FirstOrDefault();
                    if (checkFile == null)
                    {
                        fileItems.Add(fileItem);
                    }

                    id++;
                }

                AddRowGridControl(fileItems);
            }

            buttonConvert.Text = "Convert";
            isCancel = false;
            progressBarControlGeneral.Position = 0;
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string outputPath = folderBrowserDialog.SelectedPath;

                    GlobalVariables.iniFile.Write("Output Path", outputPath, "GeneralSettings");

                    textBoxOutputPath.Text = outputPath;
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewFiles.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewFiles.SelectedRows)
                {
                    int id = (int)row.Cells["id"].Value;
                    FileItem fileItem = fileItems.Where(x => x.Id == id).FirstOrDefault();
                    fileItems.Remove(fileItem);

                    dataGridViewFiles.Rows.RemoveAt(row.Index);
                }

                dataGridViewFiles.ClearSelection();
            }
        }

        private void dataGridViewFiles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string inputFormat = dataGridViewFiles.CurrentRow.Cells["InputFormat"].Value.ToString();
            if (inputFormat == "mp4" || inputFormat == "avi" || inputFormat == "mkv" || inputFormat == "mov" || inputFormat == "flv" || inputFormat == "3gp")
            {
                comboBoxFormats.DataSource = comboBoxFormats.DataSource = new BindingSource(GetVideoSupportedFormats(), null);
            }

            if (inputFormat == "mp3" || inputFormat == "mp2" || inputFormat == "ogg" || inputFormat == "wav" || inputFormat == "flac" || inputFormat == "aac")
            {
                comboBoxFormats.DataSource = comboBoxFormats.DataSource = new BindingSource(GetAudioSupportedFormats(), null);
            }
            comboBoxFormats.DisplayMember = "Value";
            comboBoxFormats.ValueMember = "Key";

            bool isCompleted = (bool)dataGridViewFiles.CurrentRow.Cells["IsCompleted"].Value;
            int progress = (int)dataGridViewFiles.CurrentRow.Cells["Progress"].Value;

            if (progress > 0 && progress < 100 && !isCompleted)
            {
                buttonConvert.Text = "Cancel";
                isCancel = true;
            }
            else if (isCompleted)
            {
                buttonConvert.Text = "Convert";
                isCancel = false;
            }
        }

        private void radioButtonOnlyVideo_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Video Output", false.ToString(), "VideoSettings");
        }

        private void radioButtonVideoAndAudioOnly_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Video Output", true.ToString(), "VideoSettings");
        }

        private void radioButtonLowQuality_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Video Quality", false.ToString(), "VideoSettings");
        }

        private void radioButtonHighQuality_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Video Quality", true.ToString(), "VideoSettings");
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (dataGridViewFiles.SelectedRows.Count > 0)
            {
                bool selectedIsCompleted = (bool)dataGridViewFiles.CurrentRow.Cells["IsCompleted"].Value;
                if (selectedIsCompleted)
                {
                    string selectedFilePath = dataGridViewFiles.CurrentRow.Cells["FilePath"].Value.ToString();
                    Process.Start(selectedFilePath);
                }
            }
        }

        private void comboBoxSampleRateWAV_TextChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Sample Rate WAV", comboBoxSampleRateWAV.Text, "AudioSettings");
        }

        private void comboBoxBitrateWAV_TextChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Bitrate WAV", comboBoxBitrateWAV.Text, "AudioSettings");
        }

        private void radioButtonMonoWAV_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Channel WAV", false.ToString(), "AudioSettings");
        }

        private void radioButtonStereoWAV_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Channel WAV", true.ToString(), "AudioSettings");
        }

        private void comboBoxSampleRateMP3_TextChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Sample Rate MP3", comboBoxSampleRateMP3.Text, "AudioSettings");
        }

        private void comboBoxBitrateMP3_TextChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Bitrate MP3", comboBoxBitrateMP3.Text, "AudioSettings");
        }

        private void radioButtonMonoMP3_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Channel MP3", false.ToString(), "AudioSettings");
        }

        private void radioButtonStereoMP3_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVariables.iniFile.Write("Channel MP3", true.ToString(), "AudioSettings");
        }
    }
}
