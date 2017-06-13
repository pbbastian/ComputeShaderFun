using System;
using UnityEngine;

namespace BenchmarkPipeline.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class BenchmarkState : MonoBehaviour
    {
        public bool benchmarkEnabled { get; set; }
    }
}
