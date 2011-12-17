using System;
using System.Reflection;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = Assembly.Load("ImplementationLibrary").GetType("Foo");
            var instance = (IFoo) Activator.CreateInstance(type);
            Console.WriteLine(instance.Bar());
        }
    }
}
