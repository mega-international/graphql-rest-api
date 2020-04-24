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
/*
	public void setId(String id) {
		this.id = id;				
	}
	
	public String getId() {
		return this.id;
	}	
*/
	
/*
	public String getMaName() {
		return this.maName;
	}	
*/	
	public String getMaeName() {
		return this.maeName;
	}
	
	public String getMaeID() {
		return this.maeID;
	}	
	
	public void setMaeID (String maeID) {
		this.maeID = maeID;
	}
/*	
	public void setMaName(String maName) {
		if (this.id != null && this.id != "") {
			if (overrideNameList.containsKey(this.maeID)) {
				String localname = overrideNameList.get(this.id);				
				this.maName = localname;
			} else {
				this.maName = maName;							
			}
		} else {
			this.maName = maName;			
		}			
	}	
*/
	
	public void setMaeName(String maeName) {
		this.maeName = getRealName(maeName);
	}

	public void setMetaClassName(String metaClassName) {
		this.metaClassName = getRealName(metaClassName);
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
