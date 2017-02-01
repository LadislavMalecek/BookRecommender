using System;
using System.Collections.Generic;
using BookRecommender.DataManipulation;

namespace BookRecommender.DataManipulation.WikiData
{

    class WikiDataEndpointMiner : SparqlEndPointMiner
    {
        IMiner miner = new WikiDataMiner();
        IParser parser = new WikiDataCsvParser();

        protected override List<Dictionary<string, string>> Execute(string query)
        {
            var rawData = miner.MineData(query);
            return parser.Parse(rawData);
        }

        public override IEnumerable<string> GetBooks_Uri()
        {
            var query = @"SELECT ?item
                        WHERE {
                            ?item wdt:P31 wd:Q571.
                        }";

            var result = Execute(query);
            foreach (var line in result)
            {
                yield return line["item"];
            }
            yield break;
        }

        public List<(string uri, string title, string labelCs, string labelEn)> asdf(){
            return null;
        }
        public override IEnumerable<(string uri, string title, string labelCs, string labelEn)> GetBooks_Title_LabelCs_LabelEn()
        {
            var query = @"SELECT ?item ?title ?label_cs ?label_en
                        WHERE {
                            ?item wdt:P31 wd:Q571.
                            OPTIONAL{
                                ?item wdt:P1476 ?title.
                            }
                            OPTIONAL{
                                ?item rdfs:label ?label_cs.
                                    FILTER(LANG(?label_cs) = ""cs"")
                            }
                            OPTIONAL{
                                ?item rdfs:label ?label_en.
  	                                FILTER(LANG(?label_en) = ""en"")
                            }
                        }";
            var result = Execute(query);
            foreach (var line in result)
            {
                yield return (line["item"], line["title"], line["label_cs"], line["label_en"]);
            }
            yield break;
        }

        public override IEnumerable<(string uri, string langCode, string label)> GetBooksNamesByLangOfOrigin()
        {
            var query = @"SELECT ?item ?lang_code ?label
                        WHERE {
                            ?item wdt:P31 wd:Q571.
                            ?item wdt:P364 ?obj.
                            ?obj wdt:P424 ?lang_code.
                            ?item rdfs:label ?label.
                                FILTER(LANG(?label) = ?lang_code)
                        }";
            var result = Execute(query);
            foreach (var line in result)
            {
                yield return (line["item"], line["lang_code"], line["label"]);
            }
            yield break;
        }

        public override IEnumerable<(string uri, string countryLangCode, string label)> GetBooksNamesByAuthorCountryLang()
        {
            var query = @"SELECT ?item ?country_lang_code ?label
                            WHERE {
                                ?item wdt:P31 wd:Q571.
                                ?item wdt:P50 ?author.
                                ?author wdt:P27 ?country_citiz.
                                ?country_citiz wdt:P37 ?country_lang.
                                ?country_lang wdt:P424 ?country_lang_code.
                            
                                ?item rdfs:label ?label.
                                    FILTER(LANG(?label) = ?country_lang_code)
                            }";
            var result = Execute(query);
            foreach (var line in result)
            {
                yield return (line["item"], line["country_lang_code"], line["label"]);
            }
            yield break;
        }
        
    }
}