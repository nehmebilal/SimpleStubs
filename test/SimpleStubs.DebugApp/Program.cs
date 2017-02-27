﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Etg.SimpleStubs.CodeGen;
using Etg.SimpleStubs.CodeGen.DI;

namespace SimpleStubs.DebugApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\\projects\\rdclient-universal\\RdClient\\GeneratedStubs\\GeneratedStubs.csproj";

            SimpleStubsGenerator stubsGenerator =
                new DiModule(path, @"..\..\Properties\SimpleStubs.generated.cs").StubsGenerator;
            string stubs = stubsGenerator.GenerateStubs(path).Result;
            File.WriteAllText(@"..\..\Properties\SimpleStubs.generated.cs", stubs);
        }
    }
}
