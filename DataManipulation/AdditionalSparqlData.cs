using System.Collections.Generic;

namespace BookRecommender.DataManipulation
{
    public class AdditionalSparqlData
    {
        public AdditionalSparqlData(List<(string text, string lang)> labels,
                                    List<(string text, string lang)> descriptions,
                                    List<(string propertyUrl, string propValue, string propLabel, string propValueLabel, string propDescription)> properties,
                                    string dateModified)
        {
            Labels = labels;
            Descriptions = descriptions;
            Properties = properties;
            DateModified = dateModified;
        }
        public List<(string text, string lang)> Labels { get; private set; }
        public List<(string text, string lang)> Descriptions { get; private set; }
        public List<(string propertyUrl, string propValue, string propLabel, string propValueLabel, string propDescription)> Properties { get; private set; }
        public string DateModified { get; private set; }
    }
}