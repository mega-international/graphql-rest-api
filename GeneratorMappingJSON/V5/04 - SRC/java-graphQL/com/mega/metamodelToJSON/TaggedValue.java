package com.mega.metamodelToJSON;

import java.util.HashMap;
import java.util.List;

import com.mega.mappingJSON.EnumValuesJSON;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;

public class TaggedValue extends GenericAttributeOrTaggedValue   {
		
	public TaggedValue(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject pMetaAttribute, HashMap<String,String> overrideNameList) {
		super(megaRoot, oMetamodel, pMetaAttribute, overrideNameList);
		
	}		

	public TaggedValue(String technincalName, String absoluteIdentifier, String sDescription, String maxLength, String type, boolean isMetaAttributeFilter, boolean isMandatory, boolean isReadOnly, boolean translatable, boolean formattedText, boolean isUnique,HashMap<String,String> overrideNameList) {
		super(technincalName, absoluteIdentifier, sDescription, maxLength, type, isMetaAttributeFilter, isMandatory, isReadOnly, translatable, formattedText, isUnique,overrideNameList);

	}	
	
	protected String customNaming(String technincalName, String type ) {
		return technincalName;
	}
	
	protected List<EnumValuesJSON> getMetaAttributeValues() {		
		// TO DO				
		return null;
	}
	
	protected boolean includedAttribute(String technincalName) {
		// we remove certains attributes that are either too technical or redundant
		boolean include = true;
		// TO DO		
		return include;
	}
	
	protected boolean isUnique() {
		boolean unique = false;
		// TO DO		
		return unique;
	}
	
	
	protected boolean isReadOnly(String technincalName) {
		boolean readOnly = false;		
		// TO DO      
		return readOnly;
	}
	

}
