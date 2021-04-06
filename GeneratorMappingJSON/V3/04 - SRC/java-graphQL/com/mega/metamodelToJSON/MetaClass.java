package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import com.mega.generator.Arguments;
import com.mega.generator.Generator;
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
	private MegaCollection pColTaggedValue;
	private MegaCollection pColMetaAssociationEnd;

	private boolean isValidMetaClass = true; 
	private boolean hasNameSpace = false;

	
//	private HashMap<String,String> metaAssociationEndHashMap = new HashMap<String,String>();	
	ArrayList<RelationshipsJSON> relationshipsJSONList = new ArrayList<RelationshipsJSON>();
	ArrayList<PropertiesJSON> propertiesJSONList = new ArrayList<PropertiesJSON>();		
	
	private HashMap<String,String> globalUniqueName;
	
	
	public MetaClass(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oMetaClass, HashMap<String,String> overrideNameList,HashMap<String,String> globalUniqueName) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.oMetaClass = oMetaClass;
		this.overrideNameList = overrideNameList;
		this.globalUniqueName = globalUniqueName;

		this.isCustom = computeIsCustom(oMetaClass);			
		if (getIsCustom()) {
			setAtLeastOneCustom(true);
		}
		

		

		
		boolean isAvailable = (boolean) oMetaClass.callFunction("IsAvailable");	;		
		if (doNotRestrictConfidentiality(oMetaClass)) {
			isAvailable = true;
		}
		
		Generator.logger.finest("MetaClass = isAvailable = " + isAvailable );
		
	
		if (isAvailable) {
			pMetaClass = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClass);			
			pColMetaAttribute = pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.pMainProperties);			
			pColTaggedValue = oMetaClass.getCollection(StaticFields.metaClassTaggedValue);
			pColMetaAssociationEnd = pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.pCollections);
		
			setProperties();
		} else {
			isValidMetaClass = false;
		}

		// we take only customization if getExtendOnly = true		
		if (Arguments.getExtendOnly() && !getAtLeastOneCustom()) {
			isValidMetaClass = false;
		}
		
		Generator.logger.finest("MetaClass = isValidMetaClass = " + isValidMetaClass );
	
		
	}
	
	public static boolean doNotRestrictConfidentiality(MegaObject oMetaClass) {
		boolean isAlwaysVisible = false;
		String oIdAbs = oMetaClass.getProp(StaticFields.absoluteIdentifier);		

		
		switch (oIdAbs) {  
		case "M20000000Q10":  // Language 
			Generator.logger.finest("MetaClass = Language is always available = " + oIdAbs );
			isAlwaysVisible = true;
		break;   						
		default:  
			//
		} 
		
	
		return isAlwaysVisible;
		
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
		metaClassJSON.setProperties(propertiesJSONList);
		metaClassJSON.setRelationships(relationshipsJSONList);
	
		
		ConstraintsMetaClass constraints = metaClassJSON.getConstraints();
		
		if (MetaClass.isRootMetaClass(this.megaRoot,this.oMetaClass))  {
			constraints.setQueryNode(true);			
		} else {
			constraints.setQueryNode(false);	
			// to remove the item in the JSON
			//this.isValidMetaClass = false;
			
		}

		if (hasNameSpace(this.megaRoot,this.oMetaClass,this.pColMetaAttribute)) {
			constraints.setNameSpace(true);	
			this.hasNameSpace = true;
		}
		
		constraints.setReadOnly(isReadOnly(technincalName));

		Generator.logger.finest("MetaClass = JSON Name = " + metaClassJSON.getName());
		
		
		setMetaAttribute();
		setTaggedValue();
		setMetaAssociation();
		
		MetaModel.reportDuplicateName("TaggedValue", propertiesJSONList, ensureUniqueName);
		MetaModel.reportDuplicateName("MetaAssociation", relationshipsJSONList, ensureUniqueName);

		
	} // setProperties
	
	
	private boolean isReadOnly(String technincalName) {
		boolean readonly = false;
		
		if (technincalName.contentEquals("DiagramType") || technincalName.contentEquals("diagramType")) {
			readonly=true;
		} else if (technincalName.contentEquals("Login") || technincalName.contentEquals("login")) {
			readonly=true;
		} else if (technincalName.contentEquals("Language") || technincalName.contentEquals("language")) {
			readonly=true;
		} else if (technincalName.contentEquals("DBMSVersion") || technincalName.contentEquals("dBMSVersion")) {
			readonly=true;
		}
			
		Generator.logger.finest("MetaClass = isReadOnly = " + readonly);
		
		
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
		Generator.logger.fine("Start MetaAttributes");
		//ArrayList<MetaAttribute> metaAttributeObjectList = new ArrayList<MetaAttribute>();

		 
		propertiesJSONList.addAll(MetaAttribute.manualAttribute(this.hasNameSpace,overrideNameList));

		// we remove some elements that we don't want // TO DO : is it smart the way it is done ?
		pColMetaAttribute = MetaAttribute.cleanCollectionMetaAttribute(megaRoot, pColMetaAttribute);
		Generator.logger.fine("Size = "+pColMetaAttribute.size());
		
		while(pColMetaAttribute.hasNext()) {
			MegaObject pMetaAttribute = pColMetaAttribute.next();
			MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,pMetaAttribute, overrideNameList);
						
			if (metaAttribute.getAtLeastOneCustom()) {
				this.setAtLeastOneCustom(true);
			}
			
			if (metaAttribute.getIsValid()) {
				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				propertiesJSONList.add(propertiesJSON);	
			} // if valid	
			//if (metaAttribute.getIsMetaAttributeObject()) {
			//	metaAttributeObjectList.add(metaAttribute);
			//}
			

		} //while
		
		//MetaModel.reportDuplicateName("MetaAttribute", propertiesJSONList, ensureUniqueName);
		
		// we wanted to threat MetaAttribute Object in a different way but not possible for now
		//setMetaAttributeAsRelationShip(metaAttributeObjectList);
		
	} // setMetaAttribute

	
	
	
	
	private void setTaggedValue() {
		Generator.logger.fine("Start TaggedValue");
		Generator.logger.finest("Will take only TaggedValue connected to the metaclass");
		
		Generator.logger.fine("Size = "+pColTaggedValue.size());
		
		while(pColTaggedValue.hasNext()) {
			MegaObject pTaggedValue = pColTaggedValue.next();
			
			TaggedValue taggedValue = new TaggedValue(megaRoot,oMetamodel,pTaggedValue, overrideNameList);
			
			if (taggedValue.getAtLeastOneCustom()) {
				this.setAtLeastOneCustom(true);
			}
			
			
			if (taggedValue.getIsValid()) {
				PropertiesJSON propertiesJSON = taggedValue.getPropertiesJSON();
				propertiesJSONList.add(propertiesJSON);	
			} 
		} //while
		

	} // setTaggedValue	
	
	
/*	
	private void setMetaAttributeAsRelationShip(ArrayList<MetaAttribute> metaAttributeObjectList) {
		Generator.logger.fine("Start MetaAttributeAsRelationShip");

		Iterator<MetaAttribute> it = metaAttributeObjectList.iterator();
		
		while(it.hasNext()) {	
			MetaAttribute metaAttribute = it.next();

			MetaAttributeObject metaAttributeObject = new MetaAttributeObject(megaRoot, oMetamodel, oMetaClass, metaAttribute, overrideNameList);
			if (metaAttributeObject.getIsValidMetaAttributeObject()) {
				List<RelationshipsJSON> relationshipsJSONListtemp = metaAttributeObject.getRelationshipsJSONList();
				relationshipsJSONList.addAll(relationshipsJSONListtemp);				
			}
			
			
		} //while

		MetaModel.reportDuplicateName("MetaAttribute Object", relationshipsJSONList, ensureUniqueName);
		
		
	}
*/	
	private void setMetaAssociation() {
		Generator.logger.fine("Start MetaAssociation");

		Generator.logger.fine("Size = "+pColMetaAssociationEnd.size());

			while(pColMetaAssociationEnd.hasNext()) {		
				MegaObject pMetaAssociationEnd = pColMetaAssociationEnd.next();
				MetaAssociationEndList metaAssociationEndList = new MetaAssociationEndList(megaRoot,oMetamodel,oMetaClass,pMetaAssociationEnd, overrideNameList,globalUniqueName);
				
				if (metaAssociationEndList.getAtLeastOneCustom()) {
					this.setAtLeastOneCustom(true);
				}
				
				
				if (metaAssociationEndList.isValideMetaAssociationEnd()) {			
					List<RelationshipsJSON> relationshipsJSONListtemp = metaAssociationEndList.getRelationshipsJSONList();
					relationshipsJSONList.addAll(relationshipsJSONListtemp);
				} // if valid
			} // while

			
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
		//Generator.logger.finest("MetaClass = " + oMetaClass.getProp(StaticFields.shortName) + " isFeatured = " + result);
		
		return result;		
	}
	
	
	public static boolean isRootMetaClass(MegaRoot megaRoot,  MegaObject oMetaClass ) {
		boolean result = !isFeaturedMetaClass(megaRoot,oMetaClass);		
		//Generator.logger.finest("MetaClass = " + oMetaClass.getProp(StaticFields.shortName) +  " isRootMetaClass = " + result);
		return result;
	}	
	

	public static MegaCollection getCollectionShouldBeFeature(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");

		List<String> listMetaClass = listCollectionShouldBeFeature();
		Iterator<String> it = listMetaClass.iterator();	
		while(it.hasNext()) {
			String strMetaClass= it.next();
			MegaObject objectToInsert = megaRoot.getObjectFromID(strMetaClass);
			boolean objectexist = objectToInsert.exists();
			if (objectexist) {
				oCol.insert(objectToInsert);
			}
		}

		return oCol;
	}	
	
	// this function explicitly list the MetaClass not considered as intermediate object even if they inherint from Features
	public static MegaCollection getCollectionOfObjectNotFeature(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");
		
		List<String> listMetaClass = listCollectionOfObjectNotFeature();
		Iterator<String> it = listMetaClass.iterator();	
		while(it.hasNext()) {
			String strMetaClass= it.next();
			MegaObject objectToInsert = megaRoot.getObjectFromID(strMetaClass);
			boolean objectexist = objectToInsert.exists();
			if (objectexist) {
				oCol.insert(objectToInsert);
			}
		}
			
		return oCol;
	}	


	public static MegaCollection getCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass(MegaRoot megaRoot) {
		MegaCollection oCol = megaRoot.getSelection("");

		List<String> listMetaClass = listCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass();
		Iterator<String> it = listMetaClass.iterator();	
		while(it.hasNext()) {
			String strMetaClass= it.next();
			MegaObject objectToInsert = megaRoot.getObjectFromID(strMetaClass);
			boolean objectexist = objectToInsert.exists();
			if (objectexist) {
				oCol.insert(objectToInsert);
			}
		}		
		
		return oCol;
	
	}
	
	public static List<String> listCollectionShouldBeFeature() {
		List<String> listMetaClass = new ArrayList<String>();
		listMetaClass.add("~030000000240[Responsability Assignment]");
		listMetaClass.add("~zs2udS(dEnKQ[ASsessment Signatory]");
		
		// variation
		listMetaClass.add("~lJp58zHf4H50[Variation]");

		
		
		return listMetaClass;		
	}	
	
	public static List<String> listCollectionOfObjectNotFeature() {
		List<String> listMetaClass = new ArrayList<String>();
		listMetaClass.add("");

		listMetaClass.add("~8xkukQk8OD(1[Application Flow]");
		listMetaClass.add("~nQ65TIXKp400[Objective]");
		listMetaClass.add("~OsUiS9B5iiQ0[Operation]");
		listMetaClass.add("~DX4rZajG7jQ0[Event]");
		listMetaClass.add("~WIQqh8jB9TB0[Data Object]");
		listMetaClass.add("~GGQqWDjB95F0[Gateway]");
		listMetaClass.add("~CK0MCRcD9bV1[Message Flow]");
		listMetaClass.add("~jsV6VsHL7vJ0[Sequence Flow]");
		listMetaClass.add("~wxm88LFE9fG0[System Used]");
		listMetaClass.add("~Pyc40Cn08f30[Task]");
		listMetaClass.add("~ThXPLe61za91[Value Stage]");
		listMetaClass.add("~eNC4(5X18P30[Participant]");
		listMetaClass.add("~v2rdXCwxCjC0[Dictionary Entity Component Classification]");
		listMetaClass.add("~w(XYAufKOT6M[Project Deliverable]");
		listMetaClass.add("~YXRV)88Dp0G1[Attribute]");
		listMetaClass.add("~cHFQE4Ny(m50[Attribute <DM>]");
		listMetaClass.add("~jdFzaq1Bkyb1[colummn]");
		listMetaClass.add("~s8rEmjZmoe00[Operation <UML>]");
		listMetaClass.add("~wdFzaq1Bkmc1[Index]");
		listMetaClass.add("~VdFzZq1Bk8b1[Key]");
		listMetaClass.add("~D7p5A4oEBnC2[Enterprise Goal]");
		listMetaClass.add("~WmtRiDtI99C0[Offering]");
		listMetaClass.add("~Oump8OjLIb6J[Conversation]");
		listMetaClass.add("~WIQqh8jB9TB0[object]");
		listMetaClass.add("~RrIc2(AJQfA5[GDPR Risk]");
		listMetaClass.add("~WGBjQ)sVHz4K[Logical Software Realization]");
		listMetaClass.add("~n7zRl)MfvK70[Role]");
		listMetaClass.add("~qtv62qXBALA0[Service Point]");
		listMetaClass.add("~cqv6TqXBAnB0[Request Point]");
		listMetaClass.add("~dfbVXxN0QnPE[Organization Department]");
		listMetaClass.add("~e4v3m1H9QnLB[Organization Partner]");
		listMetaClass.add("~POj9qE31QzLU[Organizational Responsibility]");
		listMetaClass.add("~GhbVn0O0Q9hE[Organizational Position]");	
		
		return listMetaClass;		
	}	
	 


	public static List<String> listCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass() {
		List<String> listMetaClass = new ArrayList<String>();
		listMetaClass.add("~UkPT)TNyFDK5[Business document]");
		listMetaClass.add("~AbEujCE8GfyD[Business Document Version]");
		listMetaClass.add("~R9OwsdWJGj2A[Element with business Document]");
		listMetaClass.add("~KekPBSs3iS10[Diagram]");
		listMetaClass.add("~78xw2lkYo400[Note]");
		return listMetaClass;		
	}	
		
	

	
}
