using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter
{
    [ExecuteInEditMode]
    public class JDayNightCycle : MonoBehaviour
    {
        [SerializeField]
        private JDayNightCycleProfile profile;
        public JDayNightCycleProfile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value;
            }
        }

        [SerializeField]
        private JSky sky;
        public JSky Sky
        {
            get
            {
                return sky;
            }
            set
            {
                sky = value;
            }
        }

        [SerializeField]
        private Transform orbitPivot;
        public Transform OrbitPivot
        {
            get
            {
                return orbitPivot;
            }
            set
            {
                orbitPivot = value;
            }
        }

        [SerializeField]
        private float startTime;
        public float StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = Mathf.Clamp(value, 0f, 24f);
            }
        }

        [SerializeField]
        private float timeIncrement;
        public float TimeIncrement
        {
            get
            {
                return timeIncrement;
            }
            set
            {
                timeIncrement = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool autoTimeIncrement;
        public bool AutoTimeIncrement
        {
            get
            {
                return autoTimeIncrement;
            }
            set
            {
                autoTimeIncrement = value;
            }
        }

        private float time;
        public float Time
        {
            get
            {
                return time % 24f;
            }
            set
            {
                time = value % 24f;
            }
        }

        private void Reset()
        {
            Sky = GetComponent<JSky>();
            StartTime = 0;
            TimeIncrement = 1;
            AutoTimeIncrement = true;
            Time = 0;
        }

        private void OnEnable()
        {
            time = StartTime;
            Camera.onPreCull += OnCameraPreCull;
            RenderPipelineManager.beginFrameRendering += OnBeginFrameRenderingSRP;
        }
        
        private void OnDisable()
        {
            Camera.onPreCull -= OnCameraPreCull;
            RenderPipelineManager.beginFrameRendering -= OnBeginFrameRenderingSRP;
        }

        private void OnCameraPreCull(Camera cam)
        {
            AnimateSky();
        }

        private void OnBeginFrameRenderingSRP(ScriptableRenderContext context, Camera[] cameras)
        {
            AnimateSky();
        }

        private void AnimateSky()
        {
            if (Profile == null)
                return;
            if (Sky == null)
                return;
            if (Sky.Profile == null)
                return;
            if (AutoTimeIncrement)
            {
                Time += TimeIncrement * JUtilities.DELTA_TIME;
            }
            float evalTime = Mathf.InverseLerp(0f, 24f, Time);
            Profile.Animate(Sky, evalTime);

            if (Sky.Profile.EnableSun && Sky.SunLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.up);

                Matrix4x4 localToWorld = OrbitPivot ?
                    OrbitPivot.localToWorldMatrix :
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);
                Sky.SunLightSource.transform.forward = worldDirection;
            }

            if (Sky.Profile.EnableMoon && Sky.MoonLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.down);

                Matrix4x4 localToWorld = OrbitPivot ?
                    OrbitPivot.localToWorldMatrix :
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);
                Sky.MoonLightSource.transform.forward = worldDirection;
            }
        }
    }
}
