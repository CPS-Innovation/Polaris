namespace Common.Dto.Response;

public abstract class BaseCaseWitnessResponse
{
    public virtual int Id { get; set; }
    public virtual string ShoulderNumber { get; set; }
    public virtual string Title { get; set; }
    public virtual string Name { get; set; }
    public virtual bool HasStatements { get; set; }
    public virtual int? ListOrder { get; set; }
    public virtual bool Child { get; set; }
    public virtual bool Expert { get; set; }
    public virtual bool GreatestNeed { get; set; }
    public virtual bool Prisoner { get; set; }
    public virtual bool Interpreter { get; set; }
    public virtual bool Vulnerable { get; set; }
    public virtual bool Police { get; set; }
    public virtual bool Professional { get; set; }
    public virtual bool SpecialNeeds { get; set; }
    public virtual bool Intimidated { get; set; }
    public virtual bool Victim { get; set; }
}