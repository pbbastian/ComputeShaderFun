using System.Collections;
using System.Collections.Generic;
using RayTracer.Runtime.Util;

namespace Assets.RayTracer.Runtime.Util
{
    public class DebugStringBuilder : IEnumerable<string>
    {
        private List<string> m_Items = new List<string>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        public void Add(string name, object value, string valueFormat = "{1}")
        {
            m_Items.Add(value.FormatDebug(name, valueFormat));
        }

        public override string ToString()
        {
            return StringDebug.Join(m_Items.ToArray());
        }
    }
}
