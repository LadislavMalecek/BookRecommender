using System.Collections.Generic;
using System;

namespace BookRecommender.DataManipulation{
    abstract class SparqlEndPointMiner{
        protected abstract List<Dictionary<string,string>> Execute(string query);
        public abstract IEnumerable<string> GetBooks_Uri();

        public abstract IEnumerable<(string uri, string title, string labelCs, string labelEn)> GetBooks_Title_LabelCs_LabelEn();

        public abstract IEnumerable<(string uri, string langCode, string label)> GetBooksNamesByLangOfOrigin();

        public abstract IEnumerable<(string uri, string countryLangCode, string label)> GetBooksNamesByAuthorCountryLang();
    }
}