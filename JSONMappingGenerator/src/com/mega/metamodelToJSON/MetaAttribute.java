package com.mega.metamodelToJSON;


import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.mega.mappingJSON.ConstraintsProperties;
import com.mega.mappingJSON.EnumValuesJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAttribute  extends CommonAttributes  {

	private PropertiesJSON propertiesJSON;
	private ConstraintsProperties constraintsProperties;

	private MegaObject oMetaAttribute;	
	private String absoluteIdentifier;
	
	private boolean isValidMetaAttribute = true;
	
	public MetaAttribute(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject pMetaAttribute, HashMap<String,String> overrideNameList) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.overrideNameList = overrideNameList;
		this.oMetaAttribute = megaRoot.getCollection(StaticFields.metaAttribute).get(pMetaAttribute.megaField());
		this.constraintsProperties = new ConstraintsProperties();

		
		String technincalName = "";
		if (oMetaAttribute.exists()) {		
			boolean isAvailable = (boolean) oMetaAttribute.callFunction("IsAvailable");

			if (isAvailable) {
				technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAttribute(oMetaAttribute.getProp(StaticFields.shortName));
				setProperties();
			} else {
				isValidMetaAttribute = false;		
			}
			if (!includedAttribute(technincalName) && isValidMetaAttribute) {
				isValidMetaAttribute = false;			
			}			
		} else {
			isValidMetaAttribute = false;
		}
		
		
		
	}		

	public MetaAttribute(String technincalName, String absoluteIdentifier, String sDescription, String maxLength, String type, boolean isMetaAttributeFilter, boolean isMandatory, boolean isReadOnly, boolean translatable, boolean formattedText,HashMap<String,String> overrideNameList) {
		this.propertiesJSON = new PropertiesJSON(overrideNameList,absoluteIdentifier);
		this.constraintsProperties = new ConstraintsProperties();
		this.overrideNameList = overrideNameList;
		isValidMetaAttribute = true;
		
		propertiesJSON.setName(technincalName);
		propertiesJSON.setId(absoluteIdentifier);
		propertiesJSON.setDescription(sDescription);

        constraintsProperties.setMaxLength(maxLength);
        constraintsProperties.setType(type);
        constraintsProperties.setMandatory(isMandatory);
        constraintsProperties.setReadOnly(isReadOnly);
        constraintsProperties.setTranslatable(translatable);
        constraintsProperties.setFormattedText(formattedText);
        
        propertiesJSON.setConstraints(constraintsProperties);
		
		
		
	}	
	
	public String getName() {
		return this.name;
	}
	
	public String getNameErrorDisplay() {
		return this.nameErrorDisplay;
	}
	
	public String getAbsoluteIdentifier() {
		return this.absoluteIdentifier;
	}
	
	public PropertiesJSON getPropertiesJSON() {
		return propertiesJSON;
	}
	
	public boolean isValideMetaAttribute() {
		return this.isValidMetaAttribute;
	}
	
	private void setProperties() {		
		
		String technincalName = oMetaAttribute.getProp(StaticFields.shortName);
		technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAttribute(technincalName);
		absoluteIdentifier = oMetaAttribute.getProp(StaticFields.absoluteIdentifier);
		String sDescription = UtilitiesMappingJSON.getCleanComment(oMetaAttribute.getProp(StaticFields.comment,"display").toString());
		String maxLength = oMetaAttribute.getProp(StaticFields.metaAttributeLength);
		
        String oMetaAttributeType = oMetaAttribute.getProp(StaticFields.metaAttributeType,"internal").toString();
        String type = UtilitiesMappingJSON.getGraphQLType(oMetaAttributeType);

		this.propertiesJSON = new PropertiesJSON(overrideNameList, absoluteIdentifier);

		technincalName = customNaming(technincalName, type );
		
		propertiesJSON.setName(technincalName);
		propertiesJSON.setId(absoluteIdentifier);
		propertiesJSON.setDescription(sDescription);
		
        // we ignore var binary and binary type
        if (oMetaAttributeType.contentEquals("B") || oMetaAttributeType.contentEquals("Q")  ) {
        	isValidMetaAttribute = false;
        }
        
        // if the metaattribute is a varchar the lenght doesn't matter.
        if (oMetaAttributeType.contentEquals("A")) {
            //constraintsProperties.setMaxLength("0");
            
        } else if  (oMetaAttributeType.contentEquals("H"))   {
            constraintsProperties.setMaxLength("16");
        } else {
            constraintsProperties.setMaxLength(maxLength);
        }
        	
        constraintsProperties.setType(type);
       // constraintsProperties.setFilter(isMetaAttributeFilter());
        //constraintsProperties.setFilter(true);
        
        constraintsProperties.setMandatory(isMandatory());
        constraintsProperties.setReadOnly(isReadOnly(technincalName));
		constraintsProperties.setTranslatable(isTranslatable());
		constraintsProperties.setFormattedText(isFormattedText(oMetaAttributeType));
		
        propertiesJSON.setConstraints(constraintsProperties);

        if (isEnum()) {
        	propertiesJSON.setEnumValues(getMetaAttributeValues());
        }

        // managing duplicate error
		name = technincalName.toString();       
        nameErrorDisplay =  name + " - " + absoluteIdentifier;       
	}
	
	
	private String customNaming(String technincalName, String type ) {
		String localName = technincalName;
				
		if (type.contentEquals("Id") || type.contentEquals("id") ) {

			if (localName.contentEquals("AbsoluteIdentifier")  || localName.contentEquals("absoluteIdentifier") ) {
				localName = "id";
//			} else if (localName.contentEquals("Creator") || localName.contentEquals("creator")) {
//				localName = "creatorId";
//				
//			} else if (localName.contentEquals("LinkCreator") || localName.contentEquals("linkCreator")) {
//				localName = "linkCreatorId";
				
//			} else if (localName.contentEquals("Modifier") || localName.contentEquals("modifier")) {
//				localName = "modifierId";
//				
//			} else if (localName.contentEquals("LinkModifier") || localName.contentEquals("linkModifier")) {
//				localName = "linkModifierId";				
			} else {
				localName = localName + "Id";
			}
			
		} else {
			
			
			if (localName.contentEquals("ExternalIdentifier") || localName.contentEquals("externalIdentifier")) {
				localName = "externalId";	
			}
			
			
		}
		return localName;
	}
	
	private List<EnumValuesJSON> getMetaAttributeValues() {
		List<EnumValuesJSON> enumValuesJSONList = new ArrayList<EnumValuesJSON>();
	
		MegaCollection megaCollection = oMetaAttribute.getCollection(StaticFields.metaAttributeValue);

		while (megaCollection.hasNext()) {
			MegaObject oMetaAttributeValue = megaCollection.next();			
			MetaAttributeValue metaAttributeValue = new MetaAttributeValue(megaRoot, oMetamodel,oMetaAttribute,oMetaAttributeValue, overrideNameList);
			if (metaAttributeValue.isValideMetaAttributeValue()) {
				enumValuesJSONList.add(metaAttributeValue.getEnumValuesJSON());
			}
		} //while

		return enumValuesJSONList;
	}
	
	private boolean includedAttribute(String technincalName) {
		// we remove certains attributes that are either too technical or redundant
		boolean include = true;
		
		String technicalLevel = oMetaAttribute.getProp(StaticFields.technicalLevel);

		// to CHECK to impact of this removel
		if(technicalLevel.compareTo("K") == 0) {
			include = false;
			//System.out.println("WARNING --- " + oMetaAttribute.getProp(StaticFields.shortName) + " is a Sytem MetaAttribute");
		} 
		
		String name = oMetaAttribute.getProp(StaticFields.shortName);
	
		if(name.compareTo("Short Name") == 0) {
			include = false;
		}

		//System.out.println("--- " + include + " --- name=" + name + " --- technicalLevel === " + technicalLevel);	
		
		
		if (technincalName.contentEquals("LinkModifierName")  || technincalName.contentEquals("linkModifierName") ) {
			include = true;
		} else if (technincalName.contentEquals("LinkCreatorName") || technincalName.contentEquals("linkCreatorName")) {
			include = true;	
		} else if (technincalName.contentEquals("CreatorName") || technincalName.contentEquals("creatorName")) {
			include = true;	
		} else if (technincalName.contentEquals("ModifierName") || technincalName.contentEquals("modifierName")) {
			include = true;	
		} else if (technincalName.contentEquals("IsWritable") || technincalName.contentEquals("isWritable")) {
			include = true;	
		}				
			
		return include;
	}

	private boolean isFormattedText(String oMetaAttributeType) {
		boolean formattedText = false;
		
		//Varchar
		if (oMetaAttributeType.contentEquals("A")) {
			formattedText = true;
		
			String defaultMetaTextFormat = oMetaAttribute.getProp(StaticFields.defaultMetaTextFormat);
			switch (defaultMetaTextFormat) {  
			case "11D8120F3BEC0026":  // CSV 
				formattedText = false;
			break;   			
			case "20A720DC3BAA0004":  // FIELD 
				formattedText = false;
			break; 
			case "3AC340245BA2753E":  // UTF8 
				formattedText = false;
			break; 
			case "14A216B744880066":  // CONDITION 
				formattedText = false;
			break; 			
			default:  
				//
			} 

		}

		//MegaObject commentMetaAttribute = megaRoot.getObjectFromID("~f10000000b20[Comment]");
		//if (oMetaAttribute.sameID(commentMetaAttribute.getID())) {
		//	formattedText = true;			
		//}

		return formattedText;
	}

	private boolean isTranslatable() {
		boolean translatable = false;
		String translability = oMetaAttribute.getProp(StaticFields.translability);
		switch (translability) {  
		case "T":   
			translatable = true;
		break;   		
		case "U":   
			translatable = false;
		break;	
		default:  
			translatable = false;
		}  		
		return translatable;
	}	
	
	private boolean isReadOnly(String technincalName) {
		boolean readOnly = false;		
        int countMacro =  oMetaAttribute.getCollection(StaticFields.macro).size();
        int updatePermission = Integer.parseInt(oMetaAttribute.getProp(StaticFields.updatePermission));
        
        if (countMacro > 0) {
        	readOnly = true;
        }
        
        
        // if 0 then read only
        // if 1 then can be updated
        if (updatePermission < 1) {
        	readOnly = true;
        } else {
        	readOnly = false;
        }
 
        
        
		
		if (technincalName.contentEquals("ModificationDate")  || technincalName.contentEquals("modificationDate") ) {
			readOnly = true;
		} else if (technincalName.contentEquals("ModifierId") || technincalName.contentEquals("modifierId")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("Modifier") || technincalName.contentEquals("modifier")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModificationDate") || technincalName.contentEquals("linkModificationDate")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModifier") || technincalName.contentEquals("linkModifier")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("LinkModifierId") || technincalName.contentEquals("linkModifierId")) {
			readOnly = true;	
		} else if (technincalName.contentEquals("IsWritable") || technincalName.contentEquals("isWritable")) {
			readOnly = true;	
		}						
        
        //System.out.println("isReadOnly " + readOnly + " - " + oMetaAttribute.getProp("Name"));
        
		return readOnly;
	}
	
	private boolean isMandatory() {
		boolean mandatory = false;

		if (oMetaAttribute.sameID(StaticFields.shortName)) {
			mandatory = true;
		}		
		
		return mandatory;
	}
/*	
	private boolean isMetaAttributeFilter() {
		boolean isfilter = false;
		
		if (isEnum()) { 
			isfilter = true;
		} else {
			isfilter = false;
		}

		if (oMetaAttribute.sameID(StaticFields.shortName)) {
			isfilter = true;
		}
		
		return isfilter; 
	}
*/	
	private boolean isEnum() {
		boolean isEnum = false;
		
		 String sFormat=  oMetaAttribute.getProp(StaticFields.metaAttributeFormat,"internal").toString();
			switch (sFormat) {  
			case "F":   
				isEnum = true;
			break;   		
			case "T":   
				isEnum = true;
			break;	
			default:  
				isEnum = false;
		}  		
		
		return isEnum;
	}
	
	public static Map<String,String> manualAttribute(boolean hasNameSpace, List<PropertiesJSON> propertiesJSONList, HashMap<String,String>  overrideNameList) {
		
		HashMap<String,String> metaAttributeHashMap = new HashMap<String,String>();

		if (hasNameSpace) {
			String technincalName5 = "Name";
			String absoluteIdentifier5 = "Z20000000D60"; 
			String sDescription5 = "Short Name of the object (posibility to get the long name with an argument)";
			String maxLength5 = "1024";
			String type5 = "String";
			boolean isMetaAttributeFilter5 = true; 
			boolean isMandatory5 = true; 
			boolean isReadOnly5 = false;			
			boolean translatable5 = true; 
			boolean formattedText5 = false;		
			MetaAttribute metaAttribute5 = new MetaAttribute(technincalName5, absoluteIdentifier5, sDescription5, maxLength5, type5, isMetaAttributeFilter5, isMandatory5, isReadOnly5, translatable5,formattedText5,overrideNameList);
			PropertiesJSON propertiesJSON5 = metaAttribute5.getPropertiesJSON();
			propertiesJSONList.add(propertiesJSON5);				
			metaAttributeHashMap.put("ShortName","ShortName - Z20000000D60");
	
		} else {
				
		}
	
		return metaAttributeHashMap;
	}	

	public static MegaCollection cleanCollectionMetaAttributeAssociation(MegaRoot megaRoot, MegaCollection megaCollection) {
		
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaCollection);
		 oCol.remove(megaRoot.getObjectFromID("~tXERBv6k5r60[AssociativeObject]"));	   
		 oCol.remove(megaRoot.getObjectFromID("~C3cm9FyluS20[Link Comment]"));			
		   
		return oCol;
	}
	
	
	public static MegaCollection cleanCollectionMetaAttribute(MegaRoot megaRoot, MegaCollection megaCollection) {
		
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaCollection);
		 oCol.remove(megaRoot.getObjectFromID("~M30000000P90[Is Writable]"));
		 oCol.remove(megaRoot.getObjectFromID("~Z20000000D60[Short Name]"));
		 oCol.remove(megaRoot.getObjectFromID("~CFmhlMxNT1iE[External Identifier]"));
		 oCol.remove(megaRoot.getObjectFromID("~210000000900[Name]"));
		 oCol.remove(megaRoot.getObjectFromID("~f10000000b20[Comment]"));
		 oCol.remove(megaRoot.getObjectFromID("~H20000000550[_HexaIdAbs]"));
		   oCol.remove(megaRoot.getObjectFromID("~310000000D00[Absolute Identifier]"));
		   oCol.remove(megaRoot.getObjectFromID("~)10000000z30[Creator Name]"));
		   oCol.remove(megaRoot.getObjectFromID("~s20000000P70[Importance]"));
		   oCol.remove(megaRoot.getObjectFromID("~c10000000P20[Modifier Name]"));
		    oCol.remove(megaRoot.getObjectFromID("~510000000L00[Creation Date]"));
		   oCol.remove(megaRoot.getObjectFromID("~610000000P00[Modification Date]"));
		   oCol.remove(megaRoot.getObjectFromID("~(10000000v30[Creator]"));
		   oCol.remove(megaRoot.getObjectFromID("~b10000000L20[Modifier]"));
		   
		   oCol.remove(megaRoot.getObjectFromID("~g20000000f60[Generic Local name]"));
		   oCol.remove(megaRoot.getObjectFromID("~nOU8g8IMCb30[Converted Name Version]"));
		   oCol.remove(megaRoot.getObjectFromID("~f20000000b60[Log]"));
		   oCol.remove(megaRoot.getObjectFromID("~m20000000170[Comment Body]"));
		   oCol.remove(megaRoot.getObjectFromID("~d20000000T60[Object MetaClass IdAbs]"));
		   oCol.remove(megaRoot.getObjectFromID("~930000000b80[Confidential Object]"));
		   oCol.remove(megaRoot.getObjectFromID("~L30000000L90[Is Readable]"));
		   oCol.remove(megaRoot.getObjectFromID("~U30000000v90[Is UI Readable]"));
		   oCol.remove(megaRoot.getObjectFromID("~030000000180[Reading access area]"));
		   oCol.remove(megaRoot.getObjectFromID("~)20000000z70[Reading access area identifier]"));
		   oCol.remove(megaRoot.getObjectFromID("~rr0pynS5yC00[Object Validity]"));
		   oCol.remove(megaRoot.getObjectFromID("~eg6JrB6F1rH0[Object Validity Report]"));
		   oCol.remove(megaRoot.getObjectFromID("~V30000000z90[Status <Diagnostic>]"));
		   oCol.remove(megaRoot.getObjectFromID("~l20000000z60[Comment Header]"));
		   oCol.remove(megaRoot.getObjectFromID("~p20000000D70[Object MetaClass Name]"));
		   oCol.remove(megaRoot.getObjectFromID("~tbFegCw7AXA0[Picture Identifier]"));
		   oCol.remove(megaRoot.getObjectFromID("~rOcwckE07r00[Translation Requirement By Mega Doc]"));
		   oCol.remove(megaRoot.getObjectFromID("~dzKwedUCMXsR[_Report Edition Parameterization]"));
		   oCol.remove(megaRoot.getObjectFromID("~Hf9MIW8gHn5S[_TranslationSettings]"));

		   oCol.remove(megaRoot.getObjectFromID("~d10000000T20[Writing access area]"));
		   oCol.remove(megaRoot.getObjectFromID("~520000000L40[Create Version]"));
		   oCol.remove(megaRoot.getObjectFromID("~620000000P40[Update Version]"));
		   oCol.remove(megaRoot.getObjectFromID("~a20000000H60[Language Update Date]"));
		   oCol.remove(megaRoot.getObjectFromID("~e10000000X20[Writing access level]"));
		   oCol.remove(megaRoot.getObjectFromID("~Z10000000D20[Status]"));
		   oCol.remove(megaRoot.getObjectFromID("~c20000000P60[_DefineIdAbs]"));
		   oCol.remove(megaRoot.getObjectFromID("~z20000000r70[Object Availability]"));
		   oCol.remove(megaRoot.getObjectFromID("~y20000000n70[Object Origin IdAbs]"));
		   oCol.remove(megaRoot.getObjectFromID("~m20000000170[Comment Body]"));
		   oCol.remove(megaRoot.getObjectFromID("~3GYCQDP0BDN0[Owning Library]"));
		   oCol.remove(megaRoot.getObjectFromID("~OGN)aXoGH9LD[_System Current Workflow Status Information <Not Visible>]"));
		   oCol.remove(megaRoot.getObjectFromID("~xkUUefdeFbqE[Immutability]"));
		   oCol.remove(megaRoot.getObjectFromID("~1JubVrPLAfG0[Variable Object Picture Identifier]"));
		   oCol.remove(megaRoot.getObjectFromID("~sB7o8gyyBjI0[Validation State]"));
		   //oCol.remove(megaRoot.getObjectFromID("~TUOYtiORGbG1[Current Validation]"));
		   //oCol.remove(megaRoot.getObjectFromID("~w9KvON(3FHDP[Current Workflow Status]"));
		   oCol.remove(megaRoot.getObjectFromID("~RGzwku(yBrV1[Previous Validation State  <Deprecated>]"));
		  // oCol.remove(megaRoot.getObjectFromID("~(YByTkohHrGG[Current State]"));
		   oCol.remove(megaRoot.getObjectFromID("~lxfuKBkYk400[MMI]"));
		   oCol.remove(megaRoot.getObjectFromID("~7cwMFl57pe00[Purpose]"));
		   oCol.remove(megaRoot.getObjectFromID("~CoK3h1oc8Di0[GenericLabel]"));
		   oCol.remove(megaRoot.getObjectFromID("~IoxIXG5yQ17N[Criticality <DoDAF>]"));
		   oCol.remove(megaRoot.getObjectFromID("~N10000000T10[Permission]"));
		 //  oCol.remove(megaRoot.getObjectFromID("~Q10000000f10[MetaAttributeType]"));
		 //  oCol.remove(megaRoot.getObjectFromID("~R10000000j10[MetaAttributeLength]"));
		 //  oCol.remove(megaRoot.getObjectFromID("~S10000000n10[MetaAttributeFormat]"));		
		//   oCol.remove(megaRoot.getObjectFromID("~T10000000r10[Abbreviation]"));		   		   
		//   oCol.remove(megaRoot.getObjectFromID("~z10000000r30[TechnicalLevel]"));
		//   oCol.remove(megaRoot.getObjectFromID("~o20000000970[DefaultInternalValue]"));
		//   oCol.remove(megaRoot.getObjectFromID("~t20000000T70[ExtendedProperties]"));
		 //  oCol.remove(megaRoot.getObjectFromID("~v20000000b70[DefaultMetaTextFormat]"));
		 //  oCol.remove(megaRoot.getObjectFromID("~kGZC79p5u000[RefersText]"));
		   oCol.remove(megaRoot.getObjectFromID("~Dv(CwL3ZF1f4[CollectionPermission]"));

		   oCol.remove(megaRoot.getObjectFromID("~EMeJuBbAGfQJ[IllustratedPictureIdentifier]"));
		   oCol.remove(megaRoot.getObjectFromID("~eIXaJCfyhuJ0[Parameterization]"));
		   oCol.remove(megaRoot.getObjectFromID("~B6rudNEDqC20[Capability]"));
		   oCol.remove(megaRoot.getObjectFromID("~z10000000r30[TechnicalLevel]"));

		   oCol.remove(megaRoot.getObjectFromID("~aXWJUhq2DHe2[GRCsortkey]"));
		//   oCol.remove(megaRoot.getObjectFromID("~KZjN4(b9FDnC[GRCOnline]"));
		   oCol.remove(megaRoot.getObjectFromID("~gTOjdYxEAXa0[GRCCode]"));		   
		   oCol.remove(megaRoot.getObjectFromID("~JVOjoZxEAPc0[GRCGUID]"));
		   oCol.remove(megaRoot.getObjectFromID("~O9Wuh9EHAb90[GRCUserName]"));
		   oCol.remove(megaRoot.getObjectFromID("~eAWuo9EHA9E0[GRCUserPassword]"));
		   oCol.remove(megaRoot.getObjectFromID("~g10000000f20[PassWord]"));

		   oCol.remove(megaRoot.getObjectFromID("~faJBOms0EDxE[TimePeriodcolorwithfixeddate]"));
		   oCol.remove(megaRoot.getObjectFromID("~naJBWHr0Ev8E[TimePeriodcolorwithnonfixeddate]"));
		   
		   
		   
		   // List of metaAttribute of System level
		   oCol.remove(megaRoot.getObjectFromID("~gl5Hs6hkPnNL[] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~2qdSxJfqOnXF[Business Drivers How do you consume data from your application] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bl5HFcgkPnwJ[Business Drivers What is the application scability pattern] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xsdSWKfqOzgF[Business Drivers What is the current expected level of current SLA] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~(l5H)jgkP1jK[Business Drivers What is your criterion for application performance] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~qtdScKfqO9kF[Process Maturity What is the average skill on Cloud technologies and practices within your development team] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UtdS3LfqOvwF[Process Maturity What is the level of deployment process automation for provisioning & configuration] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~NrdSLLfqOH1G[Process Maturity What is your evolution model and feedback loop implementation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~6rdSHKfqObaF[Technical Enablers How the application is exposed to external services/apps] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XsdSvKfqOjtF[Technical Enablers Is this application multi-tenant] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~frdSqKfqOXqF[Technical Enablers What are the relationships beyond the application boundaries] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~lqdSjKfqOLnF[Technical Enablers What is the application database provider] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~TqdSFLfqO5(F[Technical Enablers What is the current deployment platform] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~1sdSPKfqOndF[Technical Enablers What is the current user authentication mechanism] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~i10000000n20[_AuthorizationSet] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~hpmU8xtmuC20[_CODE Directory History] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~2fn3P4Hfkm10[_DatabaseCfg] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xqmN34sEl400[_DatabaseReport] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tBP8f721r400[_DrawingTextFormat] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~iKdnITSLxW40[_DrawingXmlFormat] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~hGOgRgwcIn8G[_FavoritesContacts] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bPjcN3wNyCE0[_GenNewHelpPage] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RIvPdVs3iS10[_Ident] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Y10000000920[_idOcc] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~18P8g721re00[_idRel] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~hWpWSK8kArC0[_Mega  In Help] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~m10000000130[_mtType] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~3zHcgbf0Cf30[_NextStatusInstanceInformation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~k10000000v20[_Ordinal] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~iXZG)R4EIbiM[_ParameterizationOfAlertsScheduling] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~pCAtrcUGHDO0[_PersonsAssignedForTransition] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~dzKwedUCMXsR[_Report Edition Parameterization] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~420000000H40[_ScanInit] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~OGN)aXoGH9LD[_System Current Workflow Status Information <Not Visible>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~l10000000z20[_Temporary] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Hf9MIW8gHn5S[_TranslationSettings] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GXWWPTuQyy30[_TypeInformation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~G20000000150[_UserAccessProfile] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~BAHujIEBAL83[Action Type] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~dswHvWaHFL2U[Active Workflow Transition Instance] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~wjhzOAGeFD0F[Agent Component Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GP5qz7DbAv70[All Users] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ahaolqy7Mz1G[Analytics Information Deprecated] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~rsC4041SHfCF[Answer Comment <Deutsch>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XjAsBO(RHLCQ[Answer Comment <English>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~4tC4041SHbDF[Answer Comment <Español>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~1lAs1N(RH17Q[Answer Comment <Français>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~CqC4141SHzHF[Answer Comment <Italiano>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5EIuEYlVF1kJ[Answer to the security question] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~wI7SzddDADT3[Application Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~l7Z9LXI1RD5Q[ArchiMate - Active DiagramTypeViews] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Sgi1tSmUQvv6[ArchiMate - Classifiying Attribute] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XVsu6FETD1UJ[Architecture Use Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~adUnEhSd2vR0[Arity] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~KrmvAg0ZA5V1[Artifact Component Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~el42vfm(DHfD[AsIs Milestone] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~TOuRw)6pLXZP[Assessment Start Mode] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~O30000000X90[Assignment End Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~N30000000T90[Assignment Start Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5OSkJuBE)u30[Association Kind] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bx7yd2FEAv50[Attach objects] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~j0wnbbCb6vB0[Automatic Object] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~mAH8L6IX0900[Available products] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~4If5zQniyW10[BackupedObjetIdAbs] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~KtNn6eiOHvG0[BaselineMaxDate] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~jlfUkt7AIPQJ[Binary File] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GBHmVECX65D0[BPMX Technical Information] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~HYn(RplEIbl8[ChangeItemProcess Date Index] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bX3edYUo51F0[ChangeItemVisibility] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bpn)a(Fyt000[Column mapping] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~CzQ4jCnNFf4L[Comment Displaying] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~JTkDRd5cziY0[COMNAME_GBMCOMNATURE] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~)SkDJd5czyX0[COMNAME_GBMNATURE] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~jTkDfd5czSZ0[COMNAME_IDOP] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8i00A30rxCI0[Connector Direction] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~vCIujtkVFTqH[Connexion status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Bo8z26XBHTgK[Context Order] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~1xhMyINwBL40[Contribution Triggering Done] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~nOU8g8IMCb30[Converted Name Version] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~YIX2JlGzM9L2[Corresponding Exchange] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~MtdSWGfqOvyE[Could failure of this application lead to disruption? Please define the level of impact.] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~DrdSmGfqOH3F[Could failure of this application lead to harm of the company's public image? Please define the level of impact.] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~7sdStGfqOT6F[Could failure of this application lead to loss of customer confidence? Please define the level of impact.] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~IqdSeGfqO50F[Could failure of this application lead to loss of revenue or business opportunity? Please define the level of impact.] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~k3f6dp10)4G0[Create Permission <Read-Only Session>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~G0f6ek10)G60[Create Permission <User Interface> <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~PWLh8NBfS5ZP[Created from ITPM] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Hlqo7FD2CrC0[Creation DateTime  Workflow Status Instance] ")); 
		   //oCol.remove(megaRoot.getObjectFromID("~)10000000z30[Creator Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~TUOYtiORGbG1[Current Validation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~s10000000P30[Data access] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Us4lhDa8NbGP[DataBase Id] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Dr4lBJa8NTLP[DataBase Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~AGcxyx0QMTRE[Database Synchronization Trigger] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~MlsNZ(rsk400[Datatype] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~H)0t)pjLBf60[Dbms Version Order] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RUywbxkVFvnA[Default Desktop Deprecated] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~)0f61q10)SJ0[Delete Permission <Read-Only Session>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~j2f6wl10)yA0[Delete Permission <User Interface> <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~AsZNBXsFHzIV[Deploy Sessions Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8jDowIj4ProV[Deployment Collection Source] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~9SwtdMU8HTXR[Deployment Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~E5xR4h7BD9N1[Design Task Baseline] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~DNxivTRj7b30[DiagramTypeLink LineBeginShape] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~jKxixURj7T50[DiagramTypeLink LineEndShape] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~SKBw0ckfDPVB[DiagramTypeObject CreationGesture] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~qgYHmPqYDng2[DiagramTypePath CreationGesture] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~hGoJiTra6bn0[DiagramTypePath Hidden] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~gy0N4qL7pu00[DiagramTypeStorage] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tbEuJRE8GnKE[DocPicture] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UQ8cNafGDjiT[DocumentRTFText] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tXEx)qU41L20[DocumentText] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~w4(hTHyl8980[DoDAF Architecture Modeling Method] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~WrdSGGfqOXsE[Does the application serve internal or external users?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~eiFjCXisGLjP[Enable Snapshots] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Jq9j5RRHCPB0[Estimated End Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xt9j2ORHC560[Estimated Start Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~n7UsIiD(xS10[Evaluation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~HfxvjZVeBvT0[Event Category] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~EtbTalNfEjcF[Exchange Rate] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~bqbTwlNfEngF[Exchange Rate Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UQtYIkOGz800[FK mapping] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~9ryp42jl8D50[Functional Quality] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~rSUv1sXqPH87[GDPR - Is Archived] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~nkzG1B6yPfUF[GDPR - Order] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~BMzDzkp8HXxM[Generate Questionnaires Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~6F4C49pn)OT0[GhostMetaClassIdAbs] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~sBFTom7jMrsQ[Has Touchpoints] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~CjvjWdhJOXuC[Hide Valorized Parameters] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XTT)C(FTDHVJ[Human Asset Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~0d(4NGHv2970[Identifier-Dependent Entity] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xkUUefdeFbqE[Immutability] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~uXVIwfK6Bz00[Implementing Macro] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~pgV4Q1IP2Hu0[INCREMENT BY  Oracle <deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~MGO046Wp(000[Indicator direct value] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~0J7SDfdDATe3[Infrastructure Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~U8q(EklZ3T80[Inherited Comment] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~NVKR4x6b3f10[Inherited Comment <Deutsch>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8VKR3x6b3n00[Inherited Comment <English>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~cVKR5x6b3X20[Inherited Comment <Français>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ISKR7x6b3950[Inherited Comment <Italiano>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~jVKR5x6b3z20[Inherited Comment <Spanish>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~dcuUxkt0Cv10[Initial Workflow Status Instance] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~hgyx()(iva10[Interaction Local Name] ")); 
		   //oCol.remove(megaRoot.getObjectFromID("~L30000000L90[Is Readable] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tavGnmb7Lj0K[is Report Editable <System>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ltdSTEfqOncE[Is this application a custom or a COTS?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~CqdSyFfqOboE[Is this application in line with the company's future technology direction?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~U30000000v90[Is UI Readable] ")); 
		   //oCol.remove(megaRoot.getObjectFromID("~M30000000P90[Is Writable] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~N0cxvyUSJTfH[isAnyWhere] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GtbTC)OfEvLK[Issuer] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~oBohM(A2PPL9[IsWebAccountActivated] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~SGi7p(XsKP31[Kind of proposition] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~a20000000H60[Language Update Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~AmrRTz6BzaG0[LanguageUpdateDate <German>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~lruWiwPENzc5[Last Assessment] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xFIuXclVF5qJ[Last password change] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~opePJE7U9L31[LDAP User Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~YG1oeBzXGXQ3[Library Type] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~820000000X40[Link Creator Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~A20000000f40[Link Modifier Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~b20000000L60[LinkLanguageUpdateDate] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~0nrRTz6BzyJ0[LinkLanguageUpdateDate <German>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~THoMojrh6n10[Linktype] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~f20000000b60[Log] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~6kSdFxvCFDFF[Main Workflow Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~V4tmgHyVy800[Mapping Selection] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~qkXzfQ)X9H30[Master Plan Start Date Saved] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~uUIJZJLXBXJ1[Message Flow CreationKind] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~TLVnH2zCv4T1[Message local name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ijtl(AiGJL12[Meta Scope] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~4000)))sMHQC[MetaDiagramType Library] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~WOLaD6vuu000[MetaDiagramType Library32] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~O300)))UAIQC[MetaDiagramTypeBitmap] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~W000)))mAIQC[MetaDiagramTypeCursor] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~jUERqz6y9v30[MetaTreeBranchMetaClassFilter] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~oYhsQ0mq(q3k[Method overview] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Gs9jUURHCXL0[Milestone Estimated Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XvFt6MUDGHpO[MinMaxValueLayout] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~dTbxk3VxK1wO[Mode Questionnaires Creation <deprecated>] ")); 
		  // oCol.remove(megaRoot.getObjectFromID("~c10000000P20[Modifier Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UW5hzwS03vJ3[Navigability Display] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~oCIuIjlVFLyJ[Number of failed login attempts] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5MBfaJgSn400[Number of Times] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~z20000000r70[Object Availability] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~d20000000T60[Object MetaClass IdAbs] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~p20000000D70[Object MetaClass Name] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~T3vfqZw0CjB0[OrgUnit Component MetaPicture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8mkrnST0uK00[Origin] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~vasuYLkI1j10[Owner] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~63g32ymV9z30[Parent Idabs] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~yYwUektD)000[Parent/child] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~X4sWyGeE9fF0[Participant Type] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UcF8H62ZFHE5[Password management mode] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~HWB)5G0rTPQV[PasswordsLRU] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~kWkp2IF5HvpN[Path] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~NmXS0qAIAP35[Physical Asset Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ZaAlhAztIX02[Picture Id] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tbFegCw7AXA0[Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~7lmqbEjCz800[PK mapping] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RGzwku(yBrV1[Previous Validation State  <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~G8RVMuN5u000[Primary Index] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~w10000000f30[Profile manager] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RbFbTcPfEL5K[Progression Rate] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5dx1OqWoKrP9[Project path] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~uG7ShfdDA5k3[Project Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~KzXYUKiKObRP[Project Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~BfUK8RboKn7M[Project tree title] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~IlhZBz8UoK00[Quality] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~nZWnrQSiH5MH[Questionnaire Validation Workflow] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~qY(XlfQ7E9qV[RACI BPMN Default Value] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~U0f6zp10)iH0[Read Permission <Read-Only Session>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~T1f6Ol10)480[Read Permission <User Interface> <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~CM7LDI5fy800[Recycle bin] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~6LJ6RnYuKLiT[Report DataSet Result] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Vt0MLXLBLPGH[Report DataSet Result <Deutsch>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xq0MKXLBLH6H[Report DataSet Result <English>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~dq0MOXLBLfaH[Report DataSet Result <Español>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Lt0MOXLBLbVH[Report DataSet Result <Français>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ut0MbXLBLbmI[Report DataSet Result <Italiano>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~DKckRsBBLHPW[Report DataSet Result Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~kuKj2)y(RDTS[Report Graph Arc Group Above] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Q30000000f90[Repository Access Definition Mode] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~t10000000T30[Repository Access Mode] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~xt)IXn4JBz60[Requester] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tOu6HTIrHzQI[Reserved Objects] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~U0jKF5cCAvi0[RFC State <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~NHqtc0Ek)S30[RTF Content] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~lm8QfisQODRC[Running timestamp  System Job Execution] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ijXzGQ)X9L00[Schedule Date Begin Saved] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~JkXzVQ)X9r10[Schedule Date End Saved] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~WFIu0zkVFX9I[Security question] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~XOLV9zNs)u60[Sequence add-ons] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~68DCn9JXB1P0[Sequence Flow CreationKind] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~R7jIFl6azWR0[Service Application] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GUsFV1aIPPe8[Sketching Item Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~r4pjivRV9r50[SolMan Description] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~17pjHxRV9H80[SolMan Io Class] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~1BWTfxtP95J0[SolMan Storage Category] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~f6pjAqSV9121[SolMan Storage Category Url] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~DH7SqedDArY3[Solution Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~LLjDueJ)A590[Standard Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~DnCfRNQhNnVN[Start Session Execution Mode] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~7eV4C2IP2Pw0[START WITH  Oracle <deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~2PxgmmwTIX0D[Step Order] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~HbXzpRlx4T20[Stereotype] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~gdgzBHaVCz(0[Stereotype Extension] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GYXdc8YLALSB[Strategic Planning] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~tcl5kdbMPnbK[System Component Picture] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Y0BJ2T5VFjw5[System ID] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8TD21mpFHrnV[System Trigger] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Omkr(ST0uW00[Table mapping] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~LOp4vBIcMXdI[Tabular Update Date] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Yt9ZXpesiO40[Target-DBMS] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~18MUjwdUBLJ0[Task State <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~usSVOytIL9ST[Technical json View Component Path] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~ZuSsDlDXxK10[Technical password] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Oqypj1jl8b30[Technical Quality] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Xy1tKqDlQbkD[Test Case Code] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~BtdSvNfqODUG[The average number of people <FTE> that worked on the code over the last 12 months?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~EtdSoFfqOPlE[The number of major releases delivered over the last 12 months?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~QtylMBjCIfFA[Tree Title] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~yPCvFVU4Pzw0[Type Characteristics] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Zon)q8Gyte20[UML ActorName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~0XytF2kose20[UML ClassName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~utDLIK5Au000[UML ComponentName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~LXDonfCBsW10[UML EventName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~vXyt(2kosG30[UML PackageName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Lon)f8GytS20[UML UseCaseName] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~k0f6)p10)aI0[Update Permission <Read-Only Session>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~72f6hl10)W90[Update Permission <User Interface> <Deprecated>] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~NcNHyaar7T30[UpgradeStatus] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~aen5xX19OveL[URI] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~len5xX19ObfL[URI Alias] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~GnJ2QsGE9zz0[Used System Type] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~u20000000X70[User Activation] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8XigFCyNE9VP[Value Value Label] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~1JubVrPLAfG0[Variable Object Picture Identifier] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~WVhmuCi15520[Variation Inheritance] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RifvvT()rO00[Version] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~UhQ6VcZBLfaT[View Component Scope] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5qdSsIfqOLMF[What is the annual staff turnover within the development team?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~8rdS9FfqObfE[What is the application type] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~SsdSPGfqOjvE[What is the approximate number of end users?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~BtdSlIfqO9JF[What is the average skill level of the development team on this type of application?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~KrdSPIfqO9DF[What is the CMMI level of the organization?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~2rdSzIfqOjPF[What is the percentage of change to the base code in the last 12 months?] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~EsdScIfqOzFF[What percentage of the development effort has been spent on maintenance in the last 12 months?] ")); 
		 //  oCol.remove(megaRoot.getObjectFromID("~2NaRYvdOHzL7[Workflow Current Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~RejdHHw0Cz50[Workflow Engine Version] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~qLaREvdOHLI7[Workflow Previous Status] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~Dl)PstMpBvk0[Workflow Status Instance State] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~udfd7NwTHjYO[Workspace Connection] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~5eiWVIeqH9IK[Workspace History] ")); 
		   oCol.remove(megaRoot.getObjectFromID("~03rwGjU3z000[XPath] ")); 
		   
		   
		   
		return oCol;
	}
	
}
