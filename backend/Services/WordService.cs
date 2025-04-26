namespace backend.Services;

public class WordService : IWordService
{
  private static readonly List<string> _wordList = new List<string>
  {
    "Apple", "House", "Tree", "Sun", "Cat", "Dog", "Flower", "Book", "Star", "Moon",
      "Ball", "Hat", "Car", "Boat", "Fish", "Bird", "Cloud", "Chair", "Cup", "Candle",
      "Heart", "Pencil", "Clock", "Carrot", "Umbrella", "Sock", "Butterfly", "Turtle", "Rainbow", "Cake",
      "Leaf", "Spider", "Ghost", "Snowman", "Balloon", "Rocket", "Pizza", "Ice cream", "Crown", "Robot",
      "Dragon", "Frog", "Penguin", "Guitar", "Elephant", "Octopus", "Cupcake", "Rainbow", "Snail", "Unicorn"
  };

  private List<string> _availableWords = new List<string>();

  /// <summary>
  /// Get random words from wordlist by count
  /// </summary>
  public List<string> GetRandomWords(int count)
  {
    Random random = new Random();
    _availableWords = _wordList
      .OrderBy(_ => random.Next())
      .Take(count)
      .ToList();

    return _availableWords;
  }


  /// <summary>
  /// Determines if the word provided as a parameter is available in the list of words the user can choose.
  /// </summary>
  public bool IsWordAvailable(string word)
  {
    return _availableWords.Contains(word);
  }

  public List<string> GetAvailableWords()
  {
    return _availableWords;
  }
}
