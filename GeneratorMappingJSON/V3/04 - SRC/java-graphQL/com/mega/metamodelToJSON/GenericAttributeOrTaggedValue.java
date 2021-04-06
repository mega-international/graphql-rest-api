package com.mega.metamodelToJSON;

import java.util.HashMap;
import java.util.List;

import com.mega.generator.Arguments;
import com.mega.generator.Generator;
import com.mega.mappingJSON.ConstraintsProperties;
import com.mega.mappingJSON.EnumValuesJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public abstract class GenericAttributeOrTaggedValue extends CommonAttributes  {


	protected PropertiesJSON propertiesJSON;
	protected ConstraintsProperties constraintsProperties;

	protected MegaObject oMetaAttribute;	
	protected String absoluteIdentifier;
	
	protected boolean isValidMetaAttribute = true;
	protected boolean isMetaAttributeObject = false;
		
	public GenericAttributeOrTaggedValue(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject pMetaAttribute, HashMap<String,String> overrideNameList) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.overrideNameList = overrideNameList;
		this.oMetaAttribute = megaRoot.getCollection(StaticFields.metaAttribute).get(pMetaAttribute.megaField());
		this.constraintsProperties = new ConstraintsProperties();
				
		String technincalName = "";
		if (oMetaAttribute.exists()) {		
			this.isCustom = computeIsCustom(pMetaAttribute);
			if (getIsCustom()) {
				setAtLeastOneCustom(true);
			}		
			
			absoluteIdentifier = oMetaAttribute.getProp(StaticFields.absoluteIdentifier);
			//removeInvalid(absoluteIdentifier);
			boolean isAvailable = (boolean) oMetaAttribute.callFunction("IsAvailable");

			Generator.logger.finer("MetaAttribute = " +oMetaAttribute.getProp(StaticFields.shortName));
			Generator.logger.finest("MetaAttribute = isAvailable = " + isAvailable );
						
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
		
		
		// we take only customization if getExtendOnly = true		
		if (Arguments.getExtendOnly() && !getAtLeastOneCustom()) {
			isValidMetaAttribute = false;
		}		
		
		Generator.logger.finest("isValidMetaAttribute = " + isValidMetaAttribute );
		
		
	}		

	public GenericAttributeOrTaggedValue(String technincalName, String absoluteIdentifier, String sDescription, String maxLength, String type, boolean isMetaAttributeFilter, boolean isMandatory, boolean isReadOnly, boolean translatable, boolean formattedText, boolean isUnique,HashMap<String,String> overrideNameList) {
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
        constraintsProperties.setFormattedText(isUnique);        
       
        propertiesJSON.setConstraints(constraintsProperties);

	}	
	
	public MegaObject getoMetaAttribute() {
		return this.oMetaAttribute;
	}
	
	public boolean getIsMetaAttributeObject() {
		return this.isMetaAttributeObject;
	}	
	
	public String getAbsoluteIdentifier() {
		return this.absoluteIdentifier;
	}
	
	public PropertiesJSON getPropertiesJSON() {
		return propertiesJSON;
	}
	
	public boolean getIsValid() {
		return this.isValidMetaAttribute;
	}
	
	private void setProperties() {		
		
		String technincalName = oMetaAttribute.getProp(StaticFields.shortName);
		technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAttribute(technincalName);
		String sDescription = UtilitiesMappingJSON.getCleanComment(oMetaAttribute.getProp(StaticFields.comment,"display").toString());
		String maxLength = oMetaAttribute.getProp(StaticFields.metaAttributeLength);		
        String oMetaAttributeType = oMetaAttribute.getProp(StaticFields.metaAttributeType,"internal").toString();
        String type = UtilitiesMappingJSON.getGraphQLType(oMetaAttributeType);

		this.propertiesJSON = new PropertiesJSON(overrideNameList, absoluteIdentifier);
		technincalName = customNaming(technincalName, type );
		
		propertiesJSON.setName(technincalName);
		propertiesJSON.setId(absoluteIdentifier);
		propertiesJSON.setDescription(sDescription);
				
		//we ignoe Object 
		// String sFormat=  oMetaAttribute.getProp(StaticFields.metaAttributeFormat,"internal").toString();
	
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
        constraintsProperties.setMandatory(isMandatory());
        constraintsProperties.setReadOnly(isReadOnly(technincalName));
		constraintsProperties.setTranslatable(isTranslatable());
		constraintsProperties.setFormattedText(isFormattedText(oMetaAttributeType));
		constraintsProperties.setUnique(isUnique());

        propertiesJSON.setConstraints(constraintsProperties);

        if (isEnum()) {
        	propertiesJSON.setEnumValues(getMetaAttributeValues());
        }
         
	}
	
	
	protected abstract String customNaming(String technincalName, String type );
	
	protected abstract List<EnumValuesJSON> getMetaAttributeValues();
	
	protected abstract boolean includedAttribute(String technincalName);

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
	
	protected abstract boolean isReadOnly(String technincalName);
	
	
	
	private boolean isMandatory() {
		boolean mandatory = false;
		if (oMetaAttribute.sameID(StaticFields.shortName)) {
			mandatory = true;
		}				
		return mandatory;
	}
	
	protected abstract boolean isUnique();
	
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
	

}
