namespace MoogleEngine;




public static class Moogle
{
    public static SearchResult Query(string query)
{
    query = query.ToLower();

    string[] documents = Document.GetDocuments();

    Dictionary<string, Dictionary<string, int>> invertedIndex = Document.CreateInvertedIndex(documents);

    Dictionary<string, double> documentMagnitudes = Document.CalculateDocumentMagnitudes(invertedIndex);

    Dictionary<string, double> queryVector = Document.CreateQueryVector(query, invertedIndex);

    List<string> filteredDocuments = Operators.Exclude(query);

    List<SearchItem> results = new List<SearchItem>();
    foreach (string document in filteredDocuments)
    {
        double score = Document.CalculateScore(document, queryVector, invertedIndex, documentMagnitudes);

        string documentContent = File.ReadAllText(document);

        string snippet = Document.GetSnippet(documentContent, query);

        results.Add(new SearchItem(document.Substring(11, document.Length - 5 - 10), snippet, (float)score));
    }

    // Ordenar los documentos por score de mayor a menor y obtener solo los 7 con mayor score
    var topResults = results.OrderByDescending(x => x.Score).Take(6).ToList();

    return new SearchResult(topResults.ToArray(), query);
} 

}

