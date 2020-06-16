package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.mega.generator.Generator;
import com.mega.mappingJSON.PathToTargetJSON;
import com.mega.mappingJSON.RelationshipsJSON;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAttributeObject extends CommonAttributes {

	public static String metaAttributeObjectLabel = "MetaAttributeObject";
	
	private MetaAttribute metaAttribute;
	private MegaObject oStartingMetaClass;
	
	private boolean isValidMetaAttributeObject = true;	

	private List<RelationshipsJSON> relationshipsJSONList;
	private RelationshipsJSON relationshipsJSON;	
	
	public MetaAttributeObject(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oStartingMetaClass, MetaAttribute metaAttribute, HashMap<String,String> overrideNameList) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.oStartingMetaClass = oStartingMetaClass;
		this.metaAttribute = metaAttribute;
		this.overrideNameList = overrideNameList;
		
		this.relationshipsJSONList = new ArrayList<RelationshipsJSON>();		
		
		relationshipsJSON = new RelationshipsJSON(overrideNameList,globalUniqueName);	
		relationshipsJSONList.add(relationshipsJSON);			

		Generator.logger.finer("MetaAttributeObject starting setProperties" );
		
		
		setProperties(); 
		
	}

	public boolean getIsValidMetaAttributeObject() {
		return this.isValidMetaAttributeObject;
	}

	public List<RelationshipsJSON> getRelationshipsJSONList() {
		return this.relationshipsJSONList;
	}	
	
	@SuppressWarnings("null")
	private void setProperties() {	

		String metaAttributeId = metaAttribute.getAbsoluteIdentifier();
		String startingMetaClassId = oStartingMetaClass.getProp(StaticFields.absoluteIdentifier);
		String id = startingMetaClassId +"_" + metaAttributeId;
		String technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAttribute(metaAttribute.getoMetaAttribute().getProp(StaticFields.shortName));
		
		MegaObject oTargetMetaClass =  null;// MetaAttribute.getMetaClassOfMetAttributeObjectAsRelationShip(megaRoot, metaAttributeId);
		
		if (oTargetMetaClass.exists()) {
			String targetMetaClassID = oTargetMetaClass.getProp(StaticFields.absoluteIdentifier);
			String oIdAbsMEA = metaAttributeId;
			
			String technincalNameMEA =metaAttributeObjectLabel;
	
			String technincalNameMetaClassStarting =UtilitiesMappingJSON.getTechnicalNameMetaClass((oStartingMetaClass.getProp(StaticFields.shortName)));
			String technincalNameMetaClassTarget = UtilitiesMappingJSON.getTechnicalNameMetaClass((oTargetMetaClass.getProp(StaticFields.shortName)));
			String sMetaMultiplicity = "0..1";
					
			PathToTargetJSON pathToTargetJSON = new PathToTargetJSON(overrideNameList,oIdAbsMEA,targetMetaClassID);	
			pathToTargetJSON.setId(metaAttributeId);		
			pathToTargetJSON.setMaeID(oIdAbsMEA);
			pathToTargetJSON.setMetaClassID(targetMetaClassID);
			pathToTargetJSON.setMaeName(technincalNameMEA);
			pathToTargetJSON.setName(technincalName);
			pathToTargetJSON.setMetaClassName(technincalNameMetaClassTarget);
			pathToTargetJSON.setMultiplicity(sMetaMultiplicity);
	
			
			relationshipsJSON.setId(id);
			relationshipsJSON.addPathToTargetJSON(pathToTargetJSON,startingMetaClassId,technincalNameMetaClassStarting,false,"","","");
			
			relationshipsJSON.setName(technincalName);
			
		} else {
			Generator.logger.severe("Missing metaclass mapping for MetaAttribute Object =" + metaAttributeId + " - "+ technincalName);

			isValidMetaAttributeObject = false;
		}
		
		
	}
	
}
