namespace PasteBinASP.Services;

public interface IHashService
{
    string CalculateHash(string input);
    string CalculateHash(int input);
    Task<string> GetUniqueHashAsync();
}
