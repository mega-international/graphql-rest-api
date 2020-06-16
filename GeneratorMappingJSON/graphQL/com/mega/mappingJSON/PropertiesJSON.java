package com.mega.mappingJSON;

import java.util.HashMap;
import java.util.List;

public class PropertiesJSON extends CommonFieldsJSON {

	private ConstraintsProperties constraints;
	private List<EnumValuesJSON> enumValues;
	
	public PropertiesJSON(HashMap<String,String> overrideNameList,String id) {
		super(overrideNameList,id);
	}

	public PropertiesJSON(String name, String id, String description,HashMap<String,String> overrideNameList) {
		super(name, id, description,overrideNameList);
	}	
	
	public void setEnumValues(List<EnumValuesJSON> enumValues) {
		this.enumValues = enumValues;
	}

	public List<EnumValuesJSON>  getEnumValues() {
		return this.enumValues;
	}
	
	public void setConstraints(ConstraintsProperties constraints) {
		this.constraints = constraints;
	}

	public ConstraintsProperties getConstraints() {
		return this.constraints;
	}	
	
}
