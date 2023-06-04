
using System.Text.RegularExpressions;
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
        //documentPath = "../Content";

        string text = document ? File.ReadAllText(documentPath) : documentPath;
        string[] words = text.Split(new[] { ' ', '\r', '\n' }).Where(word => !string.IsNullOrEmpty(word)).ToArray();
        string[] terms = words.Select(word => word.Trim().ToLower().TrimEnd('+', '*', ',', '-', '.', ';', '?', '!', '(', ')')).ToArray();
        return terms;
    }

    ///<summary>la frecuencia de aparición de un término en un documento en el índice invertido.</summary> 
    public static int GetTermFrequency(string term, string document, Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        if (invertedIndex.ContainsKey(term))
        {
            if (invertedIndex[term].ContainsKey(document))
            {
                return invertedIndex[term][document];
            }
        }
        return 0;
    }
    ///<summary>la cantidad de términos de la consulta que se ajustan a un documento en el índice invertido.</summary>
    public static Dictionary<string, double> GetMatchedTerms(string document, List<string> queryTerms, Dictionary<string, Dictionary<string, int>> invertedIndex)
    {
        Dictionary<string, double> termWeights = new Dictionary<string, double>();
        double totalTerms = invertedIndex.Count;
        foreach (string queryTerm in queryTerms)
        {
            System.Diagnostics.Trace.WriteLine($"Buscando término: {queryTerm}");
            int termFrequency = GetTermFrequency(queryTerm, document, invertedIndex);
            if (termFrequency > 0)
            {
                double termWeight = (double)termFrequency / totalTerms;
                termWeights.Add(queryTerm, termWeight);
            }
        }
        System.Diagnostics.Trace.WriteLine($"Términos coincidentes encontrados: {termWeights.Count}");
        return termWeights;
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
        // Obtener los términos de la consulta
        string[] queryTerms = GetTerms(query, false);

        // Crear un diccionario para almacenar los pesos de los términos de la consulta
        Dictionary<string, double> queryVector = new Dictionary<string, double>();

        // Obtener el número total de documentos en el índice invertido
        int numDocs = invertedIndex.Values.Distinct().Count();

        // Calcular el peso de cada término en la consulta
        foreach (string term in queryTerms)
        {
            if (invertedIndex.ContainsKey(term))
            {
                // Calcular la frecuencia del término en la consulta
                double tf = 1 + Math.Log10(queryTerms.Count(t => t == term));

                // Calcular la frecuencia inversa del documento para el término
                double idf = Math.Log10((double)numDocs / (double)(1 + invertedIndex[term].Keys.Count));

                // Calcular el peso del término
                double weight = tf * Math.Pow(idf, 0.5);

                // Agregar el término y su peso al diccionario del vector de consulta
                queryVector[term] = weight;
            }
        }

        // Devolver el diccionario del vector de consulta
        return queryVector;
    }
    ///<summary>calcula el score de un documento en base a su similitud coseno con la consulta, su ponderación según la cantidad de términos de la consulta que se ajustan a él y su ponderación según la rareza de los términos de la consulta.</summary>
   public static double CalculateScore(string document, Dictionary<string, double> queryVector, Dictionary<string, Dictionary<string, int>> invertedIndex, Dictionary<string, double> documentMagnitudes)
{
    double score = 0;

    int numDocuments = invertedIndex.Values.SelectMany(dict => dict.Keys).Distinct().Count();

    foreach (var term in queryVector.Keys)
    {
        if (invertedIndex.ContainsKey(term))
        {
            int termFreq = 0;
            if (invertedIndex[term].ContainsKey(document))
            {
                termFreq = invertedIndex[term][document];
            }

            if (invertedIndex[term].Values.Count(f => f > 1000) > 1)
            {
                termFreq = 0;
            }
            Console.WriteLine("Frecuencia:"+termFreq);

            double idf = Math.Log(numDocuments / (double)invertedIndex[term].Keys.Count);
            double queryWeight = queryVector[term] * idf;
            double docWeight = termFreq * idf;
            score += queryWeight * docWeight;
        }
    }

    if (documentMagnitudes.ContainsKey(document))
    {
        score /= documentMagnitudes[document];
    }

    if (score == 0)
    {
        return -1;
    }

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
