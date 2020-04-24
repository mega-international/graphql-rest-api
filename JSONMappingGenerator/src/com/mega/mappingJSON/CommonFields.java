package com.mega.mappingJSON;

import java.util.HashMap;

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

	protected String getRealName(String name) {
		String realName = name;
		if (getId()!= null && getId() != "") {			
			if (overrideNameList.containsKey(getId())) {			
				realName = overrideNameList.get(getId());
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
		
		return minimizeFirstLetter(realName);
	}
	
	public void setName(String name) {
		this.name = getRealName(name);
	}	
	
	public String getName() {
		return this.name;
	}		
	
	private String minimizeFirstLetter(String str) {
	    if(str == null || str.isEmpty()) {
	        return str;
	    } 
	    return str.substring(0, 1).toLowerCase() + str.substring(1);
	}	
	
}
