namespace backend.Services;

public interface IWordService
{
  List<string> GetRandomWords(int count);
  bool IsWordAvailable(string word);
  List<string> GetAvailableWords();
}
