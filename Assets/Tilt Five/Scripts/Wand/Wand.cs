/*
 * Copyright (C) 2020 Tilt Five, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using TiltFive.Logging;

namespace TiltFive
{
    /// <summary>
    /// Wand Settings encapsulates all configuration data used by the Wand's
    /// tracking runtime to compute the Wand Pose and apply it to the driven GameObject.
    /// </summary>
    [System.Serializable]
    public class WandSettings : TrackableSettings
    {
        public ControllerIndex controllerIndex;

        public GameObject GripPoint;
        public GameObject FingertipPoint;
        public GameObject AimPoint;

        // TODO: Think about some accessors for physical attributes of the wand (length, distance to tip, etc)?
    }

    /// <summary>
    /// The Wand API and runtime.
    /// </summary>
    public class Wand : Singleton<Wand>
    {
        #region Private Fields

        private Dictionary<ControllerIndex, WandCore> wandCores = new Dictionary<ControllerIndex, WandCore>()
        {
            { ControllerIndex.Primary, new WandCore() },
            { ControllerIndex.Secondary, new WandCore() }
        };

        #endregion


        #region Public Functions

        // Update is called once per frame
        public static void Update(WandSettings wandSettings, ScaleSettings scaleSettings, GameBoardSettings gameBoardSettings)
        {
            Instance.wandCores[wandSettings.controllerIndex].Update(wandSettings, scaleSettings, gameBoardSettings);
        }

        /// <summary>
        /// Gets the position of the wand in world space.
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <param name="controllerPosition"></param>
        /// <returns></returns>
        public static Vector3 GetPosition(
            ControllerIndex controllerIndex = ControllerIndex.Primary,
            ControllerPosition controllerPosition = ControllerPosition.Grip)
        {
            switch (controllerPosition)
            {
                case ControllerPosition.Fingertips:
                    return Instance.wandCores[controllerIndex].fingertipsPose_UnityWorldSpace.position;
                case ControllerPosition.Aim:
                    return Instance.wandCores[controllerIndex].aimPose_UnityWorldSpace.position;
                case ControllerPosition.Grip:
                    return Instance.wandCores[controllerIndex].Pose_UnityWorldSpace.position;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets the rotation of the wand in world space.
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <returns></returns>
        public static Quaternion GetRotation(ControllerIndex controllerIndex = ControllerIndex.Primary)
        {
            return Instance.wandCores[controllerIndex].Pose_UnityWorldSpace.rotation;
        }

        #endregion


        #region Private Classes

        /// <summary>
        /// Internal Wand core runtime.
        /// </summary>
        private class WandCore : TrackableCore<WandSettings>
        {
            /// <summary>
            /// The default position of the wand relative to the board.
            /// </summary>
            /// <remarks>
            /// The wand GameObject will snap back to this position if the glasses and/or wand are unavailable.
            /// </remarks>
            private static readonly Vector3 DEFAULT_WAND_POSITION_GAME_BOARD_SPACE = new Vector3(0f, 0.25f, -0.25f);
            /// <summary>
            /// A left/right offset to the default wand position, depending on handedness.
            /// </summary>
            private static readonly Vector3 DEFAULT_WAND_HANDEDNESS_OFFSET_GAME_BOARD_SPACE = new Vector3(0.125f, 0f, 0f);

            /// <summary>
            /// The default rotation of the wand relative to the board.
            /// </summary>
            /// <remarks>
            /// The wand GameObject will snap back to this rotation if the glasses are unavailable.
            /// If different behavior is desired in this scenario, a different camera should be used.
            /// </remarks>
            private static readonly Quaternion DEFAULT_WAND_ROTATION_GAME_BOARD_SPACE = Quaternion.Euler(new Vector3(-33f, 0f, 0f));

            private Pose fingertipsPose_GameboardSpace = new Pose(DEFAULT_WAND_POSITION_GAME_BOARD_SPACE, Quaternion.identity);
            private Pose aimPose_GameboardSpace = new Pose(DEFAULT_WAND_POSITION_GAME_BOARD_SPACE, Quaternion.identity);

            public Pose gripPose_UnityWorldSpace => pose_UnityWorldSpace;
            public Pose fingertipsPose_UnityWorldSpace;
            public Pose aimPose_UnityWorldSpace;

            public new void Update(WandSettings wandSettings, ScaleSettings scaleSettings, GameBoardSettings gameBoardSettings)
            {
                if (wandSettings == null)
                {
                    Log.Error("WandSettings configuration required for Wand tracking updates.");
                    return;
                }

                base.Update(wandSettings, scaleSettings, gameBoardSettings);
            }

            protected override void SetDefaultPoseGameboardSpace(WandSettings settings)
            {
                Vector3 defaultPosition = DEFAULT_WAND_POSITION_GAME_BOARD_SPACE;

                defaultPosition += DEFAULT_WAND_HANDEDNESS_OFFSET_GAME_BOARD_SPACE
                    * (settings.controllerIndex == ControllerIndex.Primary ? 1f : -1f);

                pose_GameboardSpace = new Pose(defaultPosition, DEFAULT_WAND_ROTATION_GAME_BOARD_SPACE);
                // We don't have a good offset that we can use for default fingertips/aim poses, so just use the default pose for everything
                fingertipsPose_GameboardSpace = pose_GameboardSpace;
                aimPose_GameboardSpace = pose_GameboardSpace;
            }

            protected override void SetPoseUnityWorldSpace(ScaleSettings scaleSettings, GameBoardSettings gameBoardSettings)
            {
                pose_UnityWorldSpace = GameboardToWorldSpace(pose_GameboardSpace, scaleSettings, gameBoardSettings);
                fingertipsPose_UnityWorldSpace = GameboardToWorldSpace(fingertipsPose_GameboardSpace, scaleSettings, gameBoardSettings);
                aimPose_UnityWorldSpace = GameboardToWorldSpace(aimPose_GameboardSpace, scaleSettings, gameBoardSettings);
            }

            protected override bool GetTrackingAvailability(WandSettings settings)
            {
                return Display.GetGlassesAvailability()
                    && GameBoard.TryGetGameboardType(out var gameboardType)
                    && gameboardType != GameboardType.GameboardType_None
                    && Input.GetWandAvailability(settings.controllerIndex);
            }

            protected override bool TryGetPoseFromPlugin(out Pose gripPose_GameboardSpace, WandSettings settings)
            {
                // Unity reference frames:
                //
                // UWND        - Unity WaND local space.
                //               +x right, +y up, +z forward
                // UGBD        - Unity Gameboard space.
                //               +x right, +y up, +z forward
                //
                // Tilt Five reference frames:
                //
                // DW          - Our right-handed version of Unity's default wand space.
                //               +x right, +y down, +z forward
                // GBD         - Gameboard space.
                //               +x right, +y forward, +z up

                Quaternion rotToDW_GBD = Quaternion.AngleAxis(90f, Vector3.right);

                T5_ControllerState controllerState = new T5_ControllerState();

                int result = 1;

                try
                {
                    result = NativePlugin.GetControllerState(settings.controllerIndex, ref controllerState);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                Quaternion rotToWND_GBD = controllerState.RotToWND_GBD;
                Quaternion rotToWND_DW = rotToWND_GBD * Quaternion.Inverse(rotToDW_GBD);
                Quaternion rotToUGBD_UWND = new Quaternion(-rotToWND_DW.x, rotToWND_DW.y, -rotToWND_DW.z, rotToWND_DW.w);

                Vector3 gripPos_UGBD = ConvertPosGBDToUGBD(controllerState.GripPos_GBD);
                Vector3 fingertipsPos_UGBD = ConvertPosGBDToUGBD(controllerState.FingertipsPos_GBD);
                Vector3 aimPos_UGBD = ConvertPosGBDToUGBD(controllerState.AimPos_GBD);

                // These three poses share the same rotation.
                gripPose_GameboardSpace = new Pose(gripPos_UGBD, rotToUGBD_UWND);
                fingertipsPose_GameboardSpace = new Pose(fingertipsPos_UGBD, rotToUGBD_UWND);
                aimPose_GameboardSpace = new Pose(aimPos_UGBD, rotToUGBD_UWND);

                return result == 0;
            }

            private Vector3 ConvertPosGBDToUGBD(Vector3 pos_GBD)
            {
                // Swap Y and Z to change between GBD and UGBD
                return new Vector3(pos_GBD.x, pos_GBD.z, pos_GBD.y);
            }

            protected override void SetDrivenObjectTransform(WandSettings settings)
            {
                if(GameBoard.TryGetGameboardType(out var gameboardType) && gameboardType == GameboardType.GameboardType_None)
                {
                    // TODO: Implement default poses for wands when the glasses lose tracking.
                    return;
                }

                if(settings.GripPoint != null)
                {
                    settings.GripPoint.transform.position = gripPose_UnityWorldSpace.position;
                    settings.GripPoint.transform.rotation = gripPose_UnityWorldSpace.rotation;
                }

                if (settings.FingertipPoint != null)
                {
                    settings.FingertipPoint.transform.position = fingertipsPose_UnityWorldSpace.position;
                    settings.FingertipPoint.transform.rotation = fingertipsPose_UnityWorldSpace.rotation;
                }

                if (settings.AimPoint != null)
                {
                    settings.AimPoint.transform.position = aimPose_UnityWorldSpace.position;
                    settings.AimPoint.transform.rotation = aimPose_UnityWorldSpace.rotation;
                }
            }
        }

        #endregion
    }

}