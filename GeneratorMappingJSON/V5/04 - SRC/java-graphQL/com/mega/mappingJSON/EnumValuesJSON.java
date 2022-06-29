package com.mega.mappingJSON;

import java.util.HashMap;

public class EnumValuesJSON extends CommonFieldsJSON {

	private String internalValue;
	private int order = 0;	

	public EnumValuesJSON(HashMap<String, String> overrideNameList, String id) {
		super(overrideNameList,id);
	}
	
	public void setInternalValue(String internalValue) {
		this.internalValue = internalValue;
	}
	
	public String getInternalValue() {
		return internalValue;
	}
	
	public void setOrder(int order) {
		this.order = order;
	}
	
	public int getOrder() {
		return order;
	}
	
}
