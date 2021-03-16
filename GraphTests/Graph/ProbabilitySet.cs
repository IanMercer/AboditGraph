using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Abodit.Graph;

namespace Abodit.Units.Tests.Graph
{
    [TestFixture]
    public class ProbabilitySet
    {
        [Test]
        public void CanChooseOne()
        {
            var list = new List<string> { "A", "B", "C" };
            var one = list.ChooseOne().ToList();
            one.Should().HaveCount(3);

            string.Join(",", one.Select(x => x.chosen)).Should().Be("A,B,C");
            string.Join(",", one.Select(x => string.Join("|", x.notChosen))).Should().Be("B|C,A|C,A|B");
        }

        [Test]
        public void CanChooseN()
        {
            var a = new ProbabilitySet<Person>(new Person("A"));
            var b = new ProbabilitySet<Person>(new Person("B"));

            var uut = (a + b) * 0.5;
            var choose1 = uut.ChooseN(1);
            var choose2 = uut.ChooseN(2);

            choose1.Count().Should().Be(2);
            choose2.Count().Should().Be(2);
            choose1.NodeProbabilities.Values.Should().AllBeEquivalentTo(0.5);
            choose2.NodeProbabilities.Values.Should().AllBeEquivalentTo(1.0);
            choose2.DotLabel.Should().Be("A : 1.000\\nB : 1.000");
        }

        [Test]
        public void CanChooseNFrom3()
        {
            var a = new ProbabilitySet<Person>(new Person("A"));
            var b = new ProbabilitySet<Person>(new Person("B"));
            var c = new ProbabilitySet<Person>(new Person("C"));

            var uut = (a + b + c) / 3.0;
            var choose1 = uut.ChooseN(1);
            var choose2 = uut.ChooseN(2);
            var choose3 = uut.ChooseN(3);

            choose1.NodeProbabilities.Values.Should().AllBeEquivalentTo(1.0 / 3.0);
            choose2.NodeProbabilities.Values.Should().AllBeEquivalentTo(2.0 / 3.0);
            choose3.NodeProbabilities.Values.Should().AllBeEquivalentTo(1.0);
        }

        [Test]
        public void CanChooseNFrom2Irregular()
        {
            var a = new ProbabilitySet<Person>(new Person("A"));  // 0.2
            var b = new ProbabilitySet<Person>(new Person("B"));  // 0.1

            var uut = (a + a + b) / 10.0;

            uut.ChooseN(1).NodeProbabilities.Values.Should().BeEquivalentTo(new double[] { 0.2, 0.1 });
            uut.ChooseN(2).NodeProbabilities.Values.Should().BeEquivalentTo(new double[] { 0.4, 0.2 });
        }
    }
}