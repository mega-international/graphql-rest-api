package com.mega.mappingJSON;

import java.util.HashMap;
import java.util.List;

public abstract class CommonFieldsMaeJSON extends CommonFields {

//	protected String id;
//	protected String maName;
	protected String maeName;
	protected String maeID;
	protected String metaClassName;
	protected String metaClassID;
	protected String multiplicity;		
//	protected HashMap<String,String> overrideNameList;
	protected List<PropertiesJSON> properties;	

	
	public CommonFieldsMaeJSON(HashMap<String,String> overrideNameList, String maeID, String metaClassID) {
		this.overrideNameList = overrideNameList;
		setMaeID(maeID);
		setMetaClassID(metaClassID);
	}
	
	public String getMaeName() {
		return this.maeName;
	}
	
	public String getMaeID() {
		return this.maeID;
	}	
	
	public void setMaeID (String maeID) {
		this.maeID = maeID;
	}

	public void setMaeName(String maeName) {
		this.maeName = getRealNameWithId(maeName,getMaeID());
	}

	public void setMetaClassName(String metaClassName) {
		this.metaClassName = getRealNameWithId(metaClassName,getMetaClassID());
	}
	
	public String getMetaClassName () {
		return this.metaClassName;
	}
	
	public void setMetaClassID (String metaClassID ) {
		this.metaClassID = metaClassID;
	}
	public String getMetaClassID () {
		return this.metaClassID;
	}
	
	public void setMultiplicity (String multiplicity) {
		this.multiplicity = multiplicity;
	}
	public String getMultiplicity() {
		return this.multiplicity;
	}

	public List<PropertiesJSON> getProperties() {
		return this.properties;
	}
	
	public void setProperties(List<PropertiesJSON> properties) {
		this.properties = properties;
	}			
	
}
