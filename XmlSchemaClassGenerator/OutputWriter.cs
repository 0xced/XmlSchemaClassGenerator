using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;

namespace XmlSchemaClassGenerator
{
    public abstract class OutputWriter
    {
        private readonly bool useNullableReferenceTypes;

        #warning removing the default constructor is a breaking change.
        // Is this acceptable? The alternative is to default to useNullableReferenceTypes: false but subclasses
        // (other than known one in XmlSampleGenerator, i.e. FileOutputWriter) will never know about it.
#if false
        protected OutputWriter() : this(useNullableReferenceTypes: false)
        {
        }
#endif

        protected OutputWriter(bool useNullableReferenceTypes)
        {
            this.useNullableReferenceTypes = useNullableReferenceTypes;
        }

        protected virtual CodeGeneratorOptions Options { get; } = new CodeGeneratorOptions
        {
            VerbatimOrder = true,
            BracingStyle = "C"
        };

        protected virtual CodeDomProvider Provider { get; } = new Microsoft.CSharp.CSharpCodeProvider();

        public abstract void Write(CodeNamespace cn);

        protected void Write(TextWriter writer, CodeCompileUnit cu)
        {
            using var sw = new SemicolonRemovalTextWriter(writer);
            if (useNullableReferenceTypes)
            {
                Provider.GenerateCodeFromCompileUnit(new CodeSnippetCompileUnit("#nullable enable"), sw, Options);
            }
            cu.StartDirectives.Add(new CodeDirective());
            Provider.GenerateCodeFromCompileUnit(cu, sw, Options);
        }

        /// <summary>
        /// A wrapper around <see cref="TextWriter"/> that removes semicolons after a closing brace
        /// due to a bug in CodeDom and auto-properties
        /// </summary>
        private sealed class SemicolonRemovalTextWriter : TextWriter
        {
            private readonly TextWriter _other;

            private bool _previousWasClosingBrace;

            public SemicolonRemovalTextWriter(TextWriter other)
            {
                _other = other;
            }

            public override Encoding Encoding => _other.Encoding;

            public override void Write(char value)
            {
                if (!(value == ';' && _previousWasClosingBrace))
                {
                    _other.Write(value);
                }

                _previousWasClosingBrace = value == '}';
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (_other != null)
                {
                    _other.Dispose();
                }
            }
        }
    }
}