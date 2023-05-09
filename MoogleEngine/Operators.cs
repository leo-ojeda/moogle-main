public class Operators

{
    public static List<string> Exclude(string query)
{
    // Separa la consulta en palabras individuales
    string[] words = query.Split();

    // Crea una lista para almacenar las palabras que deben ser excluidas
    List<string> excludeWords = new List<string>();

    // Busca las palabras que deben ser excluidas (las que tienen el símbolo ! delante)
    for (int i = 0; i < words.Length; i++)
    {
        if (words[i][0] == '!')
        {
            excludeWords.Add(words[i].Substring(1));
        }
    }

    // Crea una lista para almacenar los resultados
    List<string> results = new List<string>();

    // Obtiene la lista de documentos
    string[] documents = Document.GetDocuments();
    List<string> searchWords = new List<string>();

    // Itera sobre cada documento
    foreach (string documentPath in documents)
    {
        // Obtiene los términos del documento
        string[] terms = Document.GetTerms(documentPath, true);

        // Verifica si los términos del documento contienen todas las palabras de búsqueda
        bool containsAllWords = true;
        foreach (string word in searchWords)
        {
            if (!terms.Contains(word))
            {
                containsAllWords = false;
                break;
            }
        }

        // Verifica si el documento debe ser excluido
        bool excludeDocument = false;
        foreach (string excludeWord in excludeWords)
        {
            if (terms.Contains(excludeWord))
            {
                excludeDocument = true;
                break;
            }
        }

        // Agrega el documento a los resultados si cumple con los criterios de búsqueda
        if (containsAllWords && !excludeDocument)
        {
            results.Add(documentPath);
        }
    }

    // Devuelve los resultados finales
    return results;
}

}