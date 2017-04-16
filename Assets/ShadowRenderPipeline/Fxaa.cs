using System;
using UnityEngine;

namespace ShadowRenderPipeline
{
    public class Fxaa
    {
        [Serializable]
        public struct QualitySettings
        {
            [Tooltip("The amount of desired sub-pixel aliasing removal. Effects the sharpeness of the output.")]
            [Range(0.0f, 1.0f)]
            public float subpixelAliasingRemovalAmount;

            [Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            [Range(0.063f, 0.333f)]
            public float edgeDetectionThreshold;

            [Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            [Range(0.0f, 0.0833f)]
            public float minimumRequiredLuminance;
        }

        [Serializable]
        public struct ConsoleSettings
        {
            [Tooltip("The amount of spread applied to the sampling coordinates while sampling for subpixel information.")]
            [Range(0.33f, 0.5f)]
            public float subpixelSpreadAmount;

            [Tooltip("This value dictates how sharp the edges in the image are kept; a higher value implies sharper edges.")]
            [Range(2.0f, 8.0f)]
            public float edgeSharpnessAmount;

            [Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            [Range(0.125f, 0.25f)]
            public float edgeDetectionThreshold;

            [Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            [Range(0.04f, 0.06f)]
            public float minimumRequiredLuminance;
        }

        [Serializable]
        public struct Preset
        {
            [AttributeUsage(AttributeTargets.Field)]
            public class LayoutAttribute : PropertyAttribute
            {}

            [Layout]
            public QualitySettings qualitySettings;

            [Layout]
            public ConsoleSettings consoleSettings;

            static readonly Preset k_ExtremePerformance = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.0f,
                    edgeDetectionThreshold = 0.333f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.33f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.25f,
                    minimumRequiredLuminance = 0.06f
                }
            };

            static readonly Preset k_Performance = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.25f,
                    edgeDetectionThreshold = 0.25f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.33f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.06f
                }
            };

            static readonly Preset k_Default = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.75f,
                    edgeDetectionThreshold = 0.166f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.05f
                }
            };

            static readonly Preset k_Quality = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 1.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.0625f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 4.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.04f
                }
            };

            static readonly Preset k_ExtremeQuality = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 1.0f,
                    edgeDetectionThreshold = 0.063f,
                    minimumRequiredLuminance = 0.0312f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 2.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.04f
                }
            };

            public static Preset extremePerformancePreset
            {
                get { return k_ExtremePerformance; }
            }

            public static Preset performancePreset
            {
                get { return k_Performance; }
            }

            public static Preset defaultPreset
            {
                get { return k_Default; }
            }

            public static Preset qualityPreset
            {
                get { return k_Quality; }
            }

            public static Preset extremeQualityPreset
            {
                get { return k_ExtremeQuality; }
            }
        }

        public static Preset[] availablePresets =
        {
            Preset.extremePerformancePreset,
            Preset.performancePreset,
            Preset.defaultPreset,
            Preset.qualityPreset,
            Preset.extremeQualityPreset
        };

        public static class Uniforms
        {
            public static readonly int qualitySettings = Shader.PropertyToID("_QualitySettings");
            public static readonly int consoleSettings = Shader.PropertyToID("_ConsoleSettings");
        }
    }
}
