namespace Ddei.Domain
{
    public class DdeiWitnessDto
    {
        public int Id { get; set; }
        public string ShoulderNumber { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public bool HasStatements { get; set; }
        public int? ListOrder { get; set; }
        public bool Child { get; set; }
        public bool Expert { get; set; }
        public bool GreatestNeed { get; set; }
        public bool Prisoner { get; set; }
        public bool Interpreter { get; set; }
        public bool Vulnerable { get; set; }
        public bool Police { get; set; }
        public bool Professional { get; set; }
        public bool SpecialNeeds { get; set; }
        public bool Intimidated { get; set; }
        public bool Victim { get; set; }
    }
}