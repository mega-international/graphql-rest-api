namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class FormatsEnumerationGraphType : HopexEnumerationGraphType
    {
        public FormatsEnumerationGraphType()
        {
            Name = "Formats";
            AddValue("Internal",
                "The returned value is expressed in its native mode (for example a boolean value is returned in a boolean variable).",
                "Internal");
            AddValue("ASCII",
                "The internal value is converted in an ASCII format according to the MEGA standard.",
                "ASCII");
            AddValue("External",
                "This value is generally the ASCII value but in the case of a MetaAttribute with defined values the external value is the translation expressed in the current language.",
                "External");
            AddValue("Display",
                "This value might be needed for display in HTML pages or word report. In certain cases, identifiers are suppressed in order to display a user friendly name.",
                "Display");
            AddValue("Object",
                "Use this value to transfer an RTF text to a property of another object. Using this format, the property is not converted to an ASCII format but kept in its native form. If you use the \"ASCII\" format with a RTF property, MEGA returns the RTF text with \\ sequences, but if you affect the returned value to another object, this value is given as an ASCII text. This format fixes the problem.",
                "Object");
            AddValue("Physical",
                "Return the property using the format stored in the repository. For example, an UML attribute has a type but the property \"Expression type\" is stored physically with %s wildcards. The external format replaces that wildcards by the class linked.",
                "Physical");
        }
    }
}
