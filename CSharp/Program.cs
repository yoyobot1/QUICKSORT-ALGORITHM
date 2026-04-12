using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    const int WIDTH = 720;
    const int HEIGHT = 1280;
    const int N = 180;
    const int FPS = 30;
    const int SAMPLE_RATE = 44100;

    static int[] data;
    static int swaps = 0;
    static Process ffmpegProcess;
    static Stream ffmpegInput;
    static MemoryStream audioBuffer = new MemoryStream();

    static void Main()
    {
        Console.WriteLine("Starting C# QuickSort Visualizer...");

        data = new int[N];
        for (int i = 0; i < N; i++) data[i] = i + 1;
        
        // Fisher-Yates Shuffle
        Random rng = new Random();
        for (int i = N - 1; i > 0; i--) {
            int k = rng.Next(i + 1);
            (data[k], data[i]) = (data[i], data[k]);
        }

        // STEP 1: Start FFmpeg to capture VIDEO ONLY
        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = "ffmpeg";
        // Note: No audio input here yet
        ffmpegProcess.StartInfo.Arguments = $"-y -f image2pipe -vcodec png -r {FPS} -i - -c:v libx264 -pix_fmt yuv420p temp_video.mp4";
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardInput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true;
        ffmpegProcess.Start();

        ffmpegInput = ffmpegProcess.StandardInput.BaseStream;

        // Run the sort and pipe frames
        QuickSort(0, N - 1);

        // Final white scan
        for (int i = 0; i < N; i++)
        {
            RenderFrame(i, true);
            GenerateBeep(300 + ((double)data[i] / N) * 600, 1.0 / FPS);
        }

        // Close video pipe and wait for FFmpeg to finish temp_video.mp4
        ffmpegInput.Close();
        ffmpegProcess.WaitForExit();

        // STEP 2: Save the audio file
        Console.WriteLine("Video stream captured. Saving audio...");
        SaveAudioFile();

        // STEP 3: Final Merge (Video + Audio)
        Console.WriteLine("Merging audio and video...");
        Process mergeProcess = new Process();
        mergeProcess.StartInfo.FileName = "ffmpeg";
        mergeProcess.StartInfo.Arguments = $"-y -i temp_video.mp4 -i temp_audio.wav -c:v copy -c:a aac -b:a 192k quicksort_csharp_final.mp4";
        mergeProcess.Start();
        mergeProcess.WaitForExit();

        // Cleanup
        if (File.Exists("temp_video.mp4")) File.Delete("temp_video.mp4");
        if (File.Exists("temp_audio.wav")) File.Delete("temp_audio.wav");

        Console.WriteLine("✨ Success! Video saved as: quicksort_csharp_final.mp4");
    }

    static void QuickSort(int start, int end)
    {
        if (start >= end) return;
        int pivot = data[end];
        int low = start;

        for (int i = start; i < end; i++)
        {
            if (data[i] < pivot)
            {
                (data[i], data[low]) = (data[low], data[i]);
                low++;
                swaps++;
                RenderFrame(i, false);
                GenerateBeep(150 + ((double)data[i] / N) * 1000, 1.0 / FPS);
            }
        }
        (data[low], data[end]) = (data[end], data[low]);
        swaps++;
        RenderFrame(low, false);
        GenerateBeep(150 + ((double)data[low] / N) * 1000, 1.0 / FPS);

        QuickSort(start, low - 1);
        QuickSort(low + 1, end);
    }

    static void RenderFrame(int highlightIdx, bool isFinal)
    {
        using Bitmap bmp = new Bitmap(WIDTH, HEIGHT);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Black);

        using Font titleFont = new Font("Arial", 24, FontStyle.Bold);
        using Font subFont = new Font("Arial", 16, FontStyle.Bold);
        
        g.DrawString("QUICKSORT ALGORITHM", titleFont, Brushes.White, new PointF(150, 60));
        using SolidBrush cyanBrush = new SolidBrush(Color.FromArgb(0, 255, 204));
        g.DrawString($"{N} BARS", subFont, cyanBrush, new PointF(310, 110));
        g.DrawString($"SWAPS: {swaps}", subFont, Brushes.White, new PointF(300, 150));

        float barWidth = (float)WIDTH / N;
        for (int i = 0; i < N; i++)
        {
            float hue = (float)data[i] / N * 360f; 
            Color barColor = ColorFromHSV(hue, 1.0f, 1.0f);

            if (isFinal)
                barColor = i <= highlightIdx ? Color.White : barColor;
            else if (i == highlightIdx)
                barColor = Color.White;

            using SolidBrush brush = new SolidBrush(barColor);
            int barHeight = (int)((data[i] / (double)N) * (HEIGHT * 0.7));
            g.FillRectangle(brush, i * barWidth, HEIGHT - barHeight, (float)Math.Ceiling(barWidth), barHeight);
        }

        bmp.Save(ffmpegInput, ImageFormat.Png);
    }

    static void GenerateBeep(double frequency, double duration)
    {
        int length = (int)(SAMPLE_RATE * duration);
        byte[] buffer = new byte[length * 2];

        for (int i = 0; i < length; i++)
        {
            double time = i / (double)SAMPLE_RATE;
            double angle = 2.0 * Math.PI * frequency * time;

            double envelope = 1.0;
            int fadeLen = (int)(length * 0.1);
            if (i < fadeLen) envelope = (double)i / fadeLen;
            else if (i > length - fadeLen) envelope = (double)(length - i) / fadeLen;

            short sample = (short)(Math.Sin(angle) * envelope * 0.15 * 32767);
            byte[] bytes = BitConverter.GetBytes(sample);
            buffer[2 * i] = bytes[0];
            buffer[2 * i + 1] = bytes[1];
        }
        audioBuffer.Write(buffer, 0, buffer.Length);
    }

    static void SaveAudioFile()
    {
        using FileStream fs = new FileStream("temp_audio.wav", FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(fs);
        
        byte[] audioData = audioBuffer.ToArray();
        
        bw.Write(new[] { 'R', 'I', 'F', 'F' });
        bw.Write(36 + audioData.Length);
        bw.Write(new[] { 'W', 'A', 'V', 'E' });
        bw.Write(new[] { 'f', 'm', 't', ' ' });
        bw.Write(16); 
        bw.Write((short)1); 
        bw.Write((short)1); 
        bw.Write(SAMPLE_RATE);
        bw.Write(SAMPLE_RATE * 2); 
        bw.Write((short)2); 
        bw.Write((short)16); 
        bw.Write(new[] { 'd', 'a', 't', 'a' });
        bw.Write(audioData.Length);
        bw.Write(audioData);
    }

    static Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);
        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0) return Color.FromArgb(255, v, t, p);
        else if (hi == 1) return Color.FromArgb(255, q, v, p);
        else if (hi == 2) return Color.FromArgb(255, p, v, t);
        else if (hi == 3) return Color.FromArgb(255, p, q, v);
        else if (hi == 4) return Color.FromArgb(255, t, p, v);
        else return Color.FromArgb(255, v, p, q);
    }
}