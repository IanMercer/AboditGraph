using System;

namespace Abodit.Units.Tests.Graph
{
    public class Person : IEquatable<Person>
    {
        public string Name { get; }

        private static int sequence = 0;

        public Person()
        {
            this.Name = $"Person #{sequence++}";
        }

        public Person(string name)
        {
            this.Name = name;
        }

        public bool Equals(Person other)
        {
            return this.Name.Equals(other.Name);
        }

        public override string ToString() => this.Name;

        public static readonly Person[] Instances = new[] {
                new Person("Liam"),
                new Person("Noah"),
                new Person("Elijah"),
                new Person("Logan"),
                new Person("Mason"),
                new Person("James"),
                new Person("Aiden"),
                new Person("Ethan"),
                new Person("Lucas"),
                new Person("Jacob"),
                new Person("Michael"),
                new Person("Matthew"),
                new Person("Benjamin"),
                new Person("Alexander"),
                new Person("William"),
                new Person("Daniel"),
                new Person("Oliver"),
                new Person("Sebastian"),
                new Person("Jospeh"),
                new Person("David"),
                new Person("Gabriel"),
                new Person("Julian"),
                new Person("Jackson"),
                new Person("Anthony"),
                new Person("Christopher"),
                new Person("Christian"),
                new Person("Andrew"),
                new Person("Samuel"),
                new Person("John"),
                new Person("Luke"),
                new Person("Ryan"),
                new Person("Joshua")
        };
    }
}