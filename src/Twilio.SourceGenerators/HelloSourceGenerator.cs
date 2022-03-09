using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twilio.SourceGenerators
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Build up the source code
            StringBuilder sourceBuilder = new StringBuilder();
            sourceBuilder.Append(@"// Auto-generated code
using System;
using Microsoft.AspNetCore.Http;
using Twilio.AspNet.Common;

namespace Twilio.AspNet.Core.MinimalApi
{
    public partial class TwilioRequestBinding<T> where T : TwilioRequest
    {
        static partial void BindFromForm(IFormCollection collection, T t) 
        {");
            var twilioRequestType = context.Compilation.GetTypeByMetadataName($"Twilio.AspNet.Common.TwilioRequest");
            var twilioRequestProperties = twilioRequestType.GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .ToList();
            foreach (var property in twilioRequestProperties)
            {
                sourceBuilder.Append($@"
            t.{property.Name} = collection[""{property.Name}""].ToString();");
            }

            sourceBuilder.Append(@"
            switch (t)
            {");
            // the order of the types matters!
            var requestTypes = new[]
            {
                "SmsStatusCallbackRequest",
                "SmsRequest",
                "StatusCallbackRequest",
                "VoiceRequest"
            };
            foreach (var typeName in requestTypes)
            {
                var type = context.Compilation.GetTypeByMetadataName($"Twilio.AspNet.Common.{typeName}");
                var fieldName = typeName.Substring(0, 1).ToLowerInvariant() + typeName.Substring(1);
                sourceBuilder.Append($@" 
                case {typeName} {fieldName}:");
                var typeProperties = type.GetMembers()
                    .Where(s => s.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Where(typeProperty =>
                        !twilioRequestProperties.Any(twilioRequestProperty => twilioRequestProperties == typeProperty))
                    .ToList();
                
                foreach (var property in typeProperties)
                {
                    var propertyTypeName = property.Type.NullableAnnotation != NullableAnnotation.Annotated
                        ? property.Type.Name
                        : (property.Type as INamedTypeSymbol).TypeArguments.Single().Name;
                    if (propertyTypeName == "String")
                    {
                        sourceBuilder.Append($@"
                    {fieldName}.{property.Name} = collection[""{property.Name}""].ToString();");
                    }
                    else
                    {
                        sourceBuilder.Append($@"
                    if(collection.ContainsKey(""{property.Name}""))
                    {{
                        {fieldName}.{property.Name} = {propertyTypeName}.Parse(collection[""{property.Name}""].ToString());
                    }}");
                    }
                }

                sourceBuilder.Append($@" 
                    break;");
            }

            sourceBuilder.Append(@"
            }
        }
    }
}
");
            // Add the source code to the compilation
            context.AddSource("MinimalApiTwiMLBinding.g.cs", sourceBuilder.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}