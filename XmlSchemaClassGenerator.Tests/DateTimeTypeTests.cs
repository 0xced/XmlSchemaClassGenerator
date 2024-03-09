using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Xunit;

namespace XmlSchemaClassGenerator.Tests
{
    public sealed class DateTimeTypeTests
    {
        private static IEnumerable<string> ConvertXml(string xsd, Generator generatorPrototype)
        {
            var writer = new MemoryOutputWriter();

            var gen = new Generator
            {
                OutputWriter = writer,
                Version = new("Tests", "1.0.0.1"),
                NamespaceProvider = generatorPrototype.NamespaceProvider,
                GenerateNullables = generatorPrototype.GenerateNullables,
                DateTimeWithTimeZone = generatorPrototype.DateTimeWithTimeZone,
                DataAnnotationMode = generatorPrototype.DataAnnotationMode,
                GenerateDesignerCategoryAttribute = generatorPrototype.GenerateDesignerCategoryAttribute,
                GenerateComplexTypesForCollections = generatorPrototype.GenerateComplexTypesForCollections,
                NetCoreSpecificCode = generatorPrototype.NetCoreSpecificCode,
                EntityFramework = generatorPrototype.EntityFramework,
                AssemblyVisible = generatorPrototype.AssemblyVisible,
                GenerateInterfaces = generatorPrototype.GenerateInterfaces,
                MemberVisitor = generatorPrototype.MemberVisitor,
                CodeTypeReferenceOptions = generatorPrototype.CodeTypeReferenceOptions
            };

            var set = new XmlSchemaSet();

            using (var stringReader = new StringReader(xsd))
            {
                var schema = XmlSchema.Read(stringReader, (_, e) => throw new InvalidOperationException($"{e.Severity}: {e.Message}", e.Exception));
                ArgumentNullException.ThrowIfNull(schema);
                set.Add(schema);
            }

            gen.Generate(set);

            return writer.Content;
        }

        [Theory]
        [InlineData("date")]
        [InlineData("dateTime")]
        public void WhenDateTimeOffsetIsUsed_NoDataTypePropertyIsPresent(string dataType)
        {
            var xsd = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:complexType name=""document"">
		<xs:sequence>
			<xs:element name=""someDate"" type=""xs:{dataType}"" />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

            var generatedType = ConvertXml(
                xsd, new()
                {
                    NamespaceProvider = new()
                    {
                        GenerateNamespace = _ => "Test"
                    },
                    DateTimeWithTimeZone = true
                });

            var expectedXmlSerializationAttribute = "[System.Xml.Serialization.XmlElementAttribute(\"someDate\")]";
            var generatedProperty = generatedType.First();

            Assert.Contains(expectedXmlSerializationAttribute, generatedProperty);
        }

        [Fact]
        public void WhenDateOnlyIsUsed_NoDataTypePropertyIsPresent()
        {
            var xsd = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:complexType name=""document"">
		<xs:sequence>
			<xs:element name=""someDate"" type=""xs:date"" />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

            var generatedType = ConvertXml(
                xsd, new()
                {
                    NamespaceProvider = new()
                    {
                        GenerateNamespace = _ => "Test"
                    },
                    NetCoreSpecificCode = true,
                });

            var expectedXmlSerializationAttribute = "[System.Xml.Serialization.XmlElementAttribute(\"someDate\")]";
            var generatedProperty = generatedType.First();

            Assert.Contains(expectedXmlSerializationAttribute, generatedProperty);
        }

        [Fact]
        public void WhenDateTimeOffsetIsNotUsed_DataTypePropertyIsPresent()
        {
            var xsd = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:complexType name=""document"">
		<xs:sequence>
			<xs:element name=""someDate"" type=""xs:dateTime"" />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

            var generatedType = ConvertXml(
                xsd, new()
                {
                    NamespaceProvider = new()
                    {
                        GenerateNamespace = _ => "Test"
                    },
                    DateTimeWithTimeZone = false
                });

            var expectedXmlSerializationAttribute = "[System.Xml.Serialization.XmlElementAttribute(\"someDate\", DataType=\"dateTime\")]";
            var generatedProperty = generatedType.First();

            Assert.Contains(expectedXmlSerializationAttribute, generatedProperty);
        }

        [Theory]
        [InlineData(true, true, "System.DateTimeOffset")]
        [InlineData(true, false, "System.DateTimeOffset")]
        [InlineData(false, true, "System.DateTime")]
        [InlineData(false, false, "System.DateTime")]
        public void TestCorrectDateTimeDataType(bool dateTimeWithTimeZone, bool netCoreSpecificCode, string expectedType)
        {
            var xsd = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:complexType name=""document"">
		<xs:sequence>
			<xs:element name=""someDate"" type=""xs:dateTime"" />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

            var generatedType = ConvertXml(
                xsd, new()
                {
                    NamespaceProvider = new()
                    {
                        GenerateNamespace = _ => "Test"
                    },
                    NetCoreSpecificCode = netCoreSpecificCode,
                    DateTimeWithTimeZone = dateTimeWithTimeZone
                });

            var expectedProperty = $"public {expectedType} SomeDate";
            var generatedProperty = generatedType.First();

            Assert.Contains(expectedProperty, generatedProperty);
        }

        [Theory]
        [InlineData(true, true, "System.DateTimeOffset")]
        [InlineData(true, false, "System.DateTimeOffset")]
        [InlineData(false, true, "System.DateOnly")]
        [InlineData(false, false, "System.DateTime")]
        public void TestCorrectDateDataType(bool dateTimeWithTimeZone, bool netCoreSpecificCode, string expectedType)
        {
            var xsd = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:complexType name=""document"">
		<xs:sequence>
			<xs:element name=""someDate"" type=""xs:date"" />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

            var generatedType = ConvertXml(
                xsd, new()
                {
                    NamespaceProvider = new()
                    {
                        GenerateNamespace = _ => "Test"
                    },
                    NetCoreSpecificCode = netCoreSpecificCode,
                    DateTimeWithTimeZone = dateTimeWithTimeZone
                });

            var expectedProperty = $"public {expectedType} SomeDate";
            var generatedProperty = generatedType.First();

            Assert.Contains(expectedProperty, generatedProperty);
        }

    }
}
