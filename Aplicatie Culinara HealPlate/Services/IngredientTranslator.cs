namespace Aplicatie_Culinara_HealPlate.Services
{
    public static class IngredientTranslator
    {
        private static readonly Dictionary<string, string> _dictionary = new()
    {
        { "apple", "măr" }, { "cheese", "brânză" }, { "egg", "ou" },
        { "milk", "lapte" }, { "carrot", "morcov" }, { "tomato", "roșie" },
        { "broccoli", "broccoli" }, { "chicken", "pui" }, { "wheat", "grâu" },
        { "banana", "banane" }, { "cucumber", "castravete" }
    };

        public static List<string> Translate(List<string> ingredients)
        {
            return ingredients
                .Where(i => _dictionary.ContainsKey(i))
                .Select(i => _dictionary[i])
                .ToList();
        }
    }
}
