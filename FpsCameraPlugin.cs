using UnityEngine;
using System;
using System.Linq;
using LIV.Avatar;

public class FPSCameraSettings : IPluginSettings
{
    public float cameraSwapTimeLock = 3f;
    // actionCamera can swap faster than general Camera and people tend to change look directions quite quickly.
    public float cameraPositionTimeLock = 0.4f;

    public float controlVerticalMovementThreshold = 2f; // Meters per framea
    // How fast should we swap between 
    public bool reverseShoulder = false;
    public float cameraShoulderAngle = 35;
    public float cameraShoulderSensitivity = 2f;
    public float cameraShoulderPositioningTime = 0.8f;
    public float cameraShoulderDistance = 2f;
    public float cameraBodyVerticalTargetOffset = 0.5f;

    public bool rightHandDominant = true;
    public bool rightEyeDominant = true;
    public bool useEyePosition = true;

    public bool inBetweenCameraEnabled =  false;
    public bool cameraVerticalLock = false;
    public float forwardVerticalOffset = 0f;
    public float forwardHorizontalOffset = 5f;
    public float forwardDistance = 5f;
    public float controlMovementThreshold = 1f; // Meters per framea

    public bool removeAvatarInsteadOfHead = true;
    public float cameraDefaultFov = 80f;

    public float cameraGunFov = 80f;
    public bool cameraFovLerp = false;

    public float cameraGunHeadAlignAngleTrigger = 20f;
    public float cameraGunHeadDistanceTrigger = 0.5f;
    public float cameraGunEyeVerticalOffset = 0.15f;
    public float cameraGunMaxTwoHandedDistance = 0.8f;
    public float cameraGunMinTwoHandedDistance = 0.15f;
    public float cameraGunSmoothing = 0.3f;

}
namespace FPSCamera
{
    public class FPSCameraPlugin : IPluginCameraBehaviour
    {
        public IPluginSettings settings => _settings;
        // Matching naming schema
        FPSCameraSettings _settings = new FPSCameraSettings();

        public event EventHandler ApplySettings;

        public string name => "Menithal's FPS Camera";
        public string author => "MA 'Menithal' Lahtinen";
        public string version => "0.1.0";
        public string ID => "MFPSCamera";
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

        private FPSCameraDirector cameraDirector;

        public void OnActivate(PluginCameraHelper helper)
        {
            PluginLog.Log(ID, "OnActivate");

            timerHelper = new TimerHelper();
            cameraDirector = new FPSCameraDirector(_settings, helper, ref timerHelper);

            AvatarManager avatarManager = Resources.FindObjectsOfTypeAll<AvatarManager>().FirstOrDefault();
            avatarRefSignal = avatarManager?.GetPrivateField<AvatarReferenceSignal>("_avatarInstantiated");
            avatarRefSignal?.OnChanged.AddListener(OnAvatarChanged);

            OnAvatarChanged(avatarRefSignal?.Value);
        }

        // Called when Settings are deserialized (read) from file.
        public void OnSettingsDeserialized()
        {
            PluginLog.Log(ID, "OnSettingsDeserialized");
            PluginLog.Log(ID, "generalCameraSwapClamp " + _settings.cameraSwapTimeLock);
            PluginLog.Log(ID, "actionCameraSwapClamp " + _settings.cameraPositionTimeLock);
            PluginLog.Log(ID, "controlMovementThreshold " + _settings.controlMovementThreshold);
            PluginLog.Log(ID, "controlVerticalMovementThreshold " + _settings.controlVerticalMovementThreshold);

            PluginLog.Log(ID, "cameraVerticalLock " + _settings.cameraVerticalLock);
            PluginLog.Log(ID, "cameraShoulderDistance " + _settings.cameraShoulderDistance);
            PluginLog.Log(ID, "cameraShoulderAngle " + _settings.cameraShoulderAngle);
            PluginLog.Log(ID, "cameraShoulderPositioningTime " + _settings.cameraShoulderPositioningTime);

            PluginLog.Log(ID, "reverseShoulder " + _settings.reverseShoulder);
            PluginLog.Log(ID, "forwardVerticalOffset " + _settings.forwardVerticalOffset);
            PluginLog.Log(ID, "forwardHorizontalOffset " + _settings.forwardHorizontalOffset);
            PluginLog.Log(ID, "forwardDistance " + _settings.forwardDistance);
            PluginLog.Log(ID, "removeAvatarInsteadOfHead " + _settings.removeAvatarInsteadOfHead);

            PluginLog.Log(ID, "inBetweenCameraEnabled " + _settings.inBetweenCameraEnabled);
            PluginLog.Log(ID, "rightHandDominant " + _settings.rightHandDominant);
            // Need to make sure everything else is updated

            PluginLog.Log(ID, "cameraGunFov " + _settings.cameraGunFov);
            PluginLog.Log(ID, "cameraGunHeadAlignAngleTrigger " + _settings.cameraGunHeadAlignAngleTrigger);
            PluginLog.Log(ID, "cameraGunHeadDistanceTrigger " + _settings.cameraGunHeadDistanceTrigger);
            PluginLog.Log(ID, "cameraGunEyeVerticalOffset " + _settings.cameraGunEyeVerticalOffset);
            PluginLog.Log(ID, "cameraGunMaxTwoHandedDistance " + _settings.cameraGunMaxTwoHandedDistance);
            PluginLog.Log(ID, "cameraGunMinTwoHandedDistance " + _settings.cameraGunMinTwoHandedDistance);
            PluginLog.Log(ID, "cameraGunSmoothing " + _settings.cameraGunSmoothing);

            cameraDirector.SetSettings(_settings);
        }

        public void OnFixedUpdate()
        {
        }
        public void OnAvatarChanged(Avatar avatar)
        {
            PluginLog.Log(ID, "OnAvatarChanged ");
            cameraDirector.SetAvatar(avatar);
        }

        // Called Every Frame.
        public void OnUpdate()
        {
            timerHelper.AddTime(Time.deltaTime);
            cameraDirector.SelectCamera();
            cameraDirector.HandleCameraView();
        }

        public void OnLateUpdate()
        {
        }

        public void OnDeactivate()
        {
            PluginLog.Log(ID, "OnDeactivate ");

            ApplySettings?.Invoke(this, EventArgs.Empty);

            avatarRefSignal?.OnChanged.RemoveListener(OnAvatarChanged);
            avatarRefSignal = null;
        }

        public void OnDestroy()
        {
            PluginLog.Log(ID, "OnDestroy");
        }
    }

}
