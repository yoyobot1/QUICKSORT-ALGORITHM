import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import matplotlib.cm as cm
import numpy as np
import os, wave, subprocess
from flask import Flask, send_file

app = Flask(__name__)
# Update this path to your local directory
BASE_PATH = r'C:\Users\yoyobot\Documents\IA_YOUTUBE'

def generate_quicksort_short():
    n = 180  # Number of bars
    fps = 30
    sample_rate = 44100
    duration_per_frame = 1.0 / fps
    
    temp_video = os.path.join(BASE_PATH, 'temp_video.mp4')
    temp_audio = os.path.join(BASE_PATH, 'temp_audio.wav')
    final_output = os.path.join(BASE_PATH, 'quicksort_final_english.mp4')

    # Initialize shuffled data
    data = np.arange(1, n + 1)
    np.random.shuffle(data)
    audio_frames = []
    
    # --- Vertical Format Setup (9:16) ---
    fig, ax = plt.subplots(figsize=(9, 16), facecolor='black')
    
    # Map values to the HSV Rainbow Colormap
    initial_colors = cm.hsv(data / n)
    bar_rects = ax.bar(range(n), data, align="edge", color=initial_colors, width=0.8)
    
    ax.set_facecolor('black')
    ax.set_ylim(0, int(n * 1.2))
    ax.set_xlim(0, n)
    plt.axis('off')

    # --- UI TEXT LABELS ---
    ax.text(0.5, 0.96, "QUICKSORT ALGORITHM", transform=ax.transAxes, 
            color='white', fontsize=22, fontweight='bold', ha='center')
    
    # Display the number of elements
    ax.text(0.5, 0.93, f"{n} BARS", transform=ax.transAxes, 
            color='#00FFCC', fontsize=16, fontweight='bold', ha='center')
    
    # Dynamic swap counter
    count_text = ax.text(0.5, 0.90, "SWAPS: 0", transform=ax.transAxes, 
                         color='white', fontsize=18, ha='center')

    stats = {"swaps": 0}

    # --- AUDIO GENERATION (CORRECTED) ---
    def generate_beep(frequency, duration):
        """Generates a clean sine wave beep with an envelope to prevent clicks."""
        t = np.linspace(0, duration, int(sample_rate * duration), False)
        # Generate the sine wave
        wave_data = np.sin(2 * np.pi * frequency * t)
        
        # Apply Envelope (10% Fade in/out) to eliminate digital 'clicks'
        envelope = np.ones_like(wave_data)
        fade_len = int(len(wave_data) * 0.1)
        envelope[:fade_len] = np.linspace(0, 1, fade_len)
        envelope[-fade_len:] = np.linspace(1, 0, fade_len)
        
        return (wave_data * envelope * 0.15 * 32767).astype(np.int16)

    def update(frame_data, rects):
        arr, pivot_idx, is_final, current_swaps = frame_data
        count_text.set_text(f"SWAPS: {current_swaps}")
        
        for i, (rect, val) in enumerate(zip(rects, arr)):
            rect.set_height(val)
            if is_final:
                # Turn white during the final "success" scan
                rect.set_color("#FFFFFF") if i <= pivot_idx else rect.set_color(cm.hsv(val / n))
            elif i == pivot_idx:
                # Highlight the current pivot in white
                rect.set_color("#FFFFFF")
            else:
                # Default rainbow color based on height
                rect.set_color(cm.hsv(val / n))

    def quicksort(arr, start, end):
        if start >= end: return
        pivot = arr[end]
        low = start
        for i in range(start, end):
            if arr[i] < pivot:
                arr[i], arr[low] = arr[low], arr[i]
                low += 1
                stats["swaps"] += 1
                # Map height to frequency (Pitch)
                freq = 150 + (arr[i] / n) * 1000
                audio_frames.append(generate_beep(freq, duration_per_frame))
                yield (arr.copy(), i, False, stats["swaps"])
        
        arr[low], arr[end] = arr[end], arr[low]
        stats["swaps"] += 1
        yield from quicksort(arr, start, low - 1)
        yield from quicksort(arr, low + 1, end)

    def main_generator():
        arr = data.copy()
        yield from quicksort(arr, 0, n - 1)
        
        # Final scan animation
        for i in range(n):
            freq = 300 + (arr[i]/n)*600
            audio_frames.append(generate_beep(freq, duration_per_frame))
            yield (arr.copy(), i, True, stats["swaps"])

    # Create the animation
    anim = animation.FuncAnimation(fig, update, fargs=(bar_rects,), 
                                   frames=main_generator, interval=1000/fps, cache_frame_data=False)
    
    print(f"Generating Video ({n} bars)...")
    anim.save(temp_video, writer='ffmpeg', fps=fps, dpi=120)
    plt.close()

    # Write the audio file
    print("Writing Audio file...")
    with wave.open(temp_audio, 'w') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(sample_rate)
        for frame in audio_frames:
            f.writeframes(frame.tobytes())

    # Final Merge using FFmpeg
    print("Merging Audio and Video...")
    subprocess.run(f'ffmpeg -y -i "{temp_video}" -i "{temp_audio}" -c:v libx264 -pix_fmt yuv420p -c:a aac -b:a 192k "{final_output}"', shell=True)
    
    print("Done!")
    return final_output

@app.route('/generate', methods=['GET'])
def start_generation():
    try:
        path = generate_quicksort_short()
        return send_file(path, as_attachment=True)
    except Exception as e:
        return str(e), 500

if __name__ == '__main__':
    # Default Flask port
    app.run(host='0.0.0.0', port=5000)