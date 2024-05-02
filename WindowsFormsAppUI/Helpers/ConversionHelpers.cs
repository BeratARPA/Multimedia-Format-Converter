using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace WindowsFormsAppUI.Helpers
{
    public class ConversionHelpers
    {
        public async static Task ConvertMedia(bool videoQuality, string inputFile, string outputFile, string inputFormat, int outputFormat, CancellationToken cancellationToken, DataGridViewRow currentRow)
        {
            try
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                currentRow.Cells["Status"].Value = "Being Converted";

                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full, Application.StartupPath);

                FFmpeg.SetExecutablesPath(Application.StartupPath);

                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFile);

                IStream videoStream = null;
                if (videoQuality)
                {
                    videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                       ?.SetSize(VideoSize.Hd1080);
                }
                else
                {
                    videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                       ?.SetSize(VideoSize.Hd480);
                }

                IStream audioStream = null;
                if ((Format)outputFormat == (Format)176)
                {
                    bool channelMP3 = Convert.ToBoolean(GlobalVariables.iniFile.Read("Channel MP3", "AudioSettings"));                   
                    int bitrateMP3 = Convert.ToInt32(GlobalVariables.iniFile.Read("Bitrate MP3", "AudioSettings"));             
                    long sampleRateMP3 = Convert.ToInt64(GlobalVariables.iniFile.Read("Sample Rate MP3", "AudioSettings"));
   
                    audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                        ?.SetBitrate(sampleRateMP3)
                        ?.SetSampleRate(bitrateMP3)                     
                        ?.SetChannels(channelMP3 == true ? 2 : 1);
                }
                else if ((Format)outputFormat == (Format)325)
                {
                    bool channelWAV = Convert.ToBoolean(GlobalVariables.iniFile.Read("Channel WAV", "AudioSettings"));
                    int bitrateWAV = Convert.ToInt32(GlobalVariables.iniFile.Read("Bitrate WAV", "AudioSettings"));
                    long sampleRateWAV = Convert.ToInt64(GlobalVariables.iniFile.Read("Sample Rate WAV", "AudioSettings"));

                    audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                     ?.SetBitrate(sampleRateWAV)
                     ?.SetSampleRate(bitrateWAV)                 
                     ?.SetChannels(channelWAV == true ? 2 : 1);
                }

                if (videoStream == null && audioStream == null)
                {
                    throw new Exception("No valid video or audio streams found.");
                }

                IConversion conversion = FFmpeg.Conversions.New()
                   .AddStream(videoStream, audioStream)
                   .SetOutput(outputFile)
                   .SetInputFormat(inputFormat)
                   .SetOutputFormat((Format)outputFormat);

                currentRow.Cells["OutputFormat"].Value = (Format)outputFormat;

                conversion.OnProgress += async (sender, args) =>
                {
                    currentRow.Cells["Progress"].Value = args.Percent;
                };

                await conversion.Start(cancellationToken);

                currentRow.Cells["Status"].Value = "Completed";
                currentRow.Cells["IsCompleted"].Value = true;
            }
            catch (OperationCanceledException)
            {
                currentRow.Cells["Status"].Value = "It is cancelled";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
