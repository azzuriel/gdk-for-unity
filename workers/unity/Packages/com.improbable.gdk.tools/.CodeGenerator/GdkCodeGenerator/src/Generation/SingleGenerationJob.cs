using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Gdk.CodeGeneration.FileHandling;
using Improbable.Gdk.CodeGeneration.Jobs;
using Improbable.Gdk.CodeGeneration.Utils;

namespace Improbable.Gdk.CodeGenerator
{
    public class SingleGenerationJob : CodegenJob
    {
        private readonly List<GenerationTarget<UnityComponentDetails>> componentsToGenerate;

        private readonly List<GenerationTarget<UnityTypeDetails>> typesToGenerate;

        private readonly List<GenerationTarget<UnityEnumDetails>> enumsToGenerate;

        private const string FileExtension = ".cs";

        public SingleGenerationJob(string outputDir, DetailsStore store, IFileSystem fileSystem) : base(
            outputDir, fileSystem)
        {
            InputFiles = store.SchemaFiles.ToList();
            OutputFiles = new List<string>();

            var allNestedTypes = store.Types
                .SelectMany(kv => store.GetNestedTypes(kv.Key))
                .ToHashSet();

            typesToGenerate = store.Types
                .Where(kv => !allNestedTypes.Contains(kv.Key))
                .Select(kv => new GenerationTarget<UnityTypeDetails>(kv.Value, kv.Key.PackagePath))
                .ToList();

            enumsToGenerate = store.Enums
                .Where(kv => !allNestedTypes.Contains(kv.Key))
                .Select(kv => new GenerationTarget<UnityEnumDetails>(kv.Value, kv.Key.PackagePath))
                .ToList();

            componentsToGenerate = store.Components
                .Select(kv => new GenerationTarget<UnityComponentDetails>(kv.Value, kv.Key.PackagePath))
                .ToList();

            foreach (var typeTarget in typesToGenerate)
            {
                var fileName = Path.ChangeExtension(typeTarget.Content.CapitalisedName, FileExtension);
                OutputFiles.Add(Path.Combine(typeTarget.OutputPath, fileName));
            }

            foreach (var componentTarget in componentsToGenerate)
            {
                var relativeOutputPath = componentTarget.OutputPath;
                var componentName = componentTarget.Content.ComponentName;

                OutputFiles.Add(Path.Combine(relativeOutputPath, Path.ChangeExtension(componentTarget.Content.ComponentName, FileExtension)));

                if (componentTarget.Content.CommandDetails.Count > 0)
                {
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{componentName}CommandPayloads", FileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{component.Name}CommandManager", FileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{component.Name}CommandComponents", FileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{component.Name}CommandSenderReceiver", FileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
<<<<<<< HEAD
                        Path.ChangeExtension($"{component.Name}ReactiveCommandComponents", FileExtension)));
=======
                        Path.ChangeExtension($"{component.Name}ReactiveCommandComponents", fileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{component.Name}CommandDiffDeserializer", fileExtension)));
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{component.Name}CommandDiffStorage", fileExtension)));
>>>>>>> Removing the dispatcher (#747)
                }

                if (componentTarget.Content.EventDetails.Count > 0)
                {
                    OutputFiles.Add(Path.Combine(relativeOutputPath,
                        Path.ChangeExtension($"{componentName}Events", FileExtension)));
                }

                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}UpdateSender", FileExtension)));
                OutputFiles.Add(Path.Combine(relativeOutputPath,
<<<<<<< HEAD
                    Path.ChangeExtension($"{component.Name}Translation", FileExtension)));
=======
                    Path.ChangeExtension($"{component.Name}ReactiveHandlers", fileExtension)));
>>>>>>> Removing the dispatcher (#747)
                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}UpdateManager", FileExtension)));
                OutputFiles.Add(Path.Combine(relativeOutputPath,
<<<<<<< HEAD
                    Path.ChangeExtension($"{component.Name}ReactiveComponents", FileExtension)));
=======
                    Path.ChangeExtension($"{component.Name}ComponentDiffStorage", fileExtension)));
                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}ComponentDiffDeserializer", fileExtension)));
                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}ReactiveComponents", fileExtension)));
>>>>>>> Removing the dispatcher (#747)
                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}Providers", FileExtension)));
                OutputFiles.Add(Path.Combine(relativeOutputPath,
                    Path.ChangeExtension($"{component.Name}ComponentReaderWriter", FileExtension)));
            }

            foreach (var enumTarget in enumsToGenerate)
            {
                var fileName = Path.ChangeExtension(enumTarget.Content.TypeName, FileExtension);
                OutputFiles.Add(Path.Combine(enumTarget.OutputPath, fileName));
            }
        }

        protected override void RunImpl()
        {
            var typeGenerator = new UnityTypeGenerator();
            var enumGenerator = new UnityEnumGenerator();
            var eventGenerator = new UnityEventGenerator();
            var commandPayloadGenerator = new UnityCommandPayloadGenerator();
            var commandManagerGenerator = new UnityCommandManagerGenerator();
            var commandComponentsGenerator = new UnityCommandComponentsGenerator();
            var blittableComponentGenerator = new UnityComponentDataGenerator();
            var componentReactiveHandlersGenerator = new UnityReactiveComponentHandlersGenerator();
            var componentSenderGenerator = new UnityComponentSenderGenerator();
            var componentUpdateManagerGenerator = new UnityComponentUpdateManagerGenerator();
            var referenceTypeProviderGenerator = new UnityReferenceTypeProviderGenerator();
            var componentReaderWriterGenerator = new UnityComponentReaderWriterGenerator();
            var commandSenderReceiverGenerator = new UnityCommandSenderReceiverGenerator();
            var reactiveComponentGenerator = new ReactiveComponentGenerator();
            var reactiveCommandComponentGenerator = new ReactiveCommandComponentGenerator();
            var componentDiffStorageGenerator = new ComponentDiffStorageGenerator();
            var componentDiffDeserializerGenerator = new ComponentDiffDeserializerGenerator();
            var commandDiffDeserializerGenerator = new CommandDiffDeserializerGenerator();
            var commandDiffStorageGenerator = new CommandDiffStorageGenerator();

            foreach (var enumTarget in enumsToGenerate)
            {
                var fileName = Path.ChangeExtension(enumTarget.Content.TypeName, FileExtension);
                var enumCode = enumGenerator.Generate(enumTarget.Content, enumTarget.Package);
                Content.Add(Path.Combine(enumTarget.OutputPath, fileName), enumCode);
            }

            foreach (var typeTarget in typesToGenerate)
            {
                var fileName = Path.ChangeExtension(typeTarget.Content.CapitalisedName, FileExtension);
                var typeCode = typeGenerator.Generate(typeTarget.Content, typeTarget.Package);
                Content.Add(Path.Combine(typeTarget.OutputPath, fileName), typeCode);
            }

            foreach (var componentTarget in componentsToGenerate)
            {
                var relativeOutputPath = componentTarget.OutputPath;
                var componentName = componentTarget.Content.ComponentName;
                var package = componentTarget.Package;

                var componentFileName = Path.ChangeExtension(componentName, FileExtension);
                var componentCode = blittableComponentGenerator.Generate(componentTarget.Content, package);
                Content.Add(Path.Combine(relativeOutputPath, componentFileName), componentCode);

                if (componentTarget.Content.CommandDetails.Count > 0)
                {
                    var commandPayloadsFileName =
                        Path.ChangeExtension($"{componentName}CommandPayloads", FileExtension);
                    var commandPayloadCode =
                        commandPayloadGenerator.Generate(componentTarget.Content, package);
                    Content.Add(Path.Combine(relativeOutputPath, commandPayloadsFileName), commandPayloadCode);

                    var commandManagerFileName =
                        Path.ChangeExtension($"{component.Name}CommandManager", fileExtension);
                    var commandManagerCode =
                        commandManagerGenerator.Generate(component, package, enumSet);
                    Content.Add(Path.Combine(relativeOutputPath, commandManagerFileName), commandManagerCode);

                    var commandComponentsFileName =
                        Path.ChangeExtension($"{componentName}CommandComponents", FileExtension);
                    var commandComponentsCode =
                        commandComponentsGenerator.Generate(componentTarget.Content, package);
                    Content.Add(Path.Combine(relativeOutputPath, commandComponentsFileName), commandComponentsCode);

                    var commandSenderReceiverFileName =
                        Path.ChangeExtension($"{component.Name}CommandSenderReceiver", FileExtension);
                    var commandSenderReceiverCode =
                        commandSenderReceiverGenerator.Generate(component, package, enumSet);
                    Content.Add(Path.Combine(relativeOutputPath, commandSenderReceiverFileName), commandSenderReceiverCode);

                    var reactiveCommandComponentsFileName =
                        Path.ChangeExtension($"{component.Name}ReactiveCommandComponents", fileExtension);
                    var reactiveCommandComponentsCode =
                        reactiveCommandComponentGenerator.Generate(component, package, enumSet);
                    Content.Add(Path.Combine(relativeOutputPath, reactiveCommandComponentsFileName),
                        reactiveCommandComponentsCode);

                    var commandDiffDeserializerFileName =
                        Path.ChangeExtension($"{component.Name}CommandDiffDeserializer", fileExtension);
                    var commandDiffDeserializerCode =
                        commandDiffDeserializerGenerator.Generate(component, package, enumSet);
                    Content.Add(Path.Combine(relativeOutputPath, commandDiffDeserializerFileName),
                        commandDiffDeserializerCode);

                    var commandDiffStorageFileName =
                        Path.ChangeExtension($"{component.Name}CommandDiffStorage", fileExtension);
                    var commandDiffStorageCode =
                        commandDiffStorageGenerator.Generate(component, package, enumSet);
                    Content.Add(Path.Combine(relativeOutputPath, commandDiffStorageFileName),
                        commandDiffStorageCode);
                }

                if (componentTarget.Content.EventDetails.Count > 0)
                {
                    var eventsFileName = Path.ChangeExtension($"{componentName}Events", FileExtension);
                    var eventsCode = eventGenerator.Generate(componentTarget.Content, package);
                    Content.Add(Path.Combine(relativeOutputPath, eventsFileName), eventsCode);
                }

                var updateSenderFileName = Path.ChangeExtension($"{component.Name}UpdateSender", FileExtension);
                var updateSenderCode = componentSenderGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, updateSenderFileName), updateSenderCode);

                var reactiveComponentHandlersFileName = Path.ChangeExtension($"{component.Name}ReactiveHandlers", FileExtension);
                var reactiveComponentHandlersCode = componentReactiveHandlersGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, reactiveComponentHandlersFileName), reactiveComponentHandlersCode);

                var updateManagerFileName = Path.ChangeExtension($"{component.Name}UpdateManager", FileExtension);
                var updateManagerCode = componentUpdateManagerGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, updateManagerFileName), updateManagerCode);

                var componentDiffStorageFileName = Path.ChangeExtension($"{component.Name}ComponentDiffStorage", FileExtension);
                var componentDiffStorageCode = componentDiffStorageGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, componentDiffStorageFileName), componentDiffStorageCode);

                var componentDiffDeserializerFileName = Path.ChangeExtension($"{component.Name}ComponentDiffDeserializer", FileExtension);
                var componentDiffDeserializerCode = componentDiffDeserializerGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, componentDiffDeserializerFileName), componentDiffDeserializerCode);

                var reactiveComponentsFileName = Path.ChangeExtension($"{component.Name}ReactiveComponents", FileExtension);
                var reactiveComponentsCode = reactiveComponentGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, reactiveComponentsFileName), reactiveComponentsCode);

                var referenceProviderFileName = Path.ChangeExtension($"{component.Name}Providers", FileExtension);
                var referenceProviderTranslationCode =
                    referenceTypeProviderGenerator.Generate(componentTarget.Content, package);
                Content.Add(Path.Combine(relativeOutputPath, referenceProviderFileName),
                    referenceProviderTranslationCode);

                var componentReaderWriterFileName =
                    Path.ChangeExtension($"{component.Name}ComponentReaderWriter", FileExtension);
                var componentReaderWriterCode =
                    componentReaderWriterGenerator.Generate(component, package, enumSet);
                Content.Add(Path.Combine(relativeOutputPath, componentReaderWriterFileName), componentReaderWriterCode);
            }
        }

        private struct GenerationTarget<T>
        {
            public readonly T Content;
            public readonly string Package;
            public readonly string OutputPath;

            public GenerationTarget(T content, string package)
            {
                Content = content;
                Package = Formatting.CapitaliseQualifiedNameParts(package);
                OutputPath = Formatting.GetNamespacePath(package);
            }
        }
    }
}
