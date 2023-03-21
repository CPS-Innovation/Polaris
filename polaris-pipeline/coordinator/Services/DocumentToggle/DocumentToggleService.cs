using System.IO;
using System.Collections.Generic;
using coordinator.Domain.Tracker;
using coordinator.Services.DocumentToggle.Domain;
using System.Linq;
using System;
using coordinator.Services.DocumentToggle.Exceptions;
using coordinator.Domain.Tracker.Presentation;

namespace coordinator.Services.DocumentToggle
{
    public class DocumentToggleService : IDocumentToggleService
    {
        private const string ConfigFileName = "document-toggle.config";

        private List<Definition> _defintions { get; set; }

        public static string ReadConfig()
        {
            return File.ReadAllText(ConfigFileName);
        }

        public DocumentToggleService(string configFileContent)
        {
            if (configFileContent == null)
            {
                throw new ArgumentNullException(nameof(configFileContent));
            }

            if (_defintions != null)
            {
                throw new DocumentToggleException("Service being initialised an Nth time");
            }

            var lines = SplitConfigLines(configFileContent);

            _defintions = CreateDefinitions(lines);
        }

        public bool CanReadDocument(TrackerDocument document)
        {
            AssertIsInitialised();
            return document.PresentationFlags.ReadStatus == ReadFlag.Ok;
        }

        public bool CanWriteDocument(TrackerDocument document)
        {
            AssertIsInitialised();
            return document.PresentationFlags.WriteStatus == WriteFlag.Ok;
        }

        public PresentationFlags GetDocumentPresentationFlags(TransitionDocument document)
        {
            AssertIsInitialised();

            var levelForFileType = GetLevelForFileType(document);
            var levelForDocType = GetLevelForDocType(document);

            var readStatus = (levelForDocType == DefinitionLevel.Deny || levelForFileType == DefinitionLevel.Deny)
                              ? ReadFlag.OnlyAvailableInCms
                              : ReadFlag.Ok;

            WriteFlag writeStatus;
            if (readStatus == ReadFlag.OnlyAvailableInCms)
            {
                writeStatus = WriteFlag.OnlyAvailableInCms;
            }
            else if (levelForDocType != DefinitionLevel.ReadWrite)
            {
                writeStatus = WriteFlag.DocTypeNotAllowed;
            }
            else if (levelForFileType != DefinitionLevel.ReadWrite)
            {
                writeStatus = WriteFlag.OriginalFileTypeNotAllowed;
            }
            else
            {
                writeStatus = WriteFlag.Ok;
            }

            return new PresentationFlags
            {
                ReadStatus = readStatus,
                WriteStatus = writeStatus
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

        private DefinitionLevel GetLevelForFileType(TransitionDocument document)
        {
            var winningConfigLine = _defintions
                      .LastOrDefault(def => def.Type == DefinitionType.FileType
                        && (
                          (def.Identifier == Domain.Constants.Wildcard ||
                            def.Identifier.Equals(document.FileExtension, StringComparison.InvariantCultureIgnoreCase))));

            return winningConfigLine?.Level ?? DefinitionLevel.Deny;
        }

        private DefinitionLevel GetLevelForDocType(TransitionDocument document)
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