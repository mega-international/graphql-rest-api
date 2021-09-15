package com.mega.metamodelToJSON;

import org.apache.commons.text.WordUtils;

import com.ibm.icu.text.Transliterator; 

public class UtilitiesMappingJSON {

	public static String getGraphQLType(String oMetaAttributeType) {
		String graphQLType = "";
		
		switch (oMetaAttributeType) {  
			case "W": //' DateTime64  
			graphQLType = "Date";
			break;   		
			case "X": // String  
			graphQLType = "String";
			break;  
			case "1": // ' Boolean  
			graphQLType = "Boolean";
			break;  
			case "S": // ' Short  
			graphQLType = "Int";
			break;  
			case "L": // ' Long
			graphQLType = "Long";
			break;  
			case "D": //' DateTime 
			graphQLType = "Date";
			break;  
			case "A": // ' VarChar  
			graphQLType = "String";
			break;  
			case "B": //' VarBinary  
			graphQLType = "Binary";
			break;  
			case "Q":  // Binary
			graphQLType = "Binary";
			break;  
			case "H": // ' MegaIdentifier 
			graphQLType = "Id";
			break;  
			case "F": // ' Float 
			graphQLType = "Float";
			break;  
			case "C": //' Currency  
			graphQLType = "Currency"; // To check
			break; 			
			case "U": //' AbsoluteDateTime64  
			graphQLType = "Date";
			break;   			
			default:  
			graphQLType = "String";
		}  
		
		return graphQLType;
	}
	

	public static String getTechnicalNameMetaClass(String technincalName) {
		return getTechnicalName(technincalName);
	}
	
	public static String getTechnicalNameMetaAttribute(String technincalName) {
		return getTechnicalName(technincalName);
	}

	public static String getTechnicalNameMetaAssociationEnd(String technincalName) {
		return getTechnicalName(technincalName);
	}	

	public static String getTechnicalNameMetaAssociation(String technincalName) {
		return getTechnicalName(technincalName);
	}		
	
	public static String getCleanComment(String comment) {
		Transliterator accentsconverter = Transliterator.getInstance("NFD; [:M:] Remove; NFC; ");
		comment =  accentsconverter.transliterate(comment);	
		
		comment = comment.replaceAll("\\\\", " ");
		comment = comment.replaceAll("\\/", " ");
		comment = comment.replaceAll("'", " ");
		comment = comment.replaceAll("\"", " ");		
		comment = comment.replaceAll("[\r\n]+", "");
		comment = comment.replaceAll("·", " ");		

		return comment;
	}

	public static String getTechnicalNameMetaAttributeValue(String comment) {
		
		comment = WordUtils.capitalize(comment);
		
		comment = comment.replaceAll(">", "Above");
		comment = comment.replaceAll("<", "Lower");
		comment = comment.replaceAll("0..1", "ZeroToOne");
		comment = comment.replaceAll("1\\.\\.\\*", "OneToMany");
		comment = comment.replaceAll("£", "PoundSterling");
		comment = comment.replaceAll("\\*", "Many");
		comment = comment.replaceAll("¥", "Yen");
		comment = comment.replaceAll("\\$", "USD");
		comment = comment.replaceAll("€", "Euro");
		comment = comment.replaceAll("\\(", "");
		comment = comment.replaceAll("\\)", "");
		comment = comment.replaceAll(" ", "");
		comment = comment.replaceAll("\\\\", "");
		comment = comment.replaceAll("\\/", "");
		comment = comment.replaceAll("'", "");
		comment = comment.replaceAll("\"", "");		
		comment = comment.replaceAll("\\.", "_");	
		comment = comment.replaceAll("[\r\n]+", "");	
		comment = comment.replaceAll(":", "_");	
		comment = comment.replaceAll("%", "_");	
		comment = comment.replaceAll("!", "_");	
		comment = comment.replaceAll(",", "_");	
		comment = comment.replaceAll(";", "_");	
		comment = comment.replaceAll("-", "_");
		comment = comment.replaceAll("\\\\b0\\\\b", "Zero");
		comment = comment.replaceAll("\\\\b1\\\\b", "One");
		comment = comment.replaceAll("0", "Zero");
		comment = comment.replaceAll("1", "One");
		comment = comment.replaceAll("2", "Two");
		comment = comment.replaceAll("3", "Three");
		comment = comment.replaceAll("4", "Four");
		comment = comment.replaceAll("5", "Five");
		comment = comment.replaceAll("6", "Six");
		comment = comment.replaceAll("7", "Seven");
		comment = comment.replaceAll("8", "Eight");
		comment = comment.replaceAll("9", "Nine");	

		Transliterator accentsconverter = Transliterator.getInstance("NFD; [:M:] Remove; NFC; ");
		comment =  accentsconverter.transliterate(comment);			
		
		return comment;
	}

	
	private static String getTechnicalName(String technincalName) {
		technincalName = WordUtils.capitalize(technincalName);
		Transliterator accentsconverter = Transliterator.getInstance("NFD; [:M:] Remove; NFC; ");
		technincalName =  accentsconverter.transliterate(technincalName);			
		technincalName =technincalName.replaceAll("\\P{Alnum}", "");	
		
		return technincalName;

	}
	
	
	
	
}
