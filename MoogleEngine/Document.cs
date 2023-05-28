

public class Document
{

    ///<summary>En este metodo obtiene una lista de todos los documentos</summary>  

    public static string[] GetDocuments()

    {
        string contentPath = "../Content";
        string txt = "*.txt";
        string[] filePaths = Directory.GetFiles(contentPath, txt);
        return filePaths;
    }
    ///<summary>array de términos de un documento eliminando los caracteres de puntuación y las palabras vacías.</summary>
    public static string[] GetTerms(string documentPath, bool document)
    {


        string text = document ? File.ReadAllText(documentPath) : documentPath;
        string[] words = text.Split(new[] { ' ', '\r', '\n' }).Where(word => !string.IsNullOrEmpty(word)).ToArray();
        string[] terms = words.Select(word => word.Trim().ToLower().TrimEnd('+', '*', ',', '-', '.', ';', '?', '!', '(', ')')).ToArray();
        return terms;
    }
    ///<summary>la frecuencia de aparición de un término en un documento en el índice invertido.</summary> 
    public static int GetTermFrequency(string term, string document, Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        if (invertedIndex.ContainsKey(term) && invertedIndex[term].ContainsKey(document))
        {
            return invertedIndex[term][document];
        }
        return 0;
    }
    ///<summary>la cantidad de términos de la consulta que se ajustan a un documento en el índice invertido.</summary>
    public static double GetMatchedTerms(string document, List<string> queryTerms, Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        double matchedTerms = 0;
        foreach (string queryTerm in queryTerms)
        {
            if (GetTermFrequency(queryTerm, document, invertedIndex) > 0)
            {
                matchedTerms++;
            }
        }
        return matchedTerms;
    }

    ///<summary>crea un índice invertido que mapea términos a documentos y sus frecuencias de aparición.</summary>
    public static Dictionary<string, Dictionary<string, int>> CreateInvertedIndex(string[] documents)
    {
        Dictionary<string, Dictionary<string, int>> invertedIndex = new Dictionary<string, Dictionary<string, int>>();
        foreach (string document in documents)
        {
            string[] terms = GetTerms(document, true);
            foreach (string term in terms)
            {
                if (!invertedIndex.ContainsKey(term))
                {
                    invertedIndex[term] = new Dictionary<string, int>();
                }
                if (!invertedIndex[term].ContainsKey(document))
                {
                    invertedIndex[term][document] = 0;
                }
                invertedIndex[term][document]++;
            }
        }
        return invertedIndex;
    }
    ///<summary>calcula la magnitud del vector de consulta.</summary>
    public static Dictionary<string, double> CalculateDocumentMagnitudes(Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        Dictionary<string, double> documentMagnitudes = new Dictionary<string, double>();
        foreach (string document in invertedIndex.Values.First().Keys)
        {
            double magnitude = 0;
            foreach (Dictionary<string, int> postings in invertedIndex.Values)
            {
                if (postings.ContainsKey(document))
                {
                    magnitude += Math.Pow(postings[document], 2);
                }
            }
            magnitude = Math.Sqrt(magnitude);
            documentMagnitudes[document] = magnitude;
        }

        return documentMagnitudes;
    }
    ///<summary>crea un vector de consulta que mapea términos a sus pesos en base a su frecuencia y su idf.</summary>
    public static Dictionary<string, double> CreateQueryVector(string query, Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        string[] queryTerms = GetTerms(query, false);
        Dictionary<string, double> queryVector = new Dictionary<string, double>();
        int numDocs = invertedIndex.Values.Distinct().Count();
        foreach (string term in queryTerms)
        {
            if (invertedIndex.ContainsKey(term))
            {
                double tf = 1 + Math.Log10(queryTerms.Count(t => t == term));
                double idf = Math.Log10((double)numDocs / invertedIndex[term].Count);
                double weight = tf * Math.Pow(idf, 0.5);
                queryVector[term] = weight;

            }


        }
        return queryVector;
    }

    ///<summary>calcula el score de un documento en base a su similitud coseno con la consulta, su ponderación según la cantidad de términos de la consulta que se ajustan a él y su ponderación según la rareza de los términos de la consulta.</summary>
    public static double CalculateScore(string document, Dictionary<string, double> queryVector, Dictionary<string, Dictionary<string, int>> invertedIndex, Dictionary<string, double> documentMagnitudes)
    {
        double dotProduct = 0;

        // Calcular el producto punto entre el vector del documento y el vector de consulta
        foreach (var term in queryVector.Keys)
        {
            if (invertedIndex.ContainsKey(term) && invertedIndex[term].ContainsKey(document))
            {
                dotProduct += queryVector[term] * invertedIndex[term][document];
            }
        }

        // Calcular las magnitudes del vector del documento y del vector de consulta
        double documentMagnitude = documentMagnitudes.ContainsKey(document) ? documentMagnitudes[document] : 0;
        double queryMagnitude = CalculateQueryMagnitude(queryVector);

        // Calcular la similitud del coseno
        double cosineSimilarity = 0;
        if (documentMagnitude > 0 && queryMagnitude > 0)
        {
            cosineSimilarity = dotProduct / (documentMagnitude * queryMagnitude);
        }

        // Calcular la puntuación y restar la penalización por términos no coincidentes
        double score = cosineSimilarity - 2 * (queryVector.Keys.Count - (double)GetMatchedTerms(document, queryVector.Keys.ToList(), invertedIndex));

        return score;
    }



    ///<summary>//calcula la magnitud del vector de consulta.</summary>
    public static double CalculateQueryMagnitude(Dictionary<string, double> queryVector)
    {
        double queryMagnitudeSquared = queryVector.Values.Sum(weight => Math.Pow(weight, 2));
        return Math.Sqrt(queryMagnitudeSquared);
    }
    public static string GetSnippet(string documentContent, string query)
    {
        string[] queryWords = query.Split(' ');
        foreach (string word in queryWords)
        {
            int queryIndex = documentContent.IndexOf(" " + word + " ");
            if (queryIndex <= 0)
            {
                continue;
            }

            string snippet = "";

            int extralength = 100;
            while (queryIndex + extralength > documentContent.Length || !char.IsWhiteSpace(documentContent[queryIndex + extralength - 1]))
            {
                extralength--;
            }
            snippet = snippet + documentContent.Substring(queryIndex, extralength);



            return snippet;

        }

        return "";
    }






}
