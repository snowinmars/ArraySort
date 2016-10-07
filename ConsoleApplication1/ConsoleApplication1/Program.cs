using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main()
        {
            const int amountOfDimension = 4;

            int[] dimensionLength = { 5, 3, 4, 6 };

            ArraySorter.CreateAndShowAndSortAndShowAgain(amountOfDimension, dimensionLength);
        }
    }
}