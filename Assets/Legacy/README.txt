Legacy Scripts Archive

These scripts are from the original project before the component-based
character system refactor. They are kept for reference only.

Key scripts:
- PlayerController.cs: Old input handling (used Input.GetAxis)
- PController.cs: Alternative player controller
- BossLookDirection.cs: Camera/look logic
- RotateCamera.cs: Camera controls
- ChatGPTController.cs: AI controller
- Timer.cs: Match timer logic
- CrowdAudio.cs: Audio management
- ChangeSkyBoxDay.cs / ChangeSkyBoxNight.cs: Skybox switching
- NightScript.cs / timecontroller.cs: Time/lighting controls

Do not modify these files. New implementations are in Assets/Knockout/Scripts/

These scripts will not be used in the new architecture which follows a
component-based design pattern as defined in docs/plans/Phase-0.md.
