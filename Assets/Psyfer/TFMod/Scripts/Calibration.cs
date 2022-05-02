using UnityEngine;
using TiltFive;
using UnityEngine.Events;

namespace Psyfer 
{
    public class Calibration : MonoBehaviour
    {
        [SerializeField]
        private bool calibrated = false;
        [SerializeField]
        private float _thumbstickSensitivity = 0.001f;
        [SerializeField]
        private TMPro.TMP_Text _calibrationValueTextDisplay;
        [SerializeField]
        private UnityEvent _onCalibrationComplete;

        private SplitStereoCamera _splitStereoCamera;

        private void Update()
        {
            // Wait until the game has started to find this, as tiltfive spawns it late.
            if (_splitStereoCamera == null)
            {
                _splitStereoCamera = FindObjectOfType<SplitStereoCamera>();
                _splitStereoCamera.depthPerceptionCorrection = PlayerPrefs.GetFloat("DepthPerceptionCorrection", 0f);
            }

            if(calibrated) return;

            // Collect stick input and update the depth perception value
            TiltFive.Input.TryGetStickTilt(out Vector2 stickTilt);
            if(Mathf.Abs(stickTilt.y) > 0.1f) 
            {
                AdjustPerceivedDepth(stickTilt.y);
            }

            // Display the shift value
            if(_calibrationValueTextDisplay != null)
            {
                _calibrationValueTextDisplay.text = $"{_splitStereoCamera.depthPerceptionCorrection:0.00000}";
            }

            
            // We're finished calibrating
            if(TiltFive.Input.GetButtonDown(TiltFive.Input.WandButton.One)) 
            {
                CalibrationComplete();
            }
        }

        /// <summary> Adjust the perceived depth for the SplitStereoCamera, uses _thumbstickSensitivity </summary>
        private void AdjustPerceivedDepth(float adjustment)
        {
            _splitStereoCamera.depthPerceptionCorrection += adjustment * _thumbstickSensitivity * Time.deltaTime;
            _splitStereoCamera.depthPerceptionCorrection = Mathf.Clamp(_splitStereoCamera.depthPerceptionCorrection, -0.01f, 0.0025f);
        }

        /// <summary> Called when the calibration is complete, invokes the associated UnityEvent </summary>
        private void CalibrationComplete() 
        {
            SaveCalibration();
            calibrated = true;
            _onCalibrationComplete.Invoke();
        }

        /// <summary> Save the Calibration value for the next time we run. </summary>
        private void SaveCalibration()
        {
            // Save the calibration value
            PlayerPrefs.SetFloat("DepthPerceptionCorrection", _splitStereoCamera.depthPerceptionCorrection);
        }
    }
}