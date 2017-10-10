using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Dolores.DataClasses;

namespace Dolores.CustomAttributes
{
    [System.AttributeUsage(System.AttributeTargets.Method |
                           System.AttributeTargets.Class, AllowMultiple = true)]  // multiuse attribute
    public class LangSummaryAttribute : Attribute
    {
        public LangSummaryAttribute(LanguageDictionary.Language lang, string summary)
        {
            Lang = lang;
            Summary = summary;
        }

        public LanguageDictionary.Language Lang { get; }
        public string Summary { get; }
    }
}
