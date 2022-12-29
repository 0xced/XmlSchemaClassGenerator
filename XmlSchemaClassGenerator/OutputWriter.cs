using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlSchemaClassGenerator
{
    public abstract class OutputWriter
    {
        protected OutputWriter()
        {
        }

        public abstract void Write(NamespaceDeclarationSyntax tree);

        protected void Write(TextWriter writer, CompilationUnitSyntax cu)
        {
            cu.WriteTo(writer);
        }
    }
}