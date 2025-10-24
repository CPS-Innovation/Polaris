using Common.Dto.Response;
using System.Text.Json.Serialization;

namespace DdeiClient.Domain.Response;

public class MdsCaseWitnessResponse : BaseCaseWitnessResponse
{
    [JsonPropertyName("witnessId")]
    public override int Id { get; set; }
    public override string Name => $"{FirstName} {Surname}";
    public override bool HasStatements => WitnessStatements.Any();
    [JsonPropertyName("isChild")]
    public override bool Child { get; set; }
    [JsonPropertyName("isExpert")]
    public override bool Expert { get; set; }
    [JsonPropertyName("isGreatestNeed")]
    public override bool GreatestNeed { get; set; }
    [JsonPropertyName("isPrisoner")]
    public override bool Prisoner { get; set; }
    [JsonPropertyName("isInterpreter")]
    public override bool Interpreter { get; set; }
    [JsonPropertyName("isVulnerable")]
    public override bool Vulnerable { get; set; }
    [JsonPropertyName("isPolice")]
    public override bool Police { get; set; }
    [JsonPropertyName("isProfessional")]
    public override bool Professional { get; set; }
    [JsonPropertyName("isSpecialNeeds")]
    public override bool SpecialNeeds { get; set; }
    [JsonPropertyName("isIntimidated")]
    public override bool Intimidated { get; set; }
    [JsonPropertyName("isWitnessAndVictim")]
    public override bool Victim { get; set; }


    public string FirstName { get; set; }
    public string Surname { get; set; }
    public IEnumerable<WitnessStatementDto> WitnessStatements { get; set; }
    public ContactDetails ContactDetails
    {
        set
        {
            ShoulderNumber = value.ShoulderNo;
            Title = value.Title;
        }
    }
}

public class ContactDetails
{
    public string ShoulderNo { get; set; }
    public string Title { get; set; }
}