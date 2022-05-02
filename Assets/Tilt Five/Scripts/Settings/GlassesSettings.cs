using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltFive
{
    public enum GlassesMirrorMode
    {
        None,
        LeftEye,
        RightEye,
        Stereoscopic
    }

    /// <summary>
    /// GlassesSettings encapsulates all configuration data used by the Glasses'
    /// tracking runtime to compute the Head Pose and apply it to the Camera.
    /// </summary>
    [System.Serializable]
    public class GlassesSettings : TrackableSettings
    {
    #if UNITY_EDITOR
        /// <summary>
        /// Editor only configuration to disable/enable stereo-rendering.
        /// </summary>
        public bool tiltFiveXR = true;
    #endif
        /// <summary>
        /// The main camera used for rendering the Scene when the glasses are unavailable, and the gameobject used for the glasses pose.
        /// </summary>
        public Camera headPoseCamera;

        // TODO CHANGE BACK
        public const float MIN_FOV = 35f;
        public const float MAX_FOV = 64f;
        public const float DEFAULT_FOV = 48f;

        public const float DEFAULT_IPD_UGBD = 0.072f;

        public bool overrideFOV = false;
        public float customFOV = DEFAULT_FOV;
        public float fieldOfView => overrideFOV
            ? Mathf.Clamp(customFOV, MIN_FOV, MAX_FOV)
            : DEFAULT_FOV;

        public GlassesMirrorMode glassesMirrorMode = GlassesMirrorMode.LeftEye;

        public bool usePreviewPose = true;
        public Transform previewPose;
    }
}
