using System;
using UnityEditor;

namespace ShadowRenderPipeline.Editor.Util
{
    public class IndentScope : IDisposable
    {
        public IndentScope()
        {
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
        }
    }
}
