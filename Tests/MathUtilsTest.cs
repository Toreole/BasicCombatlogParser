using CombatlogParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

class MathUtilsTest
{
    [Test]
    public void TestMedian()
    {
        float[] values = [1f, 2f, 3f];
        Assert.That(MathUtil.Median(values), Is.EqualTo(2f));

        float[] values2 = [1f, 1f, 2f, 2f];
        Assert.That(MathUtil.Median(values2), Is.EqualTo(1.5f));
    }
}
