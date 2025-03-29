public class TraducatorIngrediente
{
    private static readonly Dictionary<string, string> dictionarIngrediente = new Dictionary<string, string>
    {
        { "water", "Apa" },
        { "bell paper", "Ardei" },
        { "avocado", "Avocado" },
        { "bananas", "Banane" },
        { "basil", "Busuioc" },
        { "strawberries", "Capsuni" },
        { "potatoes", "Cartofi" },
        { "cucumbers", "Castraveti" },
        { "onion", "Ceapa" },
        { "pistachio cream", "Crema de fistic" },
        { "edamame", "Edamame" },
        { "vanilla extract", "Esenta de vanilie" },
        { "almond flour", "Faina de migdale" },
        { "gluten-free flour", "Faina fara gluten" },
        { "green beans", "Fasole verde" },
        { "pistachio", "Fistic" },
        { "granola", "Granola" },
        { "wheat flour", "Faina de grau" },
        { "yogurt", "Iaurt" },
        { "olives", "Masline" },
        { "almonds", "Migdale" },
        { "walnuts", "Nuci" },
        { "rice", "Orez" },
        { "oats", "Oua" },
        { "parsley", "Patrunjel" },
        { "baking powder", "Praf de copt" },
        { "tomatoes", "Rosii" },
        { "cinnamon", "Scortisoara" },
        { "vegan sour cream", "SmantanaVegana" },
        { "salmon", "Somon" },
        { "tomato sauce", "Sos de rosii" },
        { "teriyaki sauce", "Sos Teriyaki" },
        { "sesame", "Susan" },
        { "coconut oil", "Ulei de cocos" },
        { "olive oil", "Ulei de masline" },
        { "sugar", "Zahar" },
        { "raspberries", "Zmeura" }
    };

    // Funcție pentru a traduce un ingredient
    public static string TraducereIngredient(string englezesc)
    {
        return dictionarIngrediente.TryGetValue(englezesc.ToLower(), out string romana) ? romana : englezesc;
    }
}
