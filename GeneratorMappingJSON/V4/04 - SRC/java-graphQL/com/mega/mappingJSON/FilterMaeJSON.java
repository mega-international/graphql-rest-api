package com.mega.mappingJSON;

import java.util.HashMap;

public class FilterMaeJSON extends CommonFieldsMaeJSON {

	private String objectFilterID;
	private String objectFilterShortName;

	public FilterMaeJSON(HashMap<String, String> overrideNameList,String maeID, String metaClassID) {
		super(overrideNameList,maeID,metaClassID);
	}
	
	public String getObjectFilterID() {
		return this.objectFilterID;
	}
	
	public void setObjectFilterID(String objectFilterID) {
		this.objectFilterID = objectFilterID;
	}
	
	public String getObjectFilterShortName() {
		return this.objectFilterShortName;
	}	
	
	public void setObjectFilterShortName(String objectFilterShortName) {
		this.objectFilterShortName = getRealNameWithId(objectFilterShortName,getObjectFilterID());
	}		
}
