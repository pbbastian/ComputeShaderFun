using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RayTracer.Editor.Tests
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TestCaseData> AsNamedTestCase<T>(this IEnumerable<T> enumerable, string name)
        {
            return enumerable.Select(x => new TestCaseData(x).SetName(string.Format("{0} ({1})", name, x)));
        }
    }
}
