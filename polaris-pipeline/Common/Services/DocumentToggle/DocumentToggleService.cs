using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Text;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Services.DocumentToggle.Domain;
using Common.Dto.Response.Document;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;

namespace Common.Services.DocumentToggle
{
    public class DocumentToggleService : IDocumentToggleService
    {
        private const string ConfigResourceName = "Common.document-toggle.config";

        private static readonly PresentationFlagsDto ReadOnly = new PresentationFlagsDto
        {
            Read = ReadFlag.Ok,
            Write = WriteFlag.DocTypeNotAllowed
        };

        private List<Definition> _definitions { get; set; }

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

            _definitions = CreateDefinitions(lines);
        }

        public PresentationFlagsDto GetDocumentPresentationFlags(CmsDocumentDto document)
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
            else if (document.IsDispatched)
            {
                write = WriteFlag.IsDispatched;
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

        public PresentationFlagsDto GetPcdRequestPresentationFlags(PcdRequestCoreDto pcdRequest)
        {
            return ReadOnly;
        }

        public PresentationFlagsDto GetDefendantAndChargesPresentationFlags(DefendantsAndChargesListDto defendantAndCharges)
        {
            return ReadOnly;
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
                    if (currentLine.StartsWith(DocumentToggleConstants.Comment))
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

        private DefinitionLevel GetLevelForFileType(CmsDocumentDto document)
        {
            var winningConfigLine = _definitions
                      .LastOrDefault(def => def.Type == DefinitionType.FileType
                        && (
                          def.Identifier == DocumentToggleConstants.Wildcard ||
                            def.Identifier.Equals(document.FileExtension, StringComparison.InvariantCultureIgnoreCase)));

            return winningConfigLine?.Level ?? DefinitionLevel.Deny;
        }

        private DefinitionLevel GetLevelForDocType(CmsDocumentDto document)
        {
            var winningConfigLine = _definitions
                      .LastOrDefault(def => def.Type == DefinitionType.DocType
                        && (
                          def.Identifier == DocumentToggleConstants.Wildcard ||
                            def.Identifier.Equals(document.CmsDocType.DocumentTypeId?.ToString(), StringComparison.InvariantCultureIgnoreCase)));

            return winningConfigLine?.Level ?? DefinitionLevel.Deny;
        }
    }
}