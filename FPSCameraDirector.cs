using UnityEngine;
using LIV.Avatar;

namespace FPSCamera
{

    public class FPSCameraDirector
    {
        public readonly FPSCamera FPSCamera;
        public readonly FPSPluginCamera OverShoulderCamera;

        private readonly PluginCameraHelper cameraHelper;
        private readonly TimerHelper timerHelper;

        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private Vector3 cameraLookAtTarget;


        private Vector3 cameraLastLookAtVelocity = Vector3.zero;
        private Vector3 cameraLastVelocity = Vector3.zero;

        private Vector3 cameraLookAtVelocity = Vector3.zero;
        private Vector3 cameraVelocity = Vector3.zero;
        private Vector3 cameraPositionTarget = Vector3.zero;

        private FPSPluginCamera currentCamera;
        private FPSPluginCamera lastCamera;
        private FPSCameraSettings pluginSettings;
        private Avatar currentAvatar;

        private readonly LivPlayerEntity player;
        private readonly System.Random randomizer;
        private bool isCameraStatic = false;

        private static float CONTROLLER_THROTTLE = 0.5f;

        public FPSCameraDirector(FPSCameraSettings pluginSettings, PluginCameraHelper helper, ref TimerHelper timerHelper)
        {
            this.player = new LivPlayerEntity(helper, ref timerHelper);
            this.timerHelper = timerHelper;
            this.cameraHelper = helper;
            this.pluginSettings = pluginSettings;

            randomizer = new System.Random();
            player.SetOffsets(pluginSettings.forwardHorizontalOffset, pluginSettings.forwardVerticalOffset, pluginSettings.forwardDistance);

            FPSCamera = new FPSCamera(pluginSettings, 0.2f);
            OverShoulderCamera = new ShoulderActionCamera(pluginSettings);

            SetCamera(OverShoulderCamera);
        }
        public void SetAvatar(Avatar avatar)
        {
            currentAvatar = avatar;
        }
        public void SetSettings(FPSCameraSettings settings)
        {
            pluginSettings = settings;
            player.SetOffsets(settings.forwardHorizontalOffset, settings.forwardVerticalOffset, settings.forwardDistance);

            FPSCamera.SetPluginSettings(settings);
            OverShoulderCamera.SetPluginSettings(settings);
        }

        private void SetCamera(FPSPluginCamera camera, bool saveLast = true, float timerOverride = 0)
        {
            if (currentCamera != camera)
            {
                if (saveLast)
                {
                    lastCamera = currentCamera;
                }
                currentCamera = camera;
                isCameraStatic = false;
                timerHelper.ResetGlobalCameraTimer();
                timerHelper.ResetCameraActionTimer();
                if (timerOverride > 0)
                {
                    timerHelper.SetGlobalTimer(timerOverride);
                }
            }
        }
        public void SelectCamera()
        {

            player.CalculateInfo();
            if (timerHelper.controllerTimer >= CONTROLLER_THROTTLE)
            {
                // TODO: Take account movements of hands as well
                /* If user swining alot, an sample from x amount of time could tell if user is swinginig their hands in multiple directions 
                * (indicating meelee) or if they are steady
                * Or if they have a rapid back and forth motion.

                * Current Logic:

                * While Aiming Forwards:
                * If user turns their head to Right,  the direction left of the camera is true.
                * the Left Angle, the controllers should be reverse of where the user is looking at.
                * If looking completely down, user is most likely interacting with their inventory so show fps or full body
                * If Looking up they are about to do something, so
                */
                bool canSwapCamera = (timerHelper.globalTimer > pluginSettings.cameraSwapTimeLock);

                Vector3 averageHandPosition = player.handAverage;
                Vector3 handDirection;

                if (pluginSettings.rightHandDominant) 
                {
                    handDirection = (player.leftHand.position - player.rightHand.position).normalized;
                }
                else
                {
                    handDirection = (player.rightHand.position - player.leftHand.position).normalized;
                }

                Vector3 headForwardPosition = player.head.TransformPoint(Vector3.forward * 0.2f);
                Vector3 headBackPosition = player.head.TransformPoint(Vector3.forward * -0.1f);
                bool areHandsAboveThreshold = (headForwardPosition.y-0.15) < player.handAverage.y;

                bool isAimingTwoHandedForward = Mathf.Rad2Deg *
                    PluginUtility.GetConeAngle(headBackPosition, averageHandPosition + handDirection * 2f, player.head.right) <
                        pluginSettings.cameraGunHeadAlignAngleTrigger;

                // player is looking down sights.

                if ( areHandsAboveThreshold
                    && Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold
                    && Mathf.Abs(player.headRRadialDelta.y) < pluginSettings.controlVerticalMovementThreshold
                    && canSwapCamera )
                {

                    SetCamera(FPSCamera, true);
                    timerHelper.ResetCameraGunTimer();
                    PluginLog.Log("ActionCameraDirector", "In FPS ");
                }
               
                // Looking Side to Side while pointing forwards. Action is ahead.
                else if ((PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headForwardDirection) < 80) &&
                    Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.controlMovementThreshold && canSwapCamera && !(isAimingTwoHandedForward && areHandsAboveThreshold))
                {

                    PluginLog.Log("ActionCameraDirector", "Not in FPS mode");
                    SetCamera(OverShoulderCamera);
                }

                timerHelper.ResetControllerTimer();
            }
            HandleCameraView();
        }

        public void SnapCamera(FPSPluginCamera camera, bool revert = false)
        {

            camera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player, isCameraStatic);

            if (revert)
            {
                cameraLookAtVelocity = cameraLastLookAtVelocity;
                cameraVelocity = cameraLastVelocity;
            }
            else
            {
                cameraLastLookAtVelocity = cameraLookAt;
                cameraLastVelocity = cameraVelocity;
                cameraLookAtVelocity = Vector3.zero;
                cameraVelocity = Vector3.zero;
            }

            cameraPosition = cameraPositionTarget;
            cameraLookAt = cameraLookAtTarget;
        }
        public void HandleCameraView()
        {
            // Call the camera's behavior.
            currentCamera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player, isCameraStatic);
            isCameraStatic = currentCamera.staticCamera;

            if (currentAvatar != null)
            {
                if (pluginSettings.removeAvatarInsteadOfHead)
                {
                    currentAvatar.avatarSettings.showAvatar.Value = !currentCamera.removeHead;
                }
                else
                {
                    currentAvatar.avatarSettings.showAvatarHead.Value = !currentCamera.removeHead;
                }
                timerHelper.ResetRemoveAvatarTimer();
            }

            cameraPosition = Vector3.SmoothDamp(cameraPosition, cameraPositionTarget, ref cameraVelocity, currentCamera.GetBetweenTime());
            cameraLookAt = Vector3.SmoothDamp(cameraLookAt, cameraLookAtTarget, ref cameraLookAtVelocity, currentCamera.GetBetweenTime());

            Vector3 lookDirection = cameraLookAt - cameraPosition;

            Quaternion rotation = currentCamera.GetRotation(lookDirection, player);

            cameraHelper.UpdateCameraPose(cameraPosition, rotation);
            cameraHelper.UpdateFov(currentCamera.GetFOV());
        }
    }
}
