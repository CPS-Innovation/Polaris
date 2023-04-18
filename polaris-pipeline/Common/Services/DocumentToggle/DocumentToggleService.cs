using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Text;
using Common.Dto.Tracker;
using Common.Dto.FeatureFlags;
using Common.Services.DocumentToggle.Domain;
using Common.Services.DocumentToggle.Exceptions;
using Common.Dto.Document;
using Common.Dto.Case.PreCharge;

namespace Common.Services.DocumentToggle
{
    public class DocumentToggleService : IDocumentToggleService
    {
        private const string ConfigResourceName = "Common.document-toggle.config";

        private List<Definition> _defintions { get; set; }

        public static string ReadConfig()
        {
            var assembly = Assembly.GetAssembly(typeof(DocumentToggleService));
            var resourceStream = assembly.GetManifestResourceStream(ConfigResourceName);
            using var reader = new StreamReader(resourceStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public DocumentToggleService(string configFileContent)
        {

            if (configFileContent == null)
            {
                throw new ArgumentNullException(nameof(configFileContent));
            }

            var lines = SplitConfigLines(configFileContent);

            _defintions = CreateDefinitions(lines);
        }

        public bool CanReadDocument(TrackerCmsDocumentDto document)
        {
            return document.PresentationFlags.Read == ReadFlag.Ok;
        }

        public bool CanWriteDocument(TrackerCmsDocumentDto document)
        {
            return document.PresentationFlags.Write == WriteFlag.Ok;
        }

        public PresentationFlagsDto GetDocumentPresentationFlags(DocumentDto document)
        {
            var levelForFileType = GetLevelForFileType(document);
            var levelForDocType = GetLevelForDocType(document);

            var read = (levelForDocType == DefinitionLevel.Deny || levelForFileType == DefinitionLevel.Deny)
                              ? ReadFlag.OnlyAvailableInCms
                              : ReadFlag.Ok;

            WriteFlag write;
            if (read == ReadFlag.OnlyAvailableInCms)
            {
                write = WriteFlag.OnlyAvailableInCms;
            }
            else if (levelForDocType != DefinitionLevel.ReadWrite)
            {
                write = WriteFlag.DocTypeNotAllowed;
            }
            else if (levelForFileType != DefinitionLevel.ReadWrite)
            {
                write = WriteFlag.OriginalFileTypeNotAllowed;
            }
            else
            {
                write = WriteFlag.Ok;
            }

            return new PresentationFlagsDto
            {
                Read = read,
                Write = write
            };
        }

        // TODO - this might needs expanded
        public PresentationFlagsDto GetPcdRequestPresentationFlags(PcdRequestDto pcdRequest)
        {
            return new PresentationFlagsDto
            {
                Read = ReadFlag.Ok,
                Write = WriteFlag.DocTypeNotAllowed
            };
        }

        private void AssertIsInitialised()
        {
            if (_defintions == null)
            {
                throw new DocumentToggleException("DocumentToggleService not initialised when processing document");
            }
        }

        private string[] SplitConfigLines(string content)
        {
            try
            {
                return content.Split(
                  new string[] { "\n", "\r\n" },
                  StringSplitOptions.None // allow empty lines so we can feed back the accurate line an error occurred 
                );
            }
            catch (Exception exception)
            {
                throw new DocumentToggleException(
                  $"Error splitting config file",
                  exception
                );
            }
        }

        private List<Definition> CreateDefinitions(string[] lines)
        {
            var definitions = new List<Definition>();

            int lineIndex = 0;
            string currentLine = string.Empty;
            try
            {
                for (; lineIndex < lines.Count(); lineIndex++)
                {
                    currentLine = lines[lineIndex]
                                    .Trim();
                    if (currentLine.StartsWith(Domain.Constants.Comment))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(currentLine))
                    {
                        continue;
                    }

                    // no argument to .Split means split on whitespace
                    var lineSplits = currentLine
                        .Split()
                        .Where(split => !string.IsNullOrWhiteSpace(split))
                        .ToArray();
                    var type = (DefinitionType)Enum.Parse(typeof(DefinitionType), lineSplits[0]);
                    var level = (DefinitionLevel)Enum.Parse(typeof(DefinitionLevel), lineSplits[1]);
                    var identifier = lineSplits[2];

                    definitions.Add(new Definition
                    {
                        Type = type,
                        Level = level,
                        Identifier = identifier
                    });
                }
            }
            catch (Exception exception)
            {
                throw new DocumentToggleException(
                  $"Error processing config line {lineIndex}: {currentLine}",
                  exception
                );
            }

            return definitions;
        }

        private DefinitionLevel GetLevelForFileType(DocumentDto document)
        {
            var winningConfigLine = _defintions
                      .LastOrDefault(def => def.Type == DefinitionType.FileType
                        && (
                          (def.Identifier == Domain.Constants.Wildcard ||
                            def.Identifier.Equals(document.FileExtension, StringComparison.InvariantCultureIgnoreCase))));

            return winningConfigLine?.Level ?? DefinitionLevel.Deny;
        }

        private DefinitionLevel GetLevelForDocType(DocumentDto document)
        {
            var winningConfigLine = _defintions
                      .LastOrDefault(def => def.Type == DefinitionType.DocType
                        && (
                          (def.Identifier == Domain.Constants.Wildcard ||
                            def.Identifier.Equals(document.CmsDocType.DocumentType, StringComparison.InvariantCultureIgnoreCase))));

            return winningConfigLine?.Level ?? DefinitionLevel.Deny;
        }
    }
}