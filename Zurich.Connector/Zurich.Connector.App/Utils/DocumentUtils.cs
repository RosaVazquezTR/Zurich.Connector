using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Zurich.Connector.App.Utils
{
    /// <summary>
    /// Provides utility methods for working with document snippets.
    /// </summary>
    public static class DocumentUtils
    {
        /// <summary>
        /// Highlights the query in the snippets of the provided documents.
        /// </summary>
        /// <param name="documents">The documents containing snippets to be highlighted.</param>
        /// <param name="queryParameters">The query parameters containing the query to highlight.</param>
        /// <returns>A JArray with the highlighted snippets.</returns>
        public static JArray HighlightQueryInDocumentSnippets(JArray documents, Dictionary<string, string> queryParameters)
        {
            if (queryParameters.TryGetValue("Query", out string query))
            {
                JArray highlightedDocuments = [];

                foreach (dynamic document in documents)
                {
                    HighlightSnippetsInDocument(document, query);
                    highlightedDocuments.Add(document);
                }

                return highlightedDocuments;
            }

            return documents;
        }

        /// <summary>
        /// Highlights the query in the snippets of a single document.
        /// </summary>
        /// <param name="document">The document containing snippets to be highlighted.</param>
        /// <param name="query">The query to highlight.</param>
        private static void HighlightSnippetsInDocument(dynamic document, string query)
        {
            if (document.Snippet is not null)
            {
                document.Snippet = HighlightQueryInSingleSnippet(document.Snippet.ToString(), query);
            }

            if (document.Snippets is not null)
            {
                document.Snippets = HighlightQueryInMultipleSnippets(document.Snippets, query);
            }
        }

        /// <summary>
        /// Highlights the query in a single snippet.
        /// </summary>
        /// <param name="snippet">The snippet to be highlighted.</param>
        /// <param name="query">The query to highlight.</param>
        /// <returns>The highlighted snippet.</returns>
        private static string HighlightQueryInSingleSnippet(string snippet, string query)
        {
            if (IsSnippetNotHighlighted(snippet))
            {
                return HighlightQueryInText(snippet, query);
            }

            return snippet;
        }

        /// <summary>
        /// Checks if the snippet is already highlighted.
        /// </summary>
        /// <param name="snippet">The snippet to check.</param>
        /// <returns>True if the snippet is already highlighted, otherwise false.</returns>
        private static bool IsSnippetNotHighlighted(string snippet)
        {
            string pattern = @"<strong>|<b>|<span class=co_searchTerm>";
            return !Regex.IsMatch(WebUtility.HtmlDecode(snippet), pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Highlights the search query in the provided summary text.
        /// </summary>
        /// <param name="text">The summary text where the query should be highlighted.</param>
        /// <param name="query">The search query to highlight in the summary.</param>
        /// <returns>The summary text with the query highlighted.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the summary or query is null or empty.</exception>
        private static string HighlightQueryInText(string text, string query)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("The summary text or query cannot be null or empty.");
            }

            string[] queryWords = query.Split(' ');

            foreach (string word in queryWords)
            {
                string pattern = $@"\b{Regex.Escape(word)}\b";
                text = Regex.Replace(text, pattern, $"<b>{word}</b>", RegexOptions.IgnoreCase);
            }

            return text;
        }

        /// <summary>
        /// Highlights the query in multiple snippets.
        /// </summary>
        /// <param name="snippets">The snippets to be highlighted.</param>
        /// <param name="query">The query to highlight.</param>
        /// <returns>A JArray with the highlighted snippets.</returns>
        private static JArray HighlightQueryInMultipleSnippets(JArray snippets, string query)
        {
            JArray highlightedSnippets = [];

            foreach (dynamic snippet in snippets)
            {
                string highlightedSnippet = snippet.ToString();

                if (IsSnippetNotHighlighted(highlightedSnippet))
                {
                    highlightedSnippet = HighlightQueryInSingleSnippet(highlightedSnippet, query);
                }

                highlightedSnippets.Add(highlightedSnippet);
            }

            return highlightedSnippets;
        }
    }
}
