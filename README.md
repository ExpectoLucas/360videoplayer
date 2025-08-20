# Enhancing Attention in 360° Educational Videos: Real-Time Guidance & Heatmap Feedback

## Introduction

This repository contains a Unity project that implements a **real-time guidance interface** and **heatmap feedback system** for 360-degree educational videos in virtual reality. The project extends a 360° video player framework to help direct the viewer’s attention and provide live feedback on what has been watched. The application overlays dual-axis progress bars with **Points of Interest (POI)** markers and real-time gaze heatmaps onto a 360° video, allowing users to see where to look and review which areas of the scene they have covered. It is built with **Unity 2019.4.35f1** and designed for consumer VR headsets (tested on Meta Quest 3), enabling researchers and educators to explore guided viewing experiences in VR.

## Prerequisites

- **Unity 2019.4.35f1** – Use this specific Unity version (2019.4 LTS) for best compatibility.  
- **VR Headset** – A VR head-mounted display such as *Meta Quest 3* (or equivalent Oculus/SteamVR compatible headset).  
- **Unity XR Support** – Ensure **XR Plug-in Management** (Oculus or OpenXR) is enabled in Unity’s Project Settings. The **XR Interaction Toolkit** is used for controller input and VR interaction.

## Setup Instructions

1. **Clone or Download**: Clone this repository or download it as a ZIP and extract it.  
2. **Open in Unity**: Launch Unity Hub, add the project folder, and open it with **Unity 2019.4.35f1**.  
3. **Load the Scene**: Open the **VideoPlayer Guidance** scene.  
4. **Play in Editor**: Press **Play** to test in the Unity Editor.  
   - **Toggle UI**: Use the VR controller’s primary button to show/hide the guidance interface.  
   - **Heatmap Update**: The system logs head-tracking data and use secondary button to update the heatmap.  

## Folder Structure

- **`Assets/Scripts/New/`** – All new scripts developed for this project:  
  - `MovingHandleSliderS.cs` & `MovingHandleSliderB.cs` – Map yaw/pitch to UI sliders and log head rotation data.  
  - `POIHoverDetector.cs` – Adds tooltips/descriptions when hovering over POI markers.  
  - `XRControllerToggleUI.cs` – VR controller input for toggling the UI and saving data.  
  - `TrailDataManager.cs` – Logs time-stamped head-tracking data to JSON files.  
  - `HeatmapManager.cs` – Generates heatmaps from trail data and displays them in VR.  

- **`Assets/TrailData/`** – Contains **head-tracking logs** from experimental sessions:  
  - Subfolders for each participant/session.  
  - `Horizontal.json` – yaw (horizontal) rotation data.  
  - `Vertical.json` – pitch (vertical) rotation data.  

- **`Deliverable/`** – Contains **research documents and data**:  
  - Final Master’s Thesis.  
  - Questionnaire responses.  
  - Defence slide.  

## Acknowledgments

This project builds upon the code framework originally developed by **Shiyi Chen**. Her 360° VR video player implementation provided the foundation for the playback and UI controls in this project. We gratefully acknowledge her contribution.  

This work was conducted as part of a Master’s program in Game and Media Technology. Please see the included thesis document for full academic acknowledgments.  

## License

This project is released under the **MIT License**.  
You are free to use, modify, and distribute the code, provided attribution is given to the original authors. See the `LICENSE` file for details.
