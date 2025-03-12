# VideoPlayer360

Video player for 360 videos in VR. 

To play a 360° video, place a video file (in the format of equilateral projection) into the folder Assets/StreamingAssets. (changed)
 
## 17.02.2022
- Fixed project to enable HTC Vive to run.
- Loaded 360 videos.

## Known issues 1.0
- Player UI does not show time skips when video is paused, like it does when video is playing.
- Desktop mouse drag works properly in horizontal direction, but it's inverted in vertical direction (not relevant for the present project, but still an issue).

## 11.08.2023
- Known issues 1.0 all fixed
- Set up for Oculus Quest
- New updates for experiment
- video are imported by local path

### New updates for experiment
- New changes in [/Assets/Scripts/VideoPlayerUIController.cs](/Assets/Scripts/VideoPlayerUIController.cs)
- New scripts for experiment:
1. Write interaction log to local txt file: [/Assets/Scripts/DebugToFile.cs](/Assets/Scripts/DebugToFile.cs)
2. Calculate the distance from the camera to the control panel: [/Assets/Scripts/DistanceCalculator.cs](/Assets/Scripts/DistanceCalculator.cs)
3. Control scene jumping: [/Assets/Scripts/SceneJump.cs](/Assets/Scripts/SceneJump.cs)
- New scenes for experiment: [/Assets/Scenes/Experiment/](/Assets/Scenes/Experiment/)

## Notes
- Some local address you need to change:
1. Video local path: In Inspector 
2. Interaction log path: [/Assets/Scripts/DebugToFile.cs](/Assets/Scripts/DebugToFile.cs)
