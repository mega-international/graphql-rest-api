package com.mega.mappingJSON;

import java.util.HashMap;
import java.util.List;

public class MetaClassJSON extends CommonFieldsJSON{

	
	public MetaClassJSON(HashMap<String,String> overrideNameList, String id) {
		super(overrideNameList,id);
	}	
	public MetaClassJSON(String name, String id, String description,HashMap<String,String> overrideNameList) {
		super(name, id, description,overrideNameList);
	}
	
	private ConstraintsMetaClass constraints = new ConstraintsMetaClass();	
	
	private List<PropertiesJSON> properties;	
	private List<RelationshipsJSON> relationships;

	public void setConstraints(ConstraintsMetaClass constraints) {
		this.constraints = constraints;
	}

	public ConstraintsMetaClass getConstraints() {
		return this.constraints;
	}	
	
	public List<PropertiesJSON> getProperties() {
		return this.properties;
	}
	
	public void setProperties(List<PropertiesJSON> properties) {
		this.properties = properties;
	}
	
	public void setRelationships(List<RelationshipsJSON> relationships) {
		this.relationships = relationships;
	}

	public List<RelationshipsJSON> getRelationships() {
		return this.relationships;
	}	

	
}
