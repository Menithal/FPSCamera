# Menithal's FPS Camera

This is a simplified version of the ActionCamera Plugin for LivVR

This allows you show off your Liv Avatar while playing a regular VR Game like and FPS. 
Has three modes

- ThirdPerson Over the SHoulder
- First Person from Eye
- ADS 

See License.

## Installation

Make sure you are running LIV SDK2 build (1.3.7+)
Install by moving the ActionCamera.dll from Releases into your Liv Plugins CameraBehaviours directory at

`%HOMEPATH%/Documents/Liv/Plugins/CameraBehaviours/`

## Use
When in Liv, Set an Avatar Camera, and make sure to select Plugin > "Menithal' FPS Camera" to start using the plugin.
Closing Liv now wil update your settings file. You can then create and configure multiple profiles with different configurations in Liv and modifying the json file. See Configuration for more detail


### Available Cameras

- *OverShoulderAction* - Main feature of the plugin, Shows point of view over your shoulder. Looking around corners will always move the camera around to that shoulder towards where you are looking at, allowing your spectators to see
what you will see before you do.  You can reverse this with the `reverseShoulder` config.
- *FirstPerson* - FPS view of the game. Smoothened, and similar to how the game would play, but you can turn on avatar visibility with `removeAvatarInsteadOfHead`.  
Can be turned off with `disableFPSCamera`. You can use `useEyePosition` and `rightEyeDominant` to define the view from a specific eye. By Default uses Eye Position with Right Eye dominance.
- *ADS/GunCam/Sights* - Down Sights view of the game when holding a weapon two handed, and looking down the sights. Shows avatar body if 
`removeAvatarInsteadOfHead` is disabled.  Can be turned off with `disableGunCamera`.  
Uses `rightEyeDominant` to define the view from a specific eye. By Default uses Right Eye dominance.


### Controlling Cameras and Gestures

You direct the camera direction with head movement (for now) with your controllers behaving as keylocks You must be mostly pointing forwards with your controllers for commands to work.. 

- Move Arms close pointing ahead of you like holding a VR gun triggers FPS view.
- If you look down sights it will go into ADS mode
- If you lower you gun and look around it will go shoulder view.


## Configuring

Configurable after setting as a plugin for a camera, and closing Liv Composer AND App. It is a bit fiddly, but with sufficient configuration you can do quite a bit.

You can find the settings at
`%LOCALAPPDATA%/Liv/App/<LivVersion>.json`

## Default Setting Example: 
```
[...]
"pluginCameraBehaviourSettings": {
    "selectedPluginCameraBehaviourID": "MFPSCamera",
    "pluginSettings": {
        "ActionCamera": {
            "cameraSwapTimeLock": 3,
            "cameraPositionTimeLock": 0.4,
            "controlMovementThreshold": 2,
            "cameraShoulderAngle": 35,
            "cameraShoulderSensitivity": 2,

            "cameraShoulderPositioningTime": 0.8,
            "cameraShoulderDistance": 2,
            "cameraBodyVerticalTargetOffset": 0.5,
           
            "rightHandDominant": true,
            "rightEyeDominant": true,
            "useEyePosition": true,
            "removeAvatarInsteadOfHead": true,
            "cameraDefaultFov": 80,
            "cameraGunFov": 80,
            "cameraFovLerp": false
        }
    }
}
[...]
```

There are other configurations too, but they are not that necessary to configure

### Configurables: 

To be filled


### Contributing, Developing and Building

Note if building using DEBUG, a textfile will be output to `%HOMEPATH%/Documents/Liv/Output` 
and written into with debug messages. 

When building a release, make sure NOT to have the DEBUG flag set, otherwise the debug file will be filled to brim. We do not want to flood end users disks with logs.

### Bug Reports 

You can comments, suggestions, bug reports to me over Discord Malactus#3957 or just leave them as Github Issues
