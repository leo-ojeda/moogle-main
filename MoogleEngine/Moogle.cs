namespace MoogleEngine;
using System.Text.RegularExpressions;




public static class Moogle
{
   public static SearchResult Query(string query)
{
    // Eliminar los signos de puntuación y las tildes de la consulta
    query = Regex.Replace(query.ToLower(), @"[^\w\s]", "");
    query = Regex.Replace(query, @"[áéíóú]", m => "aeiou".Substring("áéíóú".IndexOf(m.Value.ToLower()), 1));
    query = query.Trim();

    // Obtener los documentos
    string[] documents = Document.GetDocuments();

    // Crear el índice invertido
    Dictionary<string, Dictionary<string, int>> invertedIndex = Document.CreateInvertedIndex(documents);

    // Calcular las magnitudes de los documentos
    Dictionary<string, double> documentMagnitudes = Document.CalculateDocumentMagnitudes(invertedIndex);

    // Crear el vector de consulta
    Dictionary<string, double> queryVector = Document.CreateQueryVector(query, invertedIndex);

    // Obtener los documentos filtrados
    List<string> filteredDocuments = Operators.Exclude(query);

    // Crear una lista para almacenar los resultados
    List<SearchItem> results = new List<SearchItem>();

    // Iterar sobre los documentos filtrados y calcular el score de cada uno
    foreach (string document in filteredDocuments)
    {
        double score = Document.CalculateScore(document, queryVector, invertedIndex, documentMagnitudes);

        // Leer el contenido del documento y eliminar los signos de puntuación y las tildes
        string documentContent = File.ReadAllText(document).ToLower();
        documentContent = Regex.Replace(documentContent, @"[^\w\s]", "");
        documentContent = Regex.Replace(documentContent, @"[áéíóú]", m => "aeiou".Substring("áéíóú".IndexOf(m.Value.ToLower()), 1));

        // Obtener los snippets
        string snippets = Document.GetSnippet(documentContent, query);

        // Agregar el resultado a la lista
        results.Add(new SearchItem(document.Substring(11, document.Length - 5 - 10), snippets, (float)score));
    }

    // Ordenar los resultados por score de mayor a menor y obtener solo los 6 con mayor score
    var topResults = results.OrderByDescending(x => x.Score).Take(6).ToList();

    // Devolver los resultados y la consulta
    return new SearchResult(topResults.ToArray(), query);
}
}

