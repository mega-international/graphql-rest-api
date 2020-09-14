package com.mega.mappingJSON;

import java.util.HashMap;

import org.apache.commons.text.WordUtils;


public abstract class CommonFields {

	protected String id;
	protected String name;
	protected HashMap<String,String> overrideNameList;

	public void setId (String id) {
		this.id = id;
	}
	
	public String getId() {
		return this.id;
	}

	protected String getRealNameWithId(String name, String Id) {
		String realName = name;
		if (Id!= null && Id != "") {			
			if (overrideNameList.containsKey(Id)) {			
				realName = overrideNameList.get(Id);
			} else {
				realName = name;							
			}
		} else {
			realName = name;			
		}
		if (realName== "" || realName == null || realName.length() < 1) {
			realName = "noValue";
		}	
		
		if (realName.contentEquals("")) {
			System.out.println("Error - Value name has an empty name" + realName);
		}		
		return WordUtils.capitalize(realName);		
		
	}
	
	
	protected String getRealName(String name) {
		return getRealNameWithId(name, getId());
	}
	
	public void setName(String name) {
		this.name = getRealName(name);
	}	
	
	public String getName() {
		return this.name;
	}	
	
	
/*	
	private String minimizeFirstLetter(String str) {
	    if(str == null || str.isEmpty()) {
	        return str;
	    } 
	    return str.substring(0, 1).toLowerCase() + str.substring(1);
	}	
*/	
}
