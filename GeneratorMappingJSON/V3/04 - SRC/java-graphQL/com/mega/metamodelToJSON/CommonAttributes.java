package com.mega.metamodelToJSON;

import java.util.HashMap;

import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;

public abstract class CommonAttributes {

	protected MegaRoot megaRoot;
	protected MegaObject oMetamodel;

	protected String absoluteIdentifier;
	
	
	protected String name;
	protected String nameErrorDisplay;	

	protected HashMap<String,String> overrideNameList;
	protected HashMap<String,String> globalUniqueName = new HashMap<String,String>();
	protected HashMap<String,String> ensureUniqueName = new HashMap<String,String>();
	
	//public String getName() {
	//	return name;
	//}
	
}
