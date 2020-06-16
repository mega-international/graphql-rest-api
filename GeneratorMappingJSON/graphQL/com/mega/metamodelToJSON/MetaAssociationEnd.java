package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.mega.generator.Generator;
import com.mega.mappingJSON.FilterMaeJSON;
import com.mega.mappingJSON.PathToTargetJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.mappingJSON.RelationshipsJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAssociationEnd extends CommonAttributes {

	private List<RelationshipsJSON> relationshipsJSONList;
	private boolean isValideMetaAssociationEnd;
	//private MegaObject oStartingMetaClass;

	private HashMap<String,String> globalUniqueName;
	
	
	public MetaAssociationEnd(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oStartingMetaClass, MegaObject oInitialTargetMetaClass, MegaObject oInitialMetaAssociationEnd, MegaObject oMetaAssociationEnd , MegaObject oMetaClass, MegaObject oMetaAssociation,boolean isFirst, HashMap<String,String> overrideNameList, HashMap<String,String> globalUniqueName) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.overrideNameList = overrideNameList;
		//this.oStartingMetaClass = oStartingMetaClass;
		this.relationshipsJSONList = new ArrayList<RelationshipsJSON>();		
		boolean navigate = true;	
		boolean nameForComplexPath = false;

		this.globalUniqueName = globalUniqueName;

		Generator.logger.finer("MetaAssociationEnd = " +oMetaAssociationEnd.getProp(StaticFields.shortName));
		
		
		MegaObject personAssignment = megaRoot.getObjectFromID(StaticFields.personAssignment);
		MegaObject maeAssignement = megaRoot.getObjectFromID(StaticFields.maeAssignement);

		isValideMetaAssociationEnd = true;

		// in case of starting from a diagram we do not navigate the intermediate object
		MegaObject mcDiagram = megaRoot.getObjectFromID(StaticFields.metaclassDiagram);	
		if (oStartingMetaClass.sameID(mcDiagram.getID())) {		
			navigate = false;
			
			
			boolean intermediate = MetaClass.isFeaturedMetaClass(megaRoot,oMetaClass);
			if (intermediate) {
				isValideMetaAssociationEnd = false;	
			}
			
		}
		
		if (oMetaAssociationEnd.sameID(personAssignment.getID())) {
			// We are in the case of the business role  PersonAssignement
			navigate = false;	
			managePersonAssignment(isFirst, navigate, nameForComplexPath, oStartingMetaClass, oMetaAssociationEnd, oMetaAssociation, oMetaClass);
		
		} else if (oMetaAssociationEnd.sameID(maeAssignement.getID())) {
			// We are in the case of the business role  Assignement metaAssociationEnd from PersonSystem
			navigate = false;	
			manageAssignment(isFirst, navigate, nameForComplexPath, oStartingMetaClass, oMetaAssociationEnd, oMetaAssociation, oMetaClass);
			
		} else {

			boolean isAvailableMEA = true;	
			boolean isAvailableMetaClass = true;
			
			isAvailableMEA = (boolean) oMetaAssociationEnd.callFunction("IsAvailable");
			isAvailableMetaClass = (boolean) oMetaClass.callFunction("IsAvailable");			

	
			if (doNotRestrictConfidentiality(oMetaAssociationEnd) && MetaClass.doNotRestrictConfidentiality(oMetaClass)) {
				isAvailableMEA = true;
				isAvailableMetaClass = true;
			}
			
			
			if (isAvailableMEA && isAvailableMetaClass ) {
				// we are in all the other cases
				RelationshipsJSON relationshipsJSON = new RelationshipsJSON(overrideNameList,globalUniqueName);	
				relationshipsJSONList.add(relationshipsJSON);		
				
				setProperties(relationshipsJSON, isFirst,navigate,nameForComplexPath,oStartingMetaClass,oMetaAssociationEnd,oMetaClass, oMetaAssociation);				
			} else {
				isValideMetaAssociationEnd = false;		
			}
	}
		
		Generator.logger.finest("isValideMetaAssociationEnd = " + isValideMetaAssociationEnd );
		
		
	}

	public boolean doNotRestrictConfidentiality(MegaObject oMetaAssociationEnd) {
		boolean isAlwaysVisible = false;
		String oIdAbs = oMetaAssociationEnd.getProp(StaticFields.absoluteIdentifier);		

		
		switch (oIdAbs) {  
		case "41000000CW30":  // Language SpecializedLanguage
			Generator.logger.finest("oMetaAssociationEnd = Language SpecializedLanguage is always available = " + oIdAbs );
			isAlwaysVisible = true;
		break;   						
		case "010000008W30":  // Language GeneralLanguage
			Generator.logger.finest("oMetaAssociationEnd = Language GeneralLanguage is always available = " + oIdAbs );
			isAlwaysVisible = true;
		break;
		default:  
			//
		} 
		
	
		return isAlwaysVisible;
		
	}	
	
	public boolean isValideMetaAssociationEnd() {
		return this.isValideMetaAssociationEnd;
	}	
	
	public static MegaObject getOppositeMetaAssociationEnd(MegaObject oMetaAssociationEnd) {
		MegaObject oOppositeMetaAssociationEnd = null;		
		MegaObject oAssociation = oMetaAssociationEnd.getCollection(StaticFields.metaAssociation).get(1);	
		MegaCollection oColMAE = oAssociation.getCollection(StaticFields.associationMetaAssociationEnd);	
		while (oColMAE.hasNext()) {
			MegaObject oMAE = oColMAE.next();	
			if (!oMAE.sameID(oMetaAssociationEnd.getID())) {
				oOppositeMetaAssociationEnd = oMAE;
			}
		}		
		return oOppositeMetaAssociationEnd;
	}
		
	public List<RelationshipsJSON> getRelationshipsJSONList() {
		return this.relationshipsJSONList;
	}
/*
	private void setName(String name, String oIdAbsMEA ) {
		this.name = name +"_"+oIdAbsMEA;
	}
	
	public String getName() {
		return this.name;
	}
*/
/*
	private void setNameErrorDisplay(String oIdAbsMEA, String technincalNameMetaClass, String targetMetaClassID) {
		this.nameErrorDisplay = name + " - oIdAbsMEA=" + oIdAbsMEA + " technincalNameMetaClass="+technincalNameMetaClass + " targetMetaClassID="+targetMetaClassID;  
	}		
	
	public String getNameErrorDisplay() {
		return this.nameErrorDisplay;
	}	
*/
	private void manageAssignment(boolean isFirst, boolean navigate, boolean nameForComplexPath, MegaObject oStartingMetaClass, MegaObject oMetaAssociationEnd, MegaObject oMetaAssociation, MegaObject oMetaClass) {
		MegaObject oMetaAssociationEndTarget = megaRoot.getObjectFromID("~hCr81RIpEvMH[Assigned Object]");
		MegaObject oMetaAssociationTarget = oMetaAssociationEndTarget.getCollection(StaticFields.metaAssociation).get(1);	

		MegaObject oMetaClassTarget = megaRoot.getObjectFromID("~BEr8mPIpEzHH[Assignment Object]");		
		
		MegaObject pMetaClass = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClassTarget);		
		MegaCollection pColSubMetaClass =pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.concretes);							

		while (pColSubMetaClass.hasNext()) {		
  			MegaObject pSubMetaclass = pColSubMetaClass.next();	      	      			
			MegaObject oSubMetaclass = megaRoot.getCollection(StaticFields.metaClassMetaClass).get(pSubMetaclass.megaField());
			boolean isAvailableMetaClass = (boolean) oSubMetaclass.callFunction("IsAvailable");			
			if (isAvailableMetaClass) {
				MegaObject metaClassInDiagram = oMetamodel.getCollection(StaticFields.systemDescription).get(1).getCollection(StaticFields.diagramMetaClass).get(oSubMetaclass);
				if (metaClassInDiagram.exists()) {
					MegaCollection oColAssignableRole= oSubMetaclass.getCollection(StaticFields.assignableRole);
					while (oColAssignableRole.hasNext()) {
						MegaObject oBusinessRole = oColAssignableRole.next();
						
						boolean isAvailable = (boolean) oBusinessRole.callFunction("IsAvailable");
						if (isAvailable) {					
							RelationshipsJSON relationshipsJSON = new RelationshipsJSON(overrideNameList,globalUniqueName);	
							relationshipsJSONList.add(relationshipsJSON);						
							setPropertiesPersonAssignment(relationshipsJSON, isFirst,navigate,nameForComplexPath,oStartingMetaClass,oMetaAssociationEnd,oMetaAssociation,oMetaClass,oBusinessRole,oMetaAssociationEndTarget,oMetaAssociationTarget,oSubMetaclass);
						}
					}
				} 
			} // if			
  		} // while

	}
		
	private void managePersonAssignment(boolean isFirst, boolean navigate, boolean nameForComplexPath, MegaObject oStartingMetaClass, MegaObject oMetaAssociationEnd, MegaObject oMetaAssociation, MegaObject oMetaClass) {
		MegaCollection oColAssignableRole= oStartingMetaClass.getCollection(StaticFields.assignableRole);
		MegaObject oMetaAssociationEndNext = megaRoot.getObjectFromID("~L2000000Ca80[Assigned Person]");
		MegaObject oMetaAssociationNext = oMetaAssociationEndNext.getCollection(StaticFields.metaAssociation).get(1);	
		MegaObject oMetaClassNext = megaRoot.getObjectFromID("~T20000000s10[Person <System>]");
		
		while (oColAssignableRole.hasNext()) {
			MegaObject oBusinessRole = oColAssignableRole.next();
			
			boolean isAvailable = (boolean) oBusinessRole.callFunction("IsAvailable");						
			if (isAvailable) {
				RelationshipsJSON relationshipsJSON = new RelationshipsJSON(overrideNameList,globalUniqueName);	
				relationshipsJSONList.add(relationshipsJSON);					
				setPropertiesPersonAssignment(relationshipsJSON, isFirst,navigate,nameForComplexPath,oStartingMetaClass,oMetaAssociationEnd,oMetaAssociation,oMetaClass,oBusinessRole,oMetaAssociationEndNext,oMetaAssociationNext, oMetaClassNext);
			}
		}
		
	}
	
	private void setPropertiesPersonAssignment(RelationshipsJSON relationshipsJSON, boolean isFirst, boolean navigate, boolean nameForComplexPath, MegaObject oStartingMetaClass, MegaObject oMetaAssociationEnd,MegaObject oMetaAssociation , MegaObject oMetaClass, MegaObject oBusinessRole, MegaObject oMetaAssociationEndNext, MegaObject oMetaAssociationNext ,MegaObject oMetaClassNext ) {
	
		String technincalName = UtilitiesMappingJSON.getTechnicalNameMetaAssociationEnd((oBusinessRole.getProp(StaticFields.shortName)));   		

		String technincalNameMEA = UtilitiesMappingJSON.getTechnicalNameMetaAssociationEnd((oMetaAssociationEnd.getProp(StaticFields.shortName)));   		
		String technincalNameMetaClass = UtilitiesMappingJSON.getTechnicalNameMetaClass((oMetaClass.getProp(StaticFields.shortName)));
		String targetMetaClassID= oMetaClass.getProp(StaticFields.absoluteIdentifier);
		String oIdAbsMEA = oMetaAssociationEnd.getProp(StaticFields.absoluteIdentifier);
		String sMetaMultiplicity = oMetaAssociationEnd.getProp(StaticFields.metaMultiplicity);		

		String technincalNameMA = UtilitiesMappingJSON.getTechnicalNameMetaAssociation((oMetaAssociation.getProp(StaticFields.shortName)));   		

		String technincalNameStartingMetaClass = UtilitiesMappingJSON.getTechnicalNameMetaClass((oStartingMetaClass.getProp(StaticFields.shortName)));
		String startingMetaClassId = (oStartingMetaClass.getProp(StaticFields.absoluteIdentifier));
		
		
		MegaObject metaAssociation = oMetaAssociationEnd.getCollection(StaticFields.metaAssociation).get(1);
		String id = metaAssociation.getProp(StaticFields.absoluteIdentifier);
		
		
		PathToTargetJSON pathToTargetJSON = new PathToTargetJSON(overrideNameList,oIdAbsMEA,targetMetaClassID);
		pathToTargetJSON.setId(id);
		pathToTargetJSON.setMaeID(oIdAbsMEA);
		pathToTargetJSON.setMetaClassID(targetMetaClassID);
		pathToTargetJSON.setMaeName(technincalNameMEA);
		pathToTargetJSON.setName(technincalNameMA);				
		pathToTargetJSON.setMetaClassName(technincalNameMetaClass);
		pathToTargetJSON.setMultiplicity(sMetaMultiplicity);
		
		relationshipsJSON.addPathToTargetJSON(pathToTargetJSON,startingMetaClassId, technincalNameStartingMetaClass,true,"","","");
		
		MegaObject oMetaAssociationEndCondition = megaRoot.getObjectFromID("~M2000000Ce80[Business Role]");
		MegaObject oMetaClassCondition = megaRoot.getObjectFromID("~230000000A40[Business Role]");

		String technincalNameMaeCondition = UtilitiesMappingJSON.getTechnicalNameMetaAssociationEnd((oMetaAssociationEndCondition.getProp(StaticFields.shortName)));   		
		String technincalNameMetaClassCondition = UtilitiesMappingJSON.getTechnicalNameMetaClass((oMetaClassCondition.getProp(StaticFields.shortName)));
		String targetMetaClassIDCondition= oMetaClassCondition.getProp(StaticFields.absoluteIdentifier);
		String oIdAbsMEACondition = oMetaAssociationEndCondition.getProp(StaticFields.absoluteIdentifier);
		String sMetaMultiplicityCondition = oMetaAssociationEndCondition.getProp(StaticFields.metaMultiplicity);		

		String technincalNameMaCondition = UtilitiesMappingJSON.getTechnicalNameMetaAssociation((oMetaAssociationEndCondition.getProp(StaticFields.shortName)));   		
		
		MegaObject metaAssociationCondition = oMetaAssociationEndCondition.getCollection(StaticFields.metaAssociation).get(1);
		String idCondition = metaAssociationCondition.getProp(StaticFields.absoluteIdentifier);		
		
		String objectFilterShortName = oBusinessRole.getProp(StaticFields.shortName);
		String objectFilterID = oBusinessRole.getProp(StaticFields.absoluteIdentifier);

		// specific section to add in the JSON the condition for the business role
		FilterMaeJSON filterMaeJSON = new FilterMaeJSON(overrideNameList,oIdAbsMEACondition,targetMetaClassIDCondition);
		
		filterMaeJSON.setId(idCondition);
		filterMaeJSON.setMaeID(oIdAbsMEACondition);
		filterMaeJSON.setMetaClassID(targetMetaClassIDCondition);
		filterMaeJSON.setMaeName(technincalNameMaeCondition);
		filterMaeJSON.setName(technincalNameMaCondition);
		filterMaeJSON.setMetaClassName(technincalNameMetaClassCondition);
		filterMaeJSON.setMultiplicity(sMetaMultiplicityCondition);

		
		filterMaeJSON.setObjectFilterShortName(objectFilterShortName);
		filterMaeJSON.setObjectFilterID(objectFilterID);;
			
		pathToTargetJSON.setCondition(filterMaeJSON);
/*		
		setName(technincalNameMEA.toString(),oIdAbsMEA);     
		setNameErrorDisplay(oIdAbsMEA, technincalNameMetaClass, targetMetaClassID);
*/
		
		
		
		String technincalNameMaeNext = UtilitiesMappingJSON.getTechnicalNameMetaAssociationEnd((oMetaAssociationEndNext.getProp(StaticFields.shortName)));   		
		String technincalNameMetaClassNext = UtilitiesMappingJSON.getTechnicalNameMetaClass((oMetaClassNext.getProp(StaticFields.shortName)));
		String targetMetaClassIDNext= oMetaClassNext.getProp(StaticFields.absoluteIdentifier);
		String oIdAbsMEANext = oMetaAssociationEndNext.getProp(StaticFields.absoluteIdentifier);
		String sMetaMultiplicityNext = oMetaAssociationEndNext.getProp(StaticFields.metaMultiplicity);		

		String technincalNameMaNext = UtilitiesMappingJSON.getTechnicalNameMetaAssociation((oMetaAssociationNext.getProp(StaticFields.shortName)));   		

		
		MegaObject metaAssociationNext = oMetaAssociationEndNext.getCollection(StaticFields.metaAssociation).get(1);
		String idNext = metaAssociationNext.getProp(StaticFields.absoluteIdentifier);
				
		PathToTargetJSON pathToTargetJSONNext = new PathToTargetJSON(overrideNameList,oIdAbsMEANext,targetMetaClassIDNext);
		pathToTargetJSONNext.setId(idNext);
		pathToTargetJSONNext.setMaeID(oIdAbsMEANext);
		pathToTargetJSONNext.setMetaClassID(targetMetaClassIDNext);
		pathToTargetJSONNext.setMaeName(technincalNameMaeNext);
		pathToTargetJSONNext.setName(technincalNameMaNext);		
		pathToTargetJSONNext.setMetaClassName(technincalNameMetaClassNext);
		pathToTargetJSONNext.setMultiplicity(sMetaMultiplicityNext);
			
		//override name
		technincalName = technincalName + "_" + technincalNameMetaClassNext;

	
		if (overrideNameList.containsKey(objectFilterID)) {
			technincalName = overrideNameList.get(objectFilterID)+ "_" + technincalNameMetaClassNext;
		}

		String convertedObjectFilterShortName = UtilitiesMappingJSON.getTechnicalNameMetaClass(objectFilterShortName);
		

		
		relationshipsJSON.addPathToTargetJSON(pathToTargetJSONNext,startingMetaClassId,technincalNameStartingMetaClass,false,technincalNameMetaClassCondition  , technincalNameMaeCondition, convertedObjectFilterShortName);


		//String idrelationshipsJSON = targetMetaClassIDNext + "_" + oIdAbsMEANext + "_" + targetMetaClassID + "_" + oIdAbsMEA + "_" + targetMetaClassIDCondition + "_" + oIdAbsMEACondition + "_" + objectFilterID;

		
//		String idrelationshipsJSON = id +"_"+ idNext + "_" + idCondition+ "_" + objectFilterID;
		
		//relationshipsJSON.setId(idrelationshipsJSON);
		relationshipsJSON.setName(technincalName);	
		
	}
	
	
	

	private void setProperties(RelationshipsJSON relationshipsJSON, boolean isFirst, boolean navigate, boolean nameForComplexPath, MegaObject oStartingMetaClass, MegaObject oMetaAssociationEnd, MegaObject oMetaClass, MegaObject oMetaAssociation ) {

		MegaCollection oColMetaAttribute = oMetaAssociation.getCollection(StaticFields.metaAssociationMetaAttribute);
	
		String technincalNameMEA = UtilitiesMappingJSON.getTechnicalNameMetaAssociationEnd((oMetaAssociationEnd.getProp(StaticFields.shortName)));   		
		String technincalNameMetaClass = UtilitiesMappingJSON.getTechnicalNameMetaClass((oMetaClass.getProp(StaticFields.shortName)));
		
		String technincalNameMA = UtilitiesMappingJSON.getTechnicalNameMetaAssociation((oMetaAssociation.getProp(StaticFields.shortName)));   		
		String targetMetaClassID= oMetaClass.getProp(StaticFields.absoluteIdentifier);
		
		String oIdAbsMEA = oMetaAssociationEnd.getProp(StaticFields.absoluteIdentifier);
		String sMetaMultiplicity = oMetaAssociationEnd.getProp(StaticFields.metaMultiplicity);
		
		MegaObject metaAssociation = oMetaAssociationEnd.getCollection(StaticFields.metaAssociation).get(1);
		String id = metaAssociation.getProp(StaticFields.absoluteIdentifier);

		String technincalNameStartingMetaClass = UtilitiesMappingJSON.getTechnicalNameMetaClass((oStartingMetaClass.getProp(StaticFields.shortName)));
		String startingMetaClassId = (oStartingMetaClass.getProp(StaticFields.absoluteIdentifier));

		
		//String idMetaClassSource = oStartingMetaClass.getProp(StaticFields.absoluteIdentifier);
		
		
		PathToTargetJSON pathToTargetJSON = new PathToTargetJSON(overrideNameList,oIdAbsMEA,targetMetaClassID);	
		List<PropertiesJSON> properties = setMetaAttribute(oColMetaAttribute);
		pathToTargetJSON.setProperties(properties);
		pathToTargetJSON.setId(id);		
		pathToTargetJSON.setMaeID(oIdAbsMEA);
		pathToTargetJSON.setMetaClassID(targetMetaClassID);
		pathToTargetJSON.setMaeName(technincalNameMEA);
		pathToTargetJSON.setName(technincalNameMA);
		pathToTargetJSON.setMetaClassName(technincalNameMetaClass);
		pathToTargetJSON.setMultiplicity(sMetaMultiplicity);

		if (!isFirst) {
			technincalNameMEA  = technincalNameMetaClass + "_" + technincalNameMEA;
		} 
		if (nameForComplexPath) {
			technincalNameMEA  = technincalNameMetaClass + "_" + technincalNameMEA;
		}

				
		relationshipsJSON.setId(id);
		relationshipsJSON.addPathToTargetJSON(pathToTargetJSON,startingMetaClassId,technincalNameStartingMetaClass,false,"","","");
		
		boolean navigateTheIntermediateObject = MetaClass.isFeaturedMetaClass(megaRoot, oMetaClass);

		
		// dans le cas d'un business document
		// si navigateTheIntermediateObject = true il faut ignorer toute la relationship
		// et positionner la variable isValideMetaAssociationEnd = false
				
		boolean ignorePath = ignoreSubMetaClassFeaturedForPath(oMetaClass, oStartingMetaClass);		
		
		if (ignorePath) {
			isValideMetaAssociationEnd = false;
		} else {
		
			if (navigateTheIntermediateObject && navigate) {
				setPathtoTarget(relationshipsJSON, isFirst, oStartingMetaClass,oMetaAssociationEnd, oMetaClass);			
			} // if

		}

        
	} // setProperties
	
	// TO DO Manage risk of duplicate name in 	attribute
	private List<PropertiesJSON> setMetaAttribute(MegaCollection oColMetaAttribute) {
		List<PropertiesJSON> propertiesJSONList = null;
		// we remove some elements that we don't want // TO DO : is it smart the way it is done ?

		MegaCollection oColMetaAttributeLocal = MetaAttribute.cleanCollectionMetaAttributeAssociation(megaRoot, oColMetaAttribute);
		
		if (oColMetaAttributeLocal.size()> 0) {
			propertiesJSONList = new ArrayList<PropertiesJSON>();		
			
			while(oColMetaAttributeLocal.hasNext()) {
				MegaObject oMetaAttribute = oColMetaAttributeLocal.next();
				MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,oMetaAttribute, overrideNameList);
				if (metaAttribute.getIsValideMetaAttribute()) {				
					PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
					propertiesJSONList.add(propertiesJSON);				
				} // if valid			

			} //while			

			MetaModel.reportDuplicateName("MetaAttribute Relationship", propertiesJSONList, ensureUniqueName);						
		}
			
		
		return propertiesJSONList;		
	}	
	
	
/*	
	private boolean navigateTheIntermediateObject(MegaObject oMetaClass ) {

		boolean result = MetaClass.isFeaturedMetaClass(megaRoot,oMetaClass);
			
		MegaCollection oCol = MetaClass.getCollectionOfObjectNotFeature(megaRoot); 
		while(oCol.hasNext()) {
			MegaObject oObj = oCol.next();
			if (oObj.sameID(oMetaClass.getID())) {
				result = false;
			}
		}
		
		return result;
	}
*/	
	
	private boolean ignoreSubMetaClassFeaturedForPath(MegaObject oMetaClass1, MegaObject oMetaClass2) {
		boolean result = false;		
		// we test if it's a feature MetaClass
		// as we test both direction we do the same rule in both
		if (MetaClass.isFeaturedMetaClass(megaRoot,oMetaClass1)) {			
			MegaCollection oCol = MetaClass.getCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass(megaRoot); 
			while(oCol.hasNext()) {
				MegaObject oObj = oCol.next();
				if (oObj.sameID(oMetaClass2.getID())) {
					return result = true;					
				}
			}
		}
		if (MetaClass.isFeaturedMetaClass(megaRoot,oMetaClass2)) {			
			MegaCollection oCol = MetaClass.getCollectionRootObjectThatShouldNotBeLinkedToFeatureMetaclass(megaRoot); 
			while(oCol.hasNext()) {
				MegaObject oObj = oCol.next();
				if (oObj.sameID(oMetaClass1.getID())) {
					return result = true;					
				}
			}
		}		
		return result;	
	}
	

	private void setPathtoTarget(RelationshipsJSON relationshipsJSON, boolean isFirst, MegaObject oStartingMetaClass,MegaObject oMetaAssociationEnd,MegaObject oMetaClass) {

		MegaCollection oCol = megaRoot.getSelection("");

		MegaObject pMetaClass = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClass);		
		MegaCollection pColMetaAssociationEnd = pMetaClass.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.pCollections);
		
		//MegaCollection oColTargetMetaAssociationEnd = oMetaClass.getCollection(StaticFields.metaOppositeAssociationEnd);		
		// we loop and take only the on in the diagram
		// we avoid the one we come from
		
		while (pColMetaAssociationEnd.hasNext()) {
			MegaObject pTargetMetaAssociationEnd = pColMetaAssociationEnd.next();
			MegaObject oTargetMetaAssociationEnd = megaRoot.getCollection(StaticFields.metaAssociationEnd).get(pTargetMetaAssociationEnd.megaField());

			// we filter only metaAssociation end in the diagram.
			MegaObject meaInDiagram = oMetamodel.getCollection(StaticFields.systemDescription).get(1).getCollection(StaticFields.diagramMetaAssociationEnd).get(oTargetMetaAssociationEnd);
			boolean isAvailableMEA = (boolean) oTargetMetaAssociationEnd.callFunction("IsAvailable");
			if (isAvailableMEA) {
				if (meaInDiagram.exists()) {
					// if this is not the path we come from		
					MegaObject oOppositeMetaAssociationEnd = MetaAssociationEnd.getOppositeMetaAssociationEnd(oMetaAssociationEnd);
					if (!oOppositeMetaAssociationEnd.sameID(oTargetMetaAssociationEnd.getID())) {
		
						// not optimize to do that in the loop
						MegaObject maeDescription = megaRoot.getObjectFromID(StaticFields.maedescription);	
						if (!oTargetMetaAssociationEnd.sameID(maeDescription.getID())) {
							oCol.insert(oTargetMetaAssociationEnd);							
						}
						

					}			
				} // if in diagram
			}
			
		} // while

		
		// we have a collection of MetaAssocitionEnd
		// we need to build a list of couple MAE+MetaClass
		// in case the MAE is linked to an abstract we need to get the concrete.
		
		RelationshipsJSON relationshipsJSONFirst = new RelationshipsJSON(relationshipsJSON,overrideNameList,globalUniqueName);


		int i;
		int size = oCol.size();
		boolean isFirstTimeInLoop = true;
		for (i=1; i <= size;i++) {
			
			MegaObject oTargetMetaAssociationEnd = oCol.get(i);
  			MegaObject oMetaAssociation = oTargetMetaAssociationEnd.getCollection(StaticFields.metaAssociation).get(1);
			
			MegaObject oTargetMetaClass = oTargetMetaAssociationEnd.getCollection(StaticFields.maeMetaClass).get(1);
			boolean navigate = false;
			boolean nameForComplexPath = true;

			boolean isAvailableMEA = (boolean) oTargetMetaAssociationEnd.callFunction("IsAvailable");
			boolean isAvailableMetaClass = (boolean) oTargetMetaClass.callFunction("IsAvailable");			

			// this ignorePath is to remove the case of intermediate object linked to business document.
			boolean ignorePath = ignoreSubMetaClassFeaturedForPath(oMetaClass, oTargetMetaClass);

			//System.out.println("ignorePath= " + ignorePath);
					
			if (!ignorePath) {
				if (isAvailableMEA && isAvailableMetaClass) {
					RelationshipsJSON relationshipsJSONToTake = null;										
					String sMetaLayer = oTargetMetaClass.getProp(StaticFields.metaLayer,"internal").toString();

				  	// the target is an abstract metaclass
					// we need to look for the concrete
					MegaCollection oColConcrete = megaRoot.getSelection("");
					
					if (!sMetaLayer.equals("30")) {		
						// if abstract
						MegaObject pMetaClassLocal = megaRoot.getCollection(StaticFields.pMetaClass).get(oTargetMetaClass);		
						MegaCollection oColSubMetaClass =pMetaClassLocal.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.concretes);							
			      		while (oColSubMetaClass.hasNext()) {
			      			MegaObject oSubMetaclass = oColSubMetaClass.next();	 
			      			
			    			boolean isAvailableMetaClassSub = (boolean) oSubMetaclass.callFunction("IsAvailable");			
			      			if (isAvailableMetaClassSub) {
				      			MegaObject metaClassInDiagram = oMetamodel.getCollection(StaticFields.systemDescription).get(1).getCollection(StaticFields.diagramMetaClass).get(oSubMetaclass);	      			
				      			if (metaClassInDiagram.exists()) {
				      				oColConcrete.insert(oSubMetaclass);
				      			}	
			      			}
			      		} //while
			      		
						int sizej = oColConcrete.size();		
			    		for (int j=1; j <= sizej;j++) {
			      			MegaObject oTargetMetaClassToTake = oColConcrete.get(j);
		      			
			    			if (isFirstTimeInLoop) {
			    				relationshipsJSONToTake = relationshipsJSON;
			    			} else {
			    				RelationshipsJSON relationshipsJSONNew = new RelationshipsJSON(relationshipsJSONFirst,overrideNameList,globalUniqueName);
			    				relationshipsJSONList.add(relationshipsJSONNew);								
			    				relationshipsJSONToTake = relationshipsJSONNew;
			    			} // if	
		
							setProperties(relationshipsJSONToTake, isFirst, navigate , nameForComplexPath, oStartingMetaClass,oTargetMetaAssociationEnd, oTargetMetaClassToTake, oMetaAssociation);											
							isFirstTimeInLoop = false;
			    		} // for				
			      		
			      		
			      		
					} else {
						// if concrete
						if (isFirstTimeInLoop) {
							relationshipsJSONToTake = relationshipsJSON;
						} else {
							RelationshipsJSON relationshipsJSONNew = new RelationshipsJSON(relationshipsJSONFirst,overrideNameList,globalUniqueName);
							relationshipsJSONList.add(relationshipsJSONNew);								
							relationshipsJSONToTake = relationshipsJSONNew;
						} // if					
						
						setProperties(relationshipsJSONToTake, isFirst, navigate , nameForComplexPath,oStartingMetaClass, oTargetMetaAssociationEnd, oTargetMetaClass,oMetaAssociation);																
						isFirstTimeInLoop = false;
					} // if abstract or concrete
	
				} // if available
			} // if ignorePath			
		} // for

	}


	
	
}
