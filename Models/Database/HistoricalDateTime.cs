
using System;
using System.Globalization;

namespace BookRecommender.Models
{
    public enum Era
    {
        BC, AD
    };
    public class HistoricalDateTime
    {
        DateTime date;
        Era era;

        HistoricalDateTime(DateTime date, Era era)
        {
            this.date = date;
            this.era = era;
        }

        static HistoricalDateTime FromString(string s, string format)
        {
             if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            Era era;
            if (s[0] == '-')
            {
                era = Era.BC;
            }
            else
            {
                era = Era.AD;
                // get rid of the minus
                s = s.Substring(1);
            }


            DateTime dateTime;
            var successfull = DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

            if (!successfull)
            {
                return null;
            }

            return new HistoricalDateTime(dateTime, era);
        }
        public static HistoricalDateTime FromWikiData(string dateSparql)
        {

            var format = "yyyy-MM-ddTHH:mm:ssZ";
            return FromString(dateSparql, format);

        }
        public static HistoricalDateTime FromDatabase(string date){
            var format = "yyyy-MM-dd";
            return FromString(date,format);
        }


        public string ToDatabaseString(){
            var format = "yyyy-MM-dd";
            var str = date.ToString(format);
            if(era == Era.BC){
                str = "-" + str;
            }
            return str;
        }

    }

}