namespace PainKiller.ReadLine.Contracts;

public interface IUserInputReader
{
    string ReadLine(string prompt = "");
}
