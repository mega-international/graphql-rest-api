package com.mega.metamodelToJSON;

import java.util.HashMap;

import com.mega.mappingJSON.EnumValuesJSON;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAttributeValue   extends CommonAttributes {

	private MegaObject oMetaAttributeValue;
	private EnumValuesJSON enumValuesJSON;
	private String absoluteIdentifier;
	private MegaObject oMetaAttribute;
	private boolean isValidMetaAttributeValue = true;
	
	
	public MetaAttributeValue(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oMetaAttribute, MegaObject oMetaAttributeValue,HashMap<String,String> overrideNameList) {
		this.oMetaAttributeValue = oMetaAttributeValue;
		this.overrideNameList = overrideNameList;
		this.megaRoot = megaRoot;
		this.oMetamodel=oMetamodel;
		this.oMetaAttribute=oMetaAttribute;

		boolean isAvailable = (boolean) oMetaAttributeValue.callFunction("IsAvailable");
	
		if (isAvailable) {
			setProperties();
		} else {
			this.isValidMetaAttributeValue = false;			
		}
		
	}

	public EnumValuesJSON getEnumValuesJSON() {
		return this.enumValuesJSON;
	}

	public boolean isValideMetaAttributeValue() {
		return this.isValidMetaAttributeValue;
	}
	
	private void setProperties() {

		String technincalName = oMetaAttributeValue.getProp(StaticFields.valueName);
		
		
	
		String sDescription = technincalName + " - " + oMetaAttributeValue.getProp(StaticFields.shortName);

		
		technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAttributeValue(technincalName);
		absoluteIdentifier = oMetaAttributeValue.getProp(StaticFields.absoluteIdentifier);
		sDescription = sDescription.concat(UtilitiesMappingJSON.getCleanComment(oMetaAttributeValue.getProp(StaticFields.comment,"display").toString()));
		String internalValue = oMetaAttributeValue.getProp(StaticFields.internaValue);
		
		technincalName = customCaseName(technincalName, internalValue);
		
		enumValuesJSON = new EnumValuesJSON(overrideNameList,absoluteIdentifier);	
		
		enumValuesJSON.setName(technincalName);
		enumValuesJSON.setId(absoluteIdentifier);
		enumValuesJSON.setDescription(sDescription);
		enumValuesJSON.setInternalValue(internalValue);
	
		removeInvalid(absoluteIdentifier);
	
	}
	
	private void removeInvalid(String absoluteIdentifier) {
/*		
		if (absoluteIdentifier.contentEquals("b)64m5XN3H40")) {
			this.isValidMetaAttributeValue=false;
		} else if (absoluteIdentifier.contentEquals("Vy64J6XN3j50")) {
			this.isValidMetaAttributeValue=false;			
		}
*/		
		
	}
	
	private String customCaseName(String technincalName, String internalValue) {
		
		String idAtt = oMetaAttribute.getProp(StaticFields.absoluteIdentifier);
		if (idAtt.contentEquals("PekPCSs3ii10")) { // case diagram nature metaAttribute value
			technincalName = technincalName + "_" +UtilitiesMappingJSON.getTechnicalNameMetaAttributeValue(internalValue);
		}
		
		
		return technincalName;
	}
	
}
