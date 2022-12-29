using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XmlSchemaClassGenerator
{
    public class FileOutputWriter : OutputWriter
    {
        public GeneratorConfiguration Configuration { get; set; }

        public FileOutputWriter(string directory, bool createIfNotExists = true)
        {
            OutputDirectory = directory;

            if (createIfNotExists && !Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }

        public string OutputDirectory { get; }

        /// <summary>
        /// A list of all the files written.
        /// </summary>
        public IList<string> WrittenFiles { get; } = new List<string>();

        public override void Write(NamespaceDeclarationSyntax tree)
        {

            var cu = SyntaxFactory.CompilationUnit();
            cu.AddUsings();
            //cu.Namespaces.Add(cn);

            if (Configuration?.SeparateClasses == true)
            {
                WriteSeparateFiles(tree);
            }
            else
            {
                var path = Path.Combine(OutputDirectory, tree.Name + ".cs");
                Configuration?.WriteLog(path);
                WriteFile(path, cu);
            }
        }

        protected virtual void WriteFile(string path, CompilationUnitSyntax cu)
        {
            FileStream fs = null;

            try
            {
                fs = new FileStream(path, FileMode.Create);
                using (var writer = new StreamWriter(fs))
                {
                    fs = null;
                    Write(writer, cu);
                }
                WrittenFiles.Add(path);
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        private void WriteSeparateFiles(NamespaceDeclarationSyntax cn)
        {
            var name = ValidateName(cn.Name.ToString());
            var dirPath = Path.Combine(OutputDirectory, name);
            var ccu = ParseCompilationUnit("");
            //var cns = new CodeNamespace(name);

            Directory.CreateDirectory(dirPath);

            cns.Imports.AddRange(cn.Imports.Cast<CodeNamespaceImport>().ToArray());
            cns.Comments.AddRange(cn.Comments);
            ccu.Namespaces.Add(cns);

            foreach (CodeTypeDeclaration ctd in cn.Members)
            {
                var path = Path.Combine(dirPath, ctd.Name + ".cs");
                cns.Types.Clear();
                cns.Types.Add(ctd);
                Configuration?.WriteLog(path);
                WriteFile(path, ccu);
            }
        }

        static readonly Regex InvalidCharacters = new($"[{string.Join("", Path.GetInvalidFileNameChars())}]", RegexOptions.Compiled);

        private string ValidateName(string name) => InvalidCharacters.Replace(name, "_");
    }
}