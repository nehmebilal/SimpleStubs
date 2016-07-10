﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen.CodeGen
{
    internal interface IProjectStubber
    {
        Task<StubProjectResult> StubProject(Project project, CompilationUnitSyntax cu);
    }
}