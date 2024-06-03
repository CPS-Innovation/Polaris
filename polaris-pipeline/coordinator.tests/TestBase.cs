namespace coordinator.tests
{
    public abstract class TestBase
    {
        public void Setup()
        {
            Mother.BuildModels();
        }
    }
}