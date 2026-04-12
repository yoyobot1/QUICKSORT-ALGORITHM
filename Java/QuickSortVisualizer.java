import java.awt.Color;
import java.awt.Font;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.io.*;
import java.util.Random;
import javax.imageio.ImageIO;
import javax.sound.sampled.*;

public class QuickSortVisualizer {
    static final int WIDTH = 720;
    static final int HEIGHT = 1280;
    static final int N = 300;
    static final int FPS = 30;
    static final int SAMPLE_RATE = 44100;
    
    static int[] data;
    static int swaps = 0;
    static Process ffmpegProcess;
    static OutputStream ffmpegInput;
    static ByteArrayOutputStream audioBuffer = new ByteArrayOutputStream();

    public static void main(String[] args) throws Exception {
        System.out.println("Starting Java QuickSort Visualizer...");
        
        // 1. Initialize data
        data = new int[N];
        for (int i = 0; i < N; i++) data[i] = i + 1;
        shuffleArray(data);

        // 2. Start FFmpeg to record ONLY the video first
        // We output to 'temp_video.mp4'
        ProcessBuilder pb = new ProcessBuilder(
            "ffmpeg", "-y", "-f", "image2pipe", "-vcodec", "png", "-r", String.valueOf(FPS),
            "-i", "-", "-c:v", "libx264", "-pix_fmt", "yuv420p", "temp_video.mp4"
        );
        
        pb.redirectErrorStream(true);
        ffmpegProcess = pb.start();
        ffmpegInput = ffmpegProcess.getOutputStream();

        // Standard output consumer to prevent hanging
        new Thread(() -> {
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(ffmpegProcess.getInputStream()))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    // Optional: System.out.println(line); // Uncomment for debugging
                } 
            } catch (IOException e) {}
        }).start();

        // 3. Run Sort & Render
        quickSort(0, N - 1);

        // Final white sweep
        for (int i = 0; i < N; i++) {
            renderFrame(i, true);
            generateBeep(300 + ((double) data[i] / N) * 600, 1.0 / FPS);
        }

        // 4. Close Video Stream and Save Audio
        ffmpegInput.flush();
        ffmpegInput.close();
        ffmpegProcess.waitFor(); // Wait for video to finish encoding
        
        System.out.println("Video stream captured. Saving audio...");
        saveAudioFile();

        // 5. Final Merge (Video + Audio)
        System.out.println("Merging audio and video...");
        ProcessBuilder mergePb = new ProcessBuilder(
            "ffmpeg", "-y", "-i", "temp_video.mp4", "-i", "temp_audio.wav", 
            "-c:v", "copy", "-c:a", "aac", "-b:a", "192k", "quicksort_java_final.mp4"
        );
        mergePb.inheritIO().start().waitFor();
        
        // Cleanup
        new File("temp_video.mp4").delete();
        new File("temp_audio.wav").delete();
        
        System.out.println("✨ Success! Video saved as: quicksort_java_final.mp4");
    }
    static void shuffleArray(int[] array) {
        Random rnd = new Random();
        for (int i = array.length - 1; i > 0; i--) {
            int index = rnd.nextInt(i + 1);
            // Simple swap
            int a = array[index];
            array[index] = array[i];
            array[i] = a;
        }
    }

    static void quickSort(int start, int end) throws Exception {
        if (start >= end) return;
        int pivot = data[end];
        int low = start;
        for (int i = start; i < end; i++) {
            if (data[i] < pivot) {
                swap(i, low);
                low++;
                swaps++;
                renderFrame(i, false);
                generateBeep(150 + ((double) data[i] / N) * 1000, 1.0 / FPS);
            }
        }
        swap(low, end);
        swaps++;
        renderFrame(low, false);
        generateBeep(150 + ((double) data[low] / N) * 1000, 1.0 / FPS);
        
        quickSort(start, low - 1);
        quickSort(low + 1, end);
    }

    static void swap(int i, int j) {
        int temp = data[i];
        data[i] = data[j];
        data[j] = temp;
    }

    static void renderFrame(int highlightIdx, boolean isFinal) throws Exception {
        BufferedImage img = new BufferedImage(WIDTH, HEIGHT, BufferedImage.TYPE_INT_RGB);
        Graphics2D g = img.createGraphics();
        
        g.setColor(Color.BLACK);
        g.fillRect(0, 0, WIDTH, HEIGHT);
        
        g.setFont(new Font("Arial", Font.BOLD, 36));
        g.setColor(Color.WHITE);
        g.drawString("QUICKSORT ALGORITHM", WIDTH / 2 - 230, 80);
        g.setColor(new Color(0, 255, 204));
        g.drawString(N + " BARS", WIDTH / 2 - 70, 130);
        g.setColor(Color.WHITE);
        g.drawString("SWAPS: " + swaps, WIDTH / 2 - 90, 180);

        float barWidth = (float) WIDTH / N;
        for (int i = 0; i < N; i++) {
            float hue = (float) data[i] / N;
            Color barColor = Color.getHSBColor(hue, 1.0f, 1.0f);
            
            if (isFinal) {
                barColor = (i <= highlightIdx) ? Color.WHITE : barColor;
            } else if (i == highlightIdx) {
                barColor = Color.WHITE;
            }
            
            g.setColor(barColor);
            int barHeight = (int) ((data[i] / (double) N) * (HEIGHT * 0.7));
            g.fillRect((int) (i * barWidth), HEIGHT - barHeight, (int) Math.ceil(barWidth), barHeight);
        }
        g.dispose();
        ImageIO.write(img, "png", ffmpegInput);
    }

    static void generateBeep(double frequency, double duration) throws Exception {
        int length = (int) (SAMPLE_RATE * duration);
        byte[] audioData = new byte[length * 2];
        for (int i = 0; i < length; i++) {
            double time = i / (double) SAMPLE_RATE;
            double angle = 2.0 * Math.PI * frequency * time;
            
            // Fade in/out envelope to prevent clicking
            double envelope = 1.0;
            int fadeLen = (int) (length * 0.1);
            if (i < fadeLen) envelope = (double) i / fadeLen;
            else if (i > length - fadeLen) envelope = (double) (length - i) / fadeLen;

            short sample = (short) (Math.sin(angle) * envelope * 0.15 * 32767);
            audioData[2 * i] = (byte) (sample & 0xFF);
            audioData[2 * i + 1] = (byte) ((sample >> 8) & 0xFF);
        }
        audioBuffer.write(audioData);
    }

    static void saveAudioFile() throws Exception {
        byte[] audioData = audioBuffer.toByteArray();
        AudioFormat format = new AudioFormat(SAMPLE_RATE, 16, 1, true, false);
        ByteArrayInputStream bais = new ByteArrayInputStream(audioData);
        AudioInputStream ais = new AudioInputStream(bais, format, audioData.length / 2);
        AudioSystem.write(ais, AudioFileFormat.Type.WAVE, new File("temp_audio.wav"));
    }
}