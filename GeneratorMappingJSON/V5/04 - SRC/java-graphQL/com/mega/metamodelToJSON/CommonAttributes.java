package com.mega.metamodelToJSON;

import java.util.HashMap;

import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public abstract class CommonAttributes {

	protected MegaRoot megaRoot;
	protected MegaObject oMetamodel;

	protected String absoluteIdentifier;
	
	protected boolean isCustom = false;
	protected boolean atLeastOneCustom = false;
	
	protected String name;
	protected String nameErrorDisplay;	

	protected HashMap<String,String> overrideNameList;
	protected HashMap<String,String> globalUniqueName = new HashMap<String,String>();
	protected HashMap<String,String> ensureUniqueName = new HashMap<String,String>();
	
	//public String getName() {
	//	return name;
	//}
	
	
	
	protected boolean getIsCustom() {
		return this.isCustom;		
	}
	
	protected void setIsCustom(boolean isCustom) {
		this.isCustom =isCustom;		
	}	
	
	
	protected boolean getAtLeastOneCustom() {
		return this.atLeastOneCustom;		
	}
	
	protected void setAtLeastOneCustom(boolean atLeastOneCustom) {
		this.atLeastOneCustom =atLeastOneCustom;		
	}	
		
	
	
	protected static boolean computeIsCustom(MegaObject oMegaObject) {
		boolean isCustom = false;
		
		String modifier = oMegaObject.getProp(StaticFields.modifier);		
		// j6L3BsG8kW60 = MEGA User = it has not be changed by a user
		if (!modifier.equals("j6L3BsG8kW60")) {
			isCustom = true;
		}	
		return isCustom;
		
	}
}
