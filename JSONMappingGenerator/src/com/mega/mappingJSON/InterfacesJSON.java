package com.mega.mappingJSON;

import java.util.HashMap;
import java.util.List;

public class InterfacesJSON  extends CommonFieldsJSON {
	
	public InterfacesJSON(HashMap<String,String> overrideNameList, String id) {
		super(overrideNameList,id);
	}	
	public InterfacesJSON(String name, String id, String description,HashMap<String,String> overrideNameList) {
		super(name, id, description,overrideNameList);
	}
	
	private List<PropertiesJSON> properties;	

	public List<PropertiesJSON> getProperties() {
		return this.properties;
	}
	
	public void setProperties(List<PropertiesJSON> properties) {
		this.properties = properties;
	}
	
}
