using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkPipeline.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace BenchmarkPipeline.Editor
{
    public class ProfilerStatisticsWindow : EditorWindow
    {
        [MenuItem("Window/Profiler Statistics")]
        static void CreateWindow()
        {
            var window = GetWindow<ProfilerStatisticsWindow>(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent("Statistics");
            window.Show();
        }

        [SerializeField]
        bool m_Enabled;

        [SerializeField]
        GameObject m_CameraObject;

        BenchmarkState m_BenchmarkState;
        int m_FrameIndex;
        int m_TimingIndex;
        float[] m_Timings;
        float m_Mean;
        float m_StandardDeviation;
        float m_FilteredMean;
        float m_FilteredCount;
        bool m_StatisticsCalculated;

        string m_SerializedTimings;

        string m_ExportDirectory;
        string m_ExportFileName;

        void OnEnable()
        {
            if (SceneManager.sceneCount > 0)
                Initialize(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            SceneManager.sceneLoaded += Initialize;
        }

        void Initialize(Scene scene, LoadSceneMode mode)
        {
            m_BenchmarkState = m_CameraObject != null ? m_CameraObject.GetComponent<BenchmarkState>() : null;
            m_FrameIndex = ProfilerDriver.lastFrameIndex;
            m_TimingIndex = 0;
            m_Timings = new float[1000];
            m_Mean = default(float);
            m_StandardDeviation = default(float);
            m_FilteredMean = default(float);
            m_FilteredCount = default(float);
            m_StatisticsCalculated = false;
            m_SerializedTimings = null;
        }

        void Update()
        {
            //if (m_BenchmarkState == null)
            //    return;

            if (m_Enabled && Application.isPlaying && ProfilerDriver.profileGPU && m_TimingIndex < m_Timings.Length)
            {
                //m_BenchmarkState.benchmarkEnabled = true;
                var lastFrameIndex = ProfilerDriver.lastFrameIndex;
                while (m_FrameIndex < lastFrameIndex)
                {
                    var property = new ProfilerProperty();
                    property.SetRoot(m_FrameIndex, ProfilerColumn.SelfGPUTime, ProfilerViewType.Hierarchy);
                    property.onlyShowGPUSamples = true;
                    while (property.Next(true))
                    {
                        var functionName = property.GetColumn(ProfilerColumn.FunctionName);
                        if (functionName == "Global Scan" && m_TimingIndex < m_Timings.Length)
                        {
                            m_Timings[m_TimingIndex] = property.GetColumnAsSingle(ProfilerColumn.TotalGPUTime);
                            m_TimingIndex++;
                            Repaint();
                        }
                    }
                    m_FrameIndex++;
                }
            }
            else
            {
                //m_BenchmarkState.benchmarkEnabled = false;
            }

            // Check whether we're done
            if (m_TimingIndex == m_Timings.Length && !m_StatisticsCalculated)
            {
                // Calculate statistics
                m_Mean = m_Timings.Average();
                m_StandardDeviation = Mathf.Sqrt(m_Timings.Select(x => Mathf.Pow(x - m_Mean, 2)).Average());
                var filteredTimings = m_Timings.Where(x => Mathf.Abs(x - m_Mean) < m_StandardDeviation).ToList();
                m_FilteredMean = filteredTimings.Average();
                m_FilteredCount = filteredTimings.Count();
                m_StatisticsCalculated = true;

                // Serialize timings for export
                {
                    var sb = new StringBuilder();
                    foreach (var timing in m_Timings)
                        sb.AppendLine(timing.ToString(new CultureInfo("en-US")));
                    m_SerializedTimings = sb.ToString();
                }

                // Serialize filtered timings for export
                {
                    var sb = new StringBuilder();
                    foreach (var timing in filteredTimings)
                        sb.AppendLine(timing.ToString(new CultureInfo("en-US")));
                }

                // Repaint the window
                Repaint();
            }
        }

        void OnGUI()
        {
            m_Enabled = EditorGUILayout.Toggle("Enabled", m_Enabled);

            if (m_Enabled && Application.isPlaying && !ProfilerDriver.profileGPU)
                EditorGUILayout.HelpBox("Please make sure that the Profiler window is open and that the GPU profiler is added.", MessageType.Error);

            m_CameraObject = EditorGUILayout.ObjectField("Camera", m_CameraObject, typeof(GameObject), true) as GameObject;

            if (m_CameraObject != null && m_CameraObject.GetComponent<BenchmarkState>() == null)
                EditorGUILayout.HelpBox("Camera must have a BenchmarkState component attached.", MessageType.Error);

            EditorGUILayout.Space();

            if (m_Timings != null)
                EditorGUILayout.LabelField("Timings", $"{m_TimingIndex}/{m_Timings.Length}");

            EditorGUILayout.LabelField("Mean", m_StatisticsCalculated ? $"{m_Mean} ms" : "N/A");
            EditorGUILayout.LabelField("SD", m_StatisticsCalculated ? $"{m_StandardDeviation} ms" : "N/A");
            EditorGUILayout.LabelField("Filtered Mean", m_StatisticsCalculated ? $"{m_FilteredMean} ms" : "N/A");
            EditorGUILayout.LabelField("Filtered Samples", m_StatisticsCalculated ? $"{m_FilteredCount} samples remaining" : "N/A");

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear"))
            {
                Initialize(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }

            EditorGUILayout.Space();

            GUILayout.Label("Export", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Directory");
                EditorGUILayout.SelectableLabel(m_ExportDirectory, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
                {
                    var selectedDirectory = EditorUtility.OpenFolderPanel("Select Export Directory", m_ExportDirectory, "");
                    if (!string.IsNullOrWhiteSpace(selectedDirectory))
                        m_ExportDirectory = selectedDirectory;
                }
            }
            m_ExportFileName = EditorGUILayout.TextField("File name", m_ExportFileName);
            GUI.enabled = Directory.Exists(m_ExportDirectory) && !string.IsNullOrEmpty(m_ExportFileName);
            if (GUILayout.Button("Export"))
                File.WriteAllText(Path.Combine(m_ExportDirectory, m_ExportFileName), m_SerializedTimings, new UTF8Encoding(false));
            GUI.enabled = true;
        }
    }
}
