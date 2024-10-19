namespace Common.tests.Services
{
    public abstract class TestBase
    {
        public void Setup()
        {
            Mother.BuildModels();
        }
    }
}