package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.mega.mappingJSON.ConstraintsMetaClass;
import com.mega.mappingJSON.MetaClassJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.mappingJSON.RelationshipsJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaClass  extends CommonAttributes  {

	private MetaClassJSON metaClassJSON;
	private	MegaObject oMetaClass;
	private MegaObject pMetaClass;

	private MegaCollection pColMetaAttribute;
	private MegaCollection pColMetaAssociationEnd;

	private boolean isValidMetaClass = true; 
	
	private boolean hasNameSpace = false;
	
	// use to identify duplicate in the JSON and generate errors
	HashMap<String,String> metaAttributeHashMap = new HashMap<String,String>();	
	HashMap<String,String> metaAssociationEndHashMap = new HashMap<String,String>();	
	
	private HashMap<String,String> globalUniqueName;
	
	
	public MetaClass(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oMetaClass, HashMap<String,String> overrideNameList,HashMap<String,String> globalUniqueName) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.oMetaClass = oMetaClass;
		this.overrideNameList = overrideNameList;
		this.globalUniqueName = globalUniqueName;

		boolean isAvailable = (boolean) oMetaClass.callFunction("IsAvailable");
	
		pMetaClass = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClass);		
		
		pColMetaAttribute = pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.pMainProperties);
		
		pColMetaAssociationEnd = pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.pCollections);
		
		if (isAvailable) {
			setProperties();
		} else {
			isValidMetaClass = false;
		}
	}
	
	public MetaClassJSON getMetaClasJSON() {
		return this.metaClassJSON;
	}
	
	public boolean getIsValidMetaClass() {
		return this.isValidMetaClass;
	}
	
	private String getImplementInterface() {
		String implementInterface = "genericObject";
		
		MegaObject oFound = pMetaClass.getCollection(StaticFields.upperClasses).get(StaticFields.genericObjectSystem); 
		if (oFound.exists()) {
			implementInterface = "genericObjectSystem";
		}		
		
		return implementInterface;
	}
	
	
	
	private void setProperties() {
		
		String technincalName = oMetaClass.getProp(StaticFields.shortName);
		technincalName = UtilitiesMappingJSON.getTechnicalNameMetaClass(technincalName);
		String oIdAbs = oMetaClass.getProp(StaticFields.absoluteIdentifier);
		String sDescription = UtilitiesMappingJSON.getCleanComment(oMetaClass.getProp(StaticFields.comment,"display").toString());

		String implementInterface = getImplementInterface();
		
		
		this.metaClassJSON = new MetaClassJSON(overrideNameList,oIdAbs);
	
		
		metaClassJSON.setName(technincalName);
		metaClassJSON.setId(oIdAbs);
		metaClassJSON.setDescription(sDescription);
		metaClassJSON.setImplementInterface(implementInterface);

		ConstraintsMetaClass constraints = metaClassJSON.getConstraints();
		
		if (MetaClass.isRootMetaClass(this.megaRoot,this.oMetaClass))  {
			constraints.setQueryNode(true);			
		} else {
			constraints.setQueryNode(false);		
		}

		if (hasNameSpace(this.megaRoot,this.oMetaClass,this.pColMetaAttribute)) {
			constraints.setNameSpace(true);	
			this.hasNameSpace = true;
			//isValidMetaClass = false;
		}
		
		constraints.setReadOnly(isReadOnly(technincalName));

		
		//System.out.println("------------" + metaClassJSON.getName() + "------------");
		
		setMetaAttribute();
		setMetaAssociation();
		
	} // setProperties
	
	
	private boolean isReadOnly(String technincalName) {
		boolean readonly = false;
		
		if (technincalName.equals("DiagramType")) {
			readonly=true;
		}		
		
		return readonly;
	}
	
	private boolean hasNameSpace(MegaRoot megaRoot, MegaObject oMetaClass, MegaCollection pColMetaAttribute) {
	
		boolean hasnamespace = false;
		
		MegaObject shortName = megaRoot.getObjectFromID("~Z20000000D60[Short Name]");
	
		//colMainProperties=oClassDescription.Description.item(1).MainProperties

		MegaObject oFound = pColMetaAttribute.get(shortName);
		if (oFound.exists()) {
			hasnamespace = true;
		}
		
		return hasnamespace;
	}
	
	private void setMetaAttribute() {
		List<PropertiesJSON> propertiesJSONList = new ArrayList<PropertiesJSON>();		
		 
		metaAttributeHashMap.putAll(MetaAttribute.manualAttribute(this.hasNameSpace,propertiesJSONList,overrideNameList));

		// we remove some elements that we don't want // TO DO : is it smart the way it is done ?
		pColMetaAttribute = MetaAttribute.cleanCollectionMetaAttribute(megaRoot, pColMetaAttribute);

		//System.out.println("------ MetaAttributes ------");
		
		
		while(pColMetaAttribute.hasNext()) {
			MegaObject pMetaAttribute = pColMetaAttribute.next();
			MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,pMetaAttribute, overrideNameList);
			if (metaAttribute.isValideMetaAttribute()) {
				String name = metaAttribute.getName();
				String nameerror = metaAttribute.getNameErrorDisplay();
				
				boolean alreadyExist = metaAttributeHashMap.containsKey(name);
				
				if (alreadyExist) {
					System.out.println("Error - MetAttribute name already exist : " + nameerror);
				}
				
				metaAttributeHashMap.put(name,nameerror);

				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				propertiesJSONList.add(propertiesJSON);				
			} // if valid			

		} //while
		
		metaClassJSON.setProperties(propertiesJSONList);
	}

	private void setMetaAssociation() {
		List<RelationshipsJSON> relationshipsJSONList = new ArrayList<RelationshipsJSON>();

		//System.out.println("------ MetaAssociation ------");
		
		
			while(pColMetaAssociationEnd.hasNext()) {		
				MegaObject pMetaAssociationEnd = pColMetaAssociationEnd.next();
				MetaAssociationEndList metaAssociationEndList = new MetaAssociationEndList(megaRoot,oMetamodel,oMetaClass,pMetaAssociationEnd, overrideNameList,globalUniqueName);
	
				if (metaAssociationEndList.isValideMetaAssociationEnd()) {			
					metaAssociationEndHashMap.putAll(metaAssociationEndList.getMetaAssociationEndHashMap());
					
					List<RelationshipsJSON> relationshipsJSONListtemp = metaAssociationEndList.getRelationshipsJSONList();
					relationshipsJSONList.addAll(relationshipsJSONListtemp);
				} // if valid
			} // while

		metaClassJSON.setRelationships(relationshipsJSONList);
	}

	public static boolean isFeaturedMetaClass(MegaRoot megaRoot, MegaObject oMetaClass) {
		boolean result = false;		
		// we check if this is an intermediate object
		MegaObject pMetaClassTarget = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClass);		 		
		MegaObject oFound = pMetaClassTarget.getCollection(StaticFields.upperClasses).get(StaticFields.feature); 
			
		// we found it was a Feature MetaClass
		if (oFound.exists()) {
			result = true;
		}	
		
	    // we had those who should be Feature but are not marked as featured
		MegaCollection oCol1 = MetaClass.getCollectionShouldBeFeature(megaRoot); 
		while(oCol1.hasNext()) {
			MegaObject oObj1 = oCol1.next();
			if (oObj1.sameID(oMetaClass.getID())) {
				result = true;
			}
		}
		
		// we removed those who should not be featured
		MegaCollection oCol = MetaClass.getCollectionOfObjectNotFeature(megaRoot); 
		while(oCol.hasNext()) {
			MegaObject oObj = oCol.next();
			if (oObj.sameID(oMetaClass.getID())) {
				result = false;
			}
		}		
		
		return result;		
	}
	
	
	public static boolean isRootMetaClass(MegaRoot megaRoot,  MegaObject oMetaClass ) {
		boolean result = isFeaturedMetaClass(megaRoot,oMetaClass);		
		return !result;
	}	
	

	public static MegaCollection getCollectionShouldBeFeature(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaRoot.getObjectFromID("~030000000240[Responsability Assignment]"));
		oCol.insert(megaRoot.getObjectFromID("~zs2udS(dEnKQ[ASsessment Signatory]"));


		return oCol;
	}	
	
	// this function explicitly list the MetaClass not considered as intermediate object even if they inherint from Features
	public static MegaCollection getCollectionOfObjectNotFeature(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaRoot.getObjectFromID("~8xkukQk8OD(1[Application Flow]"));
		oCol.insert(megaRoot.getObjectFromID("~nQ65TIXKp400[Objective]"));
		oCol.insert(megaRoot.getObjectFromID("~OsUiS9B5iiQ0[Operation]"));
		oCol.insert(megaRoot.getObjectFromID("~DX4rZajG7jQ0[Event]"));
		oCol.insert(megaRoot.getObjectFromID("~WIQqh8jB9TB0[Data Object]"));
		oCol.insert(megaRoot.getObjectFromID("~GGQqWDjB95F0[Gateway]"));
		oCol.insert(megaRoot.getObjectFromID("~CK0MCRcD9bV1[Message Flow]"));
		oCol.insert(megaRoot.getObjectFromID("~jsV6VsHL7vJ0[Sequence Flow]"));
		oCol.insert(megaRoot.getObjectFromID("~wxm88LFE9fG0[System Used]"));	
		oCol.insert(megaRoot.getObjectFromID("~Pyc40Cn08f30[Task]"));	
		oCol.insert(megaRoot.getObjectFromID("~ThXPLe61za91[Value Stage]"));	
		oCol.insert(megaRoot.getObjectFromID("~eNC4(5X18P30[Participant]"));			
		oCol.insert(megaRoot.getObjectFromID("~v2rdXCwxCjC0[Dictionary Entity Component Classification]"));			
		oCol.insert(megaRoot.getObjectFromID("~w(XYAufKOT6M[Project Deliverable]"));			
		oCol.insert(megaRoot.getObjectFromID("~YXRV)88Dp0G1[Attribute]"));	
		oCol.insert(megaRoot.getObjectFromID("~cHFQE4Ny(m50[Attribute <DM>]"));	
		oCol.insert(megaRoot.getObjectFromID("~jdFzaq1Bkyb1[colummn]"));	
		oCol.insert(megaRoot.getObjectFromID("~s8rEmjZmoe00[Operation <UML>]"));	
		oCol.insert(megaRoot.getObjectFromID("~wdFzaq1Bkmc1[Index]"));	
		oCol.insert(megaRoot.getObjectFromID("~VdFzZq1Bk8b1[Key]"));
		oCol.insert(megaRoot.getObjectFromID("~D7p5A4oEBnC2[Enterprise Goal]"));
		
		 

				
		return oCol;
	}	
	
	
	 
	
	public static MegaCollection getCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");
		oCol.insert(megaRoot.getObjectFromID("~UkPT)TNyFDK5[Business document]"));
		oCol.insert(megaRoot.getObjectFromID("~AbEujCE8GfyD[Business Document Version]"));
		oCol.insert(megaRoot.getObjectFromID("~R9OwsdWJGj2A[Element with business Document]"));
		oCol.insert(megaRoot.getObjectFromID("~KekPBSs3iS10[Diagram]"));
		oCol.insert(megaRoot.getObjectFromID("~78xw2lkYo400[Note]"));
				
		
			
		
		
		return oCol;
	
	}
	
}
