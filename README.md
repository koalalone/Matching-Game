# Unity Internship Case - Match-3 Game

## 📌 Project Overview
This project is developed as part of a **technical case study** for an internship opportunity. The goal was to build a **Match-3 puzzle game** that meets the requirements outlined in the provided PDF document. The project focuses on **grid-based tile management, deadlock detection, optimized shuffling, and performance improvements.**

## 🎯 Features Implemented
### **1. Grid-Based Tile System**
- A dynamic **grid manager** that initializes and maintains the game board.
- Each tile has **color variations and sprite updates** based on group formations.

### **2. Group Detection & Tile Destruction**
- Implemented a **Flood-Fill algorithm** to detect and remove connected tiles of the same color.
- Tiles above destroyed ones drop to fill empty spaces.

### **3. Deadlock Detection & Smart Shuffle**
- The game continuously checks for possible moves.
- If no valid moves exist, a **custom shuffling algorithm (Random Cluster Shuffle)** rearranges tiles **while ensuring a solvable board**.

### **4. Optimized Performance**
- **Object Pooling**: Instead of destroying and instantiating tiles, a pool is used to reuse tile objects efficiently.
- **Sprite Atlas**: All tile variations are packed into a single sprite atlas to reduce draw calls and optimize GPU usage.
- **Efficient Grid Updates**: Only changed tiles update their sprites instead of refreshing the entire board.

### **5. Additional Visual Improvements**
- **Smooth Tile Movements**: Tiles fall and swap positions with animations instead of instant transitions.

## 🛠 How to Run the Project
1. Open the project in **Unity** (tested with Unity 6 LTS).
2. Click on the GridManager object to change conditions.
3. Press **Play** in the Unity Editor to start the game.
4. Click on tiles to interact and trigger matches.

## 🏎️ Performance Optimizations
- **Reduced Instantiate() and Destroy() calls** using **Object Pooling**.
- **Reduced GPU draw calls** by **using a Sprite Atlas**.
- **Optimized memory usage** by reusing tile objects and avoiding unnecessary list allocations.
- **Deadlock handling** ensures the game always remains playable without excessive re-shuffling.

## 📩 Conclusion
This project successfully implements **a playable Match-3 game** with **optimized performance**. The goal was to not only meet but exceed the internship case requirements, ensuring a professional and scalable implementation.

---
**Author:** Yusuf

