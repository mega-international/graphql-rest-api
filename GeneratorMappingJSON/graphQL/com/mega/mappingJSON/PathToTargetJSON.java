package com.mega.mappingJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

public class PathToTargetJSON extends CommonFieldsMaeJSON {

	private FilterMaeJSON condition;

	public PathToTargetJSON(HashMap<String,String> overrideNameList,String maeID, String metaClassID) {
		super(overrideNameList,maeID,metaClassID);
	}
	
	public PathToTargetJSON(PathToTargetJSON pathToTargetJSON,HashMap<String,String> overrideNameList) {	
		super(overrideNameList,pathToTargetJSON.getMaeID(),pathToTargetJSON.getMetaClassID());
//		this.maName = pathToTargetJSON.getMaName()+"";
		this.name = pathToTargetJSON.getName()+"";
		this.maeName = pathToTargetJSON.getMaeName()+"";
		this.maeID = pathToTargetJSON.getMaeID()+"";
		this.metaClassName = pathToTargetJSON.getMetaClassName()+"";
		this.metaClassID = pathToTargetJSON.getMetaClassID()+"";
		this.multiplicity = pathToTargetJSON.getMultiplicity()+"";
		this.id = pathToTargetJSON.getId()+"";
		List<PropertiesJSON> initialProperties = pathToTargetJSON.getProperties();

		if (initialProperties != null) {
			this.properties = new ArrayList<PropertiesJSON>();
			Iterator<PropertiesJSON> iterator = initialProperties.iterator();
			while (iterator.hasNext()) {
				// not sure it work maybe we should duplicate the object
				this.properties.add(iterator.next());
			}	
		}
		
	}
	
	public void setCondition(FilterMaeJSON condition) {
		this.condition = condition;
	}

	public FilterMaeJSON getCondition( ) {
		return this.condition;
	}	
	
}
