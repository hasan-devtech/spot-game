# Spot The Difference - C# Desktop App 🔍

A robust desktop puzzle application built with **C# and .NET**. This project demonstrates mastery of **Windows Forms (or WPF)**, utilizing coordinate-based event handling to validate user interactions against visual data.

## 🧠 Engineering Logic

While the concept is simple, the implementation features structured logic:

### 1. Coordinate Validation Engine
- **Hit-Box Detection:** Instead of basic button clicks, the app maps specific `(x, y)` coordinate regions on the image to validate "correct" differences.
- **Penalty System:** Logic to track invalid clicks and deduct "lives".

### 2. UI State Management
- **Dynamic Updates:** Real-time updating of scoreboards, timers, and visual markers without UI freezing.
- **Resource Management:** Efficient loading and disposing of high-resolution image bitmaps to prevent memory leaks.

## 🛠️ Tech Stack

- **Language:** C# (.NET Framework / .NET Core)
- **Framework:** Windows Forms (WinForms) / WPF
- **IDE:** Visual Studio
- **Concepts:** Event-Driven Programming, Object-Oriented Design

## 🎮 Features

- **Timer Logic:** Countdown system triggering "Game Over" events.
- **Visual Feedback:** Drawing circles/markers dynamically on the canvas when a difference is found.
- **Level System:** Scalable architecture allowing new image sets (Levels) to be added easily via a configuration file or array.

## 🚀 How to Run

1. **Clone the Repo**
   ```bash
   git clone [https://github.com/hasan-devtech/spot-the-difference.git](https://github.com/hasan-devtech/spot-the-difference.git)
