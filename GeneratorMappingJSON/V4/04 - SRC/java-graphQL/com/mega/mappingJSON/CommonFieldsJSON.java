package com.mega.mappingJSON;

import java.util.HashMap;

public abstract class CommonFieldsJSON extends CommonFields {

//	private String name;
//	private String id;
//	private HashMap<String,String> overrideNameList;

	private String description;
	private String implementInterface;

	public CommonFieldsJSON(HashMap<String,String> overrideNameList,String id) {
		this.overrideNameList = overrideNameList;
		setId(id);
	}	
	
	public CommonFieldsJSON(String name, String id, String description,HashMap<String,String> overrideNameList) {
		this.name = name;
		this.id = id;
		this.description = description;
		this.overrideNameList = overrideNameList;
	}
/*	
	public void setName(String name) {
		if (getId()!= null && getId() != "") {
			
			if (overrideNameList.containsKey(getId())) {
				String localname = overrideNameList.get(getId());				
				this.name = localname;
			} else {
				this.name = name;							
			}
		} else {
			this.name = name;			
		}
		if (this.name== "" || this.name == null) {
			this.name = "noValue";
		}
	}
	
	public void setId (String id) {
		this.id = id;
	}
	
	public String getId() {
		return this.id;
	}
		
	public String getName() {
		return this.name;
	}	
*/
	
	public void setDescription (String description) {
		this.description = description;
	}
	
	public String getDescription() {
		return this.description;
	}
	
	public void setImplementInterface (String implementInterface) {
		this.implementInterface = implementInterface;
	}
	
	public String getImplementInterface() {
		return this.implementInterface;
	}	
	
}
