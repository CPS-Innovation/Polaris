using CommandLine;

namespace polaris.test_harness;

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Args>(args).WithParsed<Args>(a =>
        {
            General.EntryAsync(a).Wait();
        });
    }
}