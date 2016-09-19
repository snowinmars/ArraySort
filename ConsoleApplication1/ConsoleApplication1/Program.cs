﻿using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace ConsoleApplication1
{
    internal class Program
    {
        #region Private Methods

        private static CompilerResults Compile(string[] code)
        {
            CompilerParameters compilerParams = Program.SetUpCompiler();
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compiled = provider.CompileAssemblyFromSource(compilerParams, code);
            return compiled;
        }

        private static void CompileAndRun(string[] code)
        {
            CompilerResults compile = Program.Compile(code);

            Program.HandleError(compile);

            Program.Run(compile);
        }

        private static string CreateDeclareCode(int amountOfDimension, int[] dimensionLength)
        {
            // output is like
            // int[,,] array = new int[2,3,4];
            // Random random = new Random();
            // int amountNumberOfElements = 0;

            StringBuilder firstCommas = Program.GetFirstCommas(amountOfDimension);

            StringBuilder secondCommas = Program.GetSecondCommas(amountOfDimension, dimensionLength);

            string random = "Random random = new Random();";
            string array = $"int[{firstCommas}] array = new int[{secondCommas}];";
            string amountNumberOfElements = $"int amountNumberOfElements = 0;";

            return string.Join(Environment.NewLine, array,
                                                    random,
                                                    amountNumberOfElements);
        }

        private static string GetCode(int amountOfDimension, int[] dimensionLength)
        {
            string startFrame = Program.GetStartFrame();

            string declare = Program.CreateDeclareCode(amountOfDimension, dimensionLength);

            string forCycle = Program.GetForInitCycle(amountOfDimension);

            string sort = Program.GetSortCommands(amountOfDimension);

            string show = Program.GetShowCommands(amountOfDimension);

            string endFrame = Program.GetEndFrame();

            return string.Join(Environment.NewLine, startFrame,
                                                                declare,
                                                                forCycle,
                                                                sort,
                                                                show,
                                                                endFrame);
        }

        private static string GetEndFrame()
        {
            return @"
        }
    }
}";
        }

        private static StringBuilder GetFirstCommas(int amountOfDimension)
        {
            // output is like ,,,
            StringBuilder firstCommas = new StringBuilder();
            for (int i = 0; i < amountOfDimension - 1; i++)
            {
                firstCommas.Append(",");
            }

            return firstCommas;
        }

        private static string GetForInitCycle(int amountOfDimension)
        {
            // output is like
            // for (int i0 = 0; i0 < array.GetLength(0); ++i0)
            // {
            //     for (int i1 = 0; i1 < array.GetLength(1); ++i1)
            //     {
            //         for (int i2 = 0; i2 < array.GetLength(2); ++i2)
            //         {
            //             array[i0, i1, i2] = CommonValues.Random.Next(0, 10);
            //         }
            //     }
            // }

            StringBuilder forsb = new StringBuilder();
            StringBuilder evalsb = new StringBuilder();
            StringBuilder forendsb = new StringBuilder();

            evalsb.Append("array[");
            for (int i = 0; i < amountOfDimension; i++)
            {
                forsb.AppendLine($"for (int i{i} = 0; i{i} < array.GetLength({i}); ++i{i}) {{");
                evalsb.Append($"i{i},");
                forendsb.AppendLine("}");
            }
            evalsb.Remove(evalsb.Length - 1, 1);
            evalsb.AppendLine("] = random.Next(0,10);");
            evalsb.AppendLine("amountNumberOfElements++;");

            return string.Join(Environment.NewLine, forsb,
                                                    evalsb,
                                                    forendsb);
        }

        private static StringBuilder GetSecondCommas(int amountOfDimension, int[] dimensionLength)
        {
            // output is like 1,2,3,4,5
            StringBuilder secondCommas = new StringBuilder();
            for (int i = 0; i < amountOfDimension; i++)
            {
                secondCommas.Append($"{dimensionLength[i]},");
            }
            secondCommas.Remove(secondCommas.Length - 1, 1);
            return secondCommas;
        }

        private static string GetShowCommands(int amountOfDimension)
        {
            // output is like
            // for (int i0 = 0; i0 < array.GetLength(0); ++i0)
            // {
            //     for (int i1 = 0; i1 < array.GetLength(1); ++i1)
            //     {
            //         for (int i2 = 0; i2 < array.GetLength(2); ++i2)
            //         {
            //             Console.WriteLine(array[i0, i1, i2]);
            //         }
            //     }
            // }

            StringBuilder forsb = new StringBuilder(); // for (...) {      x N times
            StringBuilder cwsb = new StringBuilder(); // code inside the for cycle
            StringBuilder forendsb = new StringBuilder(); // }     x N times
            cwsb.Append("Console.WriteLine(array[");

            for (int i = 0; i < amountOfDimension; i++)
            {
                forsb.AppendLine($"for (int i{i} = 0; i{i} < array.GetLength({i}); ++i{i}) {{");
                forendsb.AppendLine("}");
                cwsb.Append($"i{i},");
            }
            cwsb.Remove(cwsb.Length - 1, 1);
            cwsb.Append("]);");

            return string.Join(Environment.NewLine, forsb,
                                                    cwsb,
                                                    forendsb);
        }

        private static string GetSortCommands(int amountOfDimension)
        {
            // output is like
            // for (int i = 0; i < amountNumberOfElements; ++i)
            // {
            //     for (int i0 = 0; i0 < array.GetLength(0); ++i0)
            //     {
            //         for (int i1 = 0; i1 < array.GetLength(1); ++i1)
            //         {
            //             for (int i2 = 0; i2 < array.GetLength(2) - 1; ++i2)
            //             {
            //                 if (array[i0, i1, i2] > array[i0, i1, i2 + 1])
            //                 {
            //                     var tmp = array[i0, i1, i2];
            //                     array[i0, i1, i2] = array[i0, i1, i2 + 1];
            //                     array[i0, i1, i2 + 1] = tmp;
            //                 }
            //             }
            //         }
            //     }
            // }

            StringBuilder forsb = new StringBuilder(); // for (...) {      x N times
            StringBuilder ifsb = new StringBuilder(); // code inside the for cycle
            StringBuilder forendsb = new StringBuilder(); // }     x N times

            forsb.AppendLine($"for (int i = 0; i < amountNumberOfElements; ++i) {{"); // due to bubble sort works for N - 1 cycles for external loop.

            for (int i = 0; i < amountOfDimension; i++)
            {
                if (i == amountOfDimension - 1)
                {
                    forsb.AppendLine($"for (int i{i} = 0; i{i} < array.GetLength({i}) - 1; ++i{i}) {{"); // this is the most internal cycle
                }
                else
                {
                    forsb.AppendLine($"for (int i{i} = 0; i{i} < array.GetLength({i}); ++i{i}) {{");
                }

                ifsb.Append($"i{i},");
                forendsb.AppendLine("}");
            }

            forendsb.AppendLine("}");
            forendsb.AppendLine("}");
            ifsb.Remove(ifsb.Length - 1, 1);

            forsb.AppendLine($"if (array[{ifsb}] > array[{ifsb} + 1]) {{");
            forsb.AppendLine($"var tmp = array[{ifsb}];{Environment.NewLine}array[{ifsb}] = array[{ifsb} + 1];{Environment.NewLine}array[{ifsb} + 1] = tmp;");
            forsb.AppendLine(forendsb.ToString());

            return forsb.ToString();
        }

        private static string GetStartFrame()
        {
            // Namespace and class name are important here, due to Program.Run() expects them
            return @"
using System;
namespace DynaCore
{
    public class DynaCore
    {
        static public void Main()
        {";
        }

        private static void HandleError(CompilerResults compiled)
        {
            if (!compiled.Errors.HasErrors)
            {
                return;
            }

            string text = "Compile error: ";

            foreach (CompilerError ce in compiled.Errors)
            {
                text += "rn" + ce;
            }

            throw new Exception(text);
        }

        private static void Main()
        {
            const int amountOfDimension = 3;

            int[] dimensionLength = { 2, 3, 4 };

            string result = Program.GetCode(amountOfDimension, dimensionLength);

            Program.CompileAndRun(new[] { result });
        }

        private static void Run(CompilerResults compile)
        {
            MethodInfo methInfo = compile.CompiledAssembly
                                            .GetModules()[0]?
                                            .GetType("DynaCore.DynaCore")?
                                            .GetMethod("Main");

            if (methInfo != null)
            {
                Console.WriteLine(methInfo.Invoke(null, null));
            }
        }

        private static CompilerParameters SetUpCompiler()
        {
            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };

            string[] references = { "System.dll" };
            compilerParams.ReferencedAssemblies.AddRange(references);

            return compilerParams;
        }

        #endregion Private Methods
    }
}