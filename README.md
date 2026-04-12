# 🌈 QuickSort Visualizer: Neon Shorts Edition

[![Python 3.10+](https://img.shields.io/badge/python-3.10+-blue.svg)](https://www.python.org/downloads/)
[![FFmpeg](https://img.shields.io/badge/FFmpeg-Required-green.svg)](https://ffmpeg.org/)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A multi-language automation tool designed to generate mesmerizing, **9:16 vertical visualizations** of the QuickSort algorithm. Perfect for **YouTube Shorts, TikTok, and Instagram Reels.**

This project transforms raw sorting logic into a high-quality sensory experience, combining **dynamic rainbow colormaps** with **synchronized audio sonification**.

---

## 📂 Project Structure

This repository is organized by language, allowing you to explore the QuickSort implementation across different stacks:

* **`/python`** : 🐍 **Current Main Version**. Uses Flask, Matplotlib, and FFmpeg for video generation.
* **`/java`** : ☕ **available**. High-performance implementation.
* **`/ c#`** : ⚡ (Coming Soon) Low-level memory management visualization.

---

## ✨ Key Features (Python Version)

* 📱 **Vertical-First Design**: Native 720x1280 (9:16) output—no cropping required for mobile platforms.
* 🎨 **Dynamic Rainbow Spectrum**: Uses the `HSV` colormap to assign colors based on element value, creating a beautiful "rainbow sort" effect.
* 🎵 **Audio Sonification**: Generates clean, anti-aliased sine waves for every swap. Each note's frequency is mapped to the value of the element being moved.
* ⚡ **High-Efficiency Rendering**: Built with Matplotlib's animation API and optimized with **FFmpeg** for fast video encoding.
* 🚀 **API Ready**: Includes a Flask endpoint to trigger generations programmatically.

---

## 🛠️ Requirements

Before running the Python script, ensure you have:

1.  **Python 3.10+**
2.  **FFmpeg**: Must be installed and added to your **System PATH**.
    * *Windows tip:* Download from [gyan.dev](https://www.gyan.dev/ffmpeg/builds/) and verify with `ffmpeg -version` in your terminal.
3.  **Python Libraries**:
    ```bash
    pip install matplotlib numpy flask
    ```

---

## 🚀 Getting Started (Python)

### 1. Installation
Clone the repository and enter the python directory:
```bash
git clone [https://github.com/yoyobot1/QUICKSORT-ALGORITHM.git](https://github.com/yoyobot1/QUICKSORT-ALGORITHM.git)
cd QUICKSORT-ALGORITHM/python
pip install -r requirements.txt
```

2. Run the Generator
Start the Flask server:
```bash
python app.py
```

3. Generate your Video
Open your browser and visit:

Local: http://localhost:5000/generate
or : https://127.0.0.1:5000/generate

Network: http://[YOUR_IP]:5000/generate

The script will begin rendering. Once finished, the .mp4 file will be sent to your browser automatically.

💡 Pro-Tip for Creators
The variable n = 180 in app.py controls the number of bars:

100-200 bars: Best for fast, high-energy Shorts.

300+ bars: Best for long-form "Oddly Satisfying" videos.

📜 License
Distributed under the Apache License 2.0 . See LICENSE for more information.
