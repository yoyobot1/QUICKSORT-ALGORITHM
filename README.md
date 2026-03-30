🌈 QuickSort Visualizer: Neon Shorts Edition
A Python-based automation tool designed to generate mesmerizing, 9:16 vertical visualizations of the QuickSort algorithm. Perfect for YouTube Shorts, TikTok, and Instagram Reels.

This project transforms raw sorting logic into a high-quality sensory experience, combining dynamic rainbow colormaps with synchronized audio sonification.

✨ Key Features
- 📱 Vertical-First Design: Native 720x1280 (9:16) output, no cropping required for mobile platforms.

- 🎨 Dynamic Rainbow Spectrum: Uses the HSV colormap to assign colors based on element value, creating a beautiful "rainbow sort" effect.

- 🎵 Audio Sonification: Generates clean, anti-aliased sine waves for every swap. Each note's frequency is mapped to the value of the element being moved.

- ⚡ High-Efficiency Rendering: Built with Matplotlib's animation API and optimized with FFmpeg for fast video encoding.

- 🚀 API Ready: Includes a Flask endpoint to trigger generations programmatically.


🛠️ Requirements
- Python 3.10+

- FFmpeg (Must be installed and added to your System PATH)

- Python Libraries: matplotlib, numpy, flask



To install dependencies:
-Using Python:
  pip install -r requirements.txt

To run the generator:
-Using Python:
  python app.py


To get the video:
Visit http://localhost:5000/generate in your browser once the script is running, or if the script is not on your machine visit http://[IP_OF_YOUR_MACHINE]:5000/generate
