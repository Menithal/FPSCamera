
using UnityEngine;
namespace FPSCamera
{
    public abstract class FPSPluginCamera
    {
        // Do something with this in the future
        protected float fov = 90;
        private float timeBetweenChange = 1f;
        public bool removeHead = false;
        public bool staticCamera = false;

        protected static readonly sbyte POSITIVE_SBYTE = 1;
        protected static readonly sbyte NEGATIVE_SBYTE = -1;

        protected FPSCameraSettings pluginSettings;

        protected sbyte currentSide;
        protected sbyte destinationSide;
        public Vector3 offset;
        public FPSPluginCamera(FPSCameraSettings pluginSettings,
            float timeBetweenChange, Vector3 offset = new Vector3(),
            bool removeHead = false, bool staticCamera = false)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.removeHead = removeHead;
            this.staticCamera = staticCamera;
            this.currentSide = 1;
            this.destinationSide = 1;
            this.offset = offset;
        }
        public FPSPluginCamera(FPSCameraSettings pluginSettings,
           float timeBetweenChange)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.currentSide = 1;
        }
        public virtual float GetFOV()
        {
            return fov;
        }
        public void SetBetweenTime(float time)
        {
            timeBetweenChange = time;
        }
        public virtual float GetBetweenTime()
        {
            return timeBetweenChange;
        }
        public virtual void SetPluginSettings(FPSCameraSettings settings)
        {
            pluginSettings = settings;
            fov = settings.cameraDefaultFov;
        }

        abstract public void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget,
            LivPlayerEntity player, bool isCameraAlreadyPlaced);

        public virtual Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.LookRotation(lookDirection);
        }
    }

    public class SimpleActionCamera : FPSPluginCamera
    {
        public Vector3 lookAtOffset = new Vector3(0f, 0, 0.25f);
        public SimpleActionCamera(FPSCameraSettings settings, float timeBetweenChange, Vector3 offset, bool removeHead = false, bool staticCamera = false) :
            base(settings, timeBetweenChange, offset, removeHead, staticCamera)
        {
        }

        // Next to FPS Camera, simplest Camerda
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            cameraTarget = player.head.TransformPoint(offset);
            lookAtTarget = player.head.TransformPoint(lookAtOffset);
            // average between Head and Waist to avoid head from flipping the camera around so much.s
            lookAtTarget = (lookAtTarget + player.waist.TransformPoint(lookAtOffset)) / 2;

            if (pluginSettings.cameraVerticalLock)
            {
                lookAtTarget.y = (player.waist.position.y + player.head.position.y) / 2;
            }

            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);
        }
    }

    public class ADSCamera : FPSPluginCamera
    {
        public Vector3 lookAtOffset = new Vector3(0f, 0f, 0f);

        Transform dominantHand;
        Transform nonDominantHand;
        Vector3 dominantEye;
        Vector3 lookAtDirection;
        public ADSCamera(FPSCameraSettings settings) :
            base(settings, 0.2f, Vector3.zero, true)
        {
            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(FPSCameraSettings settings)
        {

            fov = settings.cameraGunFov;
            pluginSettings = settings;
            offset = new Vector3(0, -settings.cameraGunEyeVerticalOffset, 0);
            lookAtOffset.y = offset.y;
            SetBetweenTime(settings.cameraGunSmoothing);
        }

        public override Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.LookRotation(lookAtDirection, player.head.up);
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            // Automatic determination which is closest?
            if (pluginSettings.rightHandDominant)
            {
                dominantHand = player.rightHand;
                nonDominantHand = player.leftHand;
            }
            else
            {
                dominantHand = player.leftHand;
                nonDominantHand = player.rightHand;
            }

            if (pluginSettings.rightEyeDominant)
            {
                dominantEye = player.rightEye;
            }
            else
            {
                dominantEye = player.leftEye;
            }

            float handDistance = Vector3.Distance(player.rightHand.position, player.leftHand.position);
            Vector3 handDirection = (nonDominantHand.position - dominantHand.position).normalized;

            if(handDistance < pluginSettings.cameraGunMinTwoHandedDistance * 1.2)
            {
                handDirection.y = handDirection.y * 0.5f;
                handDirection = handDirection.normalized;
            }

            //lookAtDirection = (nonDominantHand.position - new Vector3(0, pluginSettings.cameraGunEyeVerticalOffset, 0) - dominantHand.position);
            lookAtDirection = handDirection * 4f;
            // We will override the lookAtTarget and use GetRotation to define the actual rotation.
            lookAtTarget = Vector3.zero;
            cameraTarget = dominantEye;
        }
    }

    public class ShoulderActionCamera : FPSPluginCamera
    {
        // This one really needs an intermediary camera
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0, 5f);
        private readonly SimpleActionCamera betweenCamera;
        private bool swappingSides;
        // Predefining this to get around having to convert them.
        public ShoulderActionCamera(FPSCameraSettings settings) :
            base(settings, 0)
        {
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y = -settings.cameraBodyVerticalTargetOffset;
            neutralOffset.z = -settings.cameraShoulderDistance;
            betweenCamera = new SimpleActionCamera(settings, settings.cameraShoulderPositioningTime / 2, neutralOffset);

            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(FPSCameraSettings settings)
        {
            base.SetPluginSettings(settings);

            SetBetweenTime(settings.cameraShoulderPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetBetweenTime(settings.cameraShoulderPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetPluginSettings(settings);
            betweenCamera.offset = new Vector3(0, -settings.cameraBodyVerticalTargetOffset, -settings.cameraShoulderDistance);
            CalculateOffset();

        }
        public void CalculateOffset()
        {

            float radianAngle = Mathf.Deg2Rad * pluginSettings.cameraShoulderAngle;

            float y = pluginSettings.cameraShoulderDistance * Mathf.Cos(radianAngle);
            float x = pluginSettings.cameraShoulderDistance * Mathf.Sin(radianAngle);

            Vector3 calculatedOffset = new Vector3(x, 0.5f, -y);

            // PluginLog.Log("ShoulderActionCamera", "Calculated Offset " + calculatedOffset + " vs " + offset);
            // Gotta be from back not from front. 
            //  calculatedOffset.z = -Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2) - Mathf.Pow(offset.x, 2)));
            this.offset = calculatedOffset;
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {

            /**
             * Logic for betweenCamera
             * When Camera is triggering swap sides (when player moves head fast enough, and the direction does not match previous)
             *  use only the betweenCamera behavior.
             *  once timeBetween Change has been fully realized, then start applying current behavior as planned.
            */

            sbyte estimatedSide = (player.headRRadialDelta.x < 0 ? NEGATIVE_SBYTE : POSITIVE_SBYTE);
            if (!swappingSides && Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.cameraShoulderSensitivity &&
                player.timerHelper.cameraActionTimer > GetBetweenTime() &&
                estimatedSide != currentSide)
            {
                PluginLog.Log("ShoulderCamera", "Swapping sides " + estimatedSide);
                swappingSides = true;
                destinationSide = estimatedSide;
                player.timerHelper.ResetCameraActionTimer();
            }
            else if (swappingSides && player.timerHelper.cameraActionTimer > GetBetweenTime())
            {
                swappingSides = false;
                currentSide = destinationSide;
                PluginLog.Log("ShoulderCamera", "Done Swapping");
                player.timerHelper.ResetCameraActionTimer();
            }

            if (swappingSides && pluginSettings.inBetweenCameraEnabled)
            {
                betweenCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
            }
            else
            {
                Vector3 cameraOffsetTarget = offset;
                sbyte settingsReverse = pluginSettings.reverseShoulder ? NEGATIVE_SBYTE : POSITIVE_SBYTE;

                cameraOffsetTarget.x = -currentSide * Mathf.Abs(cameraOffsetTarget.x) * settingsReverse;
                cameraTarget = player.head.TransformPoint(cameraOffsetTarget);

                // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
                lookAtTarget = player.head.TransformPoint(lookAtOffset);

                if (pluginSettings.cameraVerticalLock)
                {
                    lookAtTarget.y = (player.waist.position.y + player.head.position.y) / 2;
                    cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y + 1f);
                } else
                {
                    lookAtTarget.y = Mathf.Clamp(lookAtTarget.y, -0.5f, player.head.position.y + 1f);
                    cameraTarget.y = Mathf.Clamp(cameraTarget.y, -0.5f, player.head.position.y  + 1f);
                }
            }
        }
    }

    public class FPSCamera : SimpleActionCamera
    {
        readonly FPSPluginCamera sightsCamera;
        bool ironSightsEnabled;
        float blend = 0;
        public FPSCamera(FPSCameraSettings settings, float timeBetweenChange) : base(settings, timeBetweenChange, Vector3.zero, true, false)
        {
            sightsCamera = new ADSCamera(settings);
            ironSightsEnabled = false;
        }

        public override float GetFOV()
        {
            if (pluginSettings.cameraFovLerp) return Mathf.Lerp(base.GetFOV(), sightsCamera.GetFOV(), blend);

            return ironSightsEnabled ? sightsCamera.GetFOV() : base.GetFOV();
        }

        public override float GetBetweenTime()
        {
            return Mathf.Lerp(base.GetBetweenTime(), sightsCamera.GetBetweenTime(), blend);
        }
        public FPSPluginCamera GetScope()
        {
            return sightsCamera;
        }
        public override void SetPluginSettings(FPSCameraSettings settings)
        {
            base.SetPluginSettings(settings);
            sightsCamera.SetPluginSettings(settings);
        }
        /*
         * 
         * Logic for Scope should only occur when 
         */
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
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

            // If Hands close enough, and aligned, then do the thing, if not then
            float handDistance = Vector3.Distance(player.rightHand.position, player.leftHand.position);
            bool isWithinTwoHandedUse = handDistance > pluginSettings.cameraGunMinTwoHandedDistance && handDistance < pluginSettings.cameraGunMaxTwoHandedDistance;
            bool isHeadWithinAimingDistance = Vector3.Distance(averageHandPosition, player.head.position) < pluginSettings.cameraGunHeadDistanceTrigger;
            bool isAimingTwoHandedForward = Mathf.Rad2Deg * PluginUtility.GetConeAngle(player.headBackwardDirection, averageHandPosition + handDirection*1.2f, player.head.right) <
                    pluginSettings.cameraGunHeadAlignAngleTrigger*0.9;


            bool snapToGun = Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold;

            if (snapToGun &&
                   isWithinTwoHandedUse && isHeadWithinAimingDistance && isAimingTwoHandedForward)
            {
                ironSightsEnabled = true;
                // Should have a smooth transition between Iron Sights and non iron sights.
                sightsCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
                blend += pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }
            else
            {
                ironSightsEnabled = false;
                if (pluginSettings.useEyePosition)
                {
                    if (pluginSettings.rightEyeDominant)
                    {
                        cameraTarget = player.rightEye;
                    }
                    else
                    {
                        cameraTarget = player.leftEye;
                    }
                }
                else { 
                    cameraTarget = player.head.TransformPoint(offset);
                }
                lookAtTarget = player.head.TransformPoint(lookAtOffset);

                blend -= 1 / pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }

            blend = Mathf.Clamp(blend, 0, 1.0f);
        }

        public override Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.Slerp(Quaternion.LookRotation(player.head.forward, player.head.up), sightsCamera.GetRotation(lookDirection, player), blend);
        }
    }

}
