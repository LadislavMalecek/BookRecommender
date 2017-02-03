using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models;

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

        public override void UpdateBooks()
        {   
            base.UpdateDatabase(GetBooksUri(), SaveBooksUri);
            base.UpdateDatabase(GetBooksTitleLabelCsLabelEn(), SaveBooksTitleLabelCsLabelEn);
            base.UpdateDatabase(GetBooksNamesByLangOfOrigin(), SaveBooksNamesByLangOfOrigin);
            base.UpdateDatabase(GetBooksNamesByAuthorCountryLang(), SaveBooksNamesByAuthorCountryLang);
            base.UpdateDatabase(GetBooksLabelsAll(),SaveBookTitleWithNoOtherName);
            base.UpdateDatabase(GetBooksIdentifiers(),SaveBooksIdentifiers);
        }
        public override void UpdateAuthors(){

        }

        IEnumerable<string> GetBooksUri()
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

        void SaveBooksUri(string line, BookRecommenderContext db)
        {
            if (db.Books.Where(b => b.Uri == line).Count() > 0)
            {
                return;
            }
            db.Books.Add(new Book
            {
                Uri = line
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------
        IEnumerable<(string uri, string title, string labelCs, string labelEn)> GetBooksTitleLabelCsLabelEn()
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
        void SaveBooksTitleLabelCsLabelEn((string uri, string title, string labelCs, string labelEn) line, BookRecommenderContext db)
        {

            var book = db.Books.Where(b => b.Uri == line.uri)?.FirstOrDefault();
            if (book == null)
            {
                System.Console.WriteLine("book not in database: " + line.uri);
                return;
            }
            book.Title = line.title;
            book.NameCs = line.labelCs;
            book.NameEn = line.labelEn;

            db.Books.Update(book);
        }
        //------------------------------------------------------------------------------------------------------------------------------------
        IEnumerable<(string uri, string langCode, string label)> GetBooksNamesByLangOfOrigin()
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
        void SaveBooksNamesByLangOfOrigin((string uri, string langCode, string label) line, BookRecommenderContext db)
        {
            var book = db.Books.Where(b => b.Uri == line.uri)?.FirstOrDefault();
            if (book == null)
            {
                System.Console.WriteLine("book not in database: " + line.uri);
                return;
            }
            book.OrigLang = line.langCode;
            book.NameOrig = line.label;
            db.Books.Update(book);
        }
        //------------------------------------------------------------------------------------------------------------------------------------        
        IEnumerable<(string uri, string countryLangCode, string label)> GetBooksNamesByAuthorCountryLang()
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
        void SaveBooksNamesByAuthorCountryLang((string uri, string countryLangCode, string label) line, BookRecommenderContext db)
        {
            var book = db.Books.Where(b => b.Uri == line.uri)?.FirstOrDefault();
            if (book == null)
            {
                System.Console.WriteLine("book not in database: " + line.uri);
                return;
            }
            book.OrigLang = line.countryLangCode;
            book.NameOrig = line.label;
            db.Books.Update(book);
        }
        //------------------------------------------------------------------------------------------------------------------------------------
        IEnumerable<(string uri, string label)> GetBooksLabelsAll()
        {
            var query = @"SELECT ?item ?label
                            WHERE {
                                ?item wdt:P31 wd:Q571.
                                ?item rdfs:label ?label.
                            }";
            var result = Execute(query);
            foreach (var line in result)
            {
                yield return (line["item"], line["label"]);
            }
            yield break;
        }

        bool BookHasNoName(Book b)
        {
            return (string.IsNullOrEmpty(b.Title) &&
                    string.IsNullOrEmpty(b.NameOrig) &&
                    string.IsNullOrEmpty(b.NameCs) &&
                    string.IsNullOrEmpty(b.NameEn));
        }
        void SaveBookTitleWithNoOtherName((string uri, string label) line, BookRecommenderContext db)
        {
            var book = db.Books.Where(b => b.Uri == line.uri)?.FirstOrDefault();
            if (book == null)
            {
                System.Console.WriteLine("book not in database: " + line.uri);
                return;
            }
            if (BookHasNoName(book))
            {
                book.Title = line.label;
                db.Books.Update(book);
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------ 

        IEnumerable<(string uri, string isbn10, string isbn13, string gndId, string openLibId, string freeBase)> GetBooksIdentifiers()
        {
            var query = @"SELECT ?item ?ISBN10 ?ISBN13 ?GND_id ?open_lib_id ?free_base
                            WHERE {
                                ?item wdt:P31 wd:Q571.
                                OPTIONAL{
                                    ?item wdt:P957 ?ISBN10.
                                }
                                OPTIONAL{
                                    ?item wdt:P212  ?ISBN13.
                                }
                                OPTIONAL{
                                    ?item wdt:P227 ?GND_id.
                                }
                                OPTIONAL{
                                    ?item wdt:P648 ?open_lib_id.
                                }
                                OPTIONAL{
                                    ?item wdt:P646 ?free_base.
                                }
                            }";
            var result = Execute(query);
            foreach (var line in result)
            {
                yield return (line["item"], line["ISBN10"], line["ISBN13"], line["GND_id"], line["open_lib_id"], line["free_base"]);
            }
            yield break;
        }
        void SaveBooksIdentifiers((string uri, string isbn10, string isbn13, string gndId, string openLibId, string freeBase) line, BookRecommenderContext db)
        {
            var book = db.Books.Where(b => b.Uri == line.uri)?.FirstOrDefault();
            if (book == null)
            {
                System.Console.WriteLine("book not in database: " + line.uri);
                return;
            }

            book.ISBN10 = line.isbn10;
            book.ISBN13 = line.isbn13;
            book.GndId = line.gndId;
            book.OpenLibId = line.openLibId;
            book.FreeBase = line.freeBase;
            db.Books.Update(book);

        }
        //------------------------------------------------------------------------------------------------------------------------------------      
    }
}