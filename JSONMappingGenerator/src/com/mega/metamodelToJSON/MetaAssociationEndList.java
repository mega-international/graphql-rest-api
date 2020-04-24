package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.mega.mappingJSON.RelationshipsJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaAssociationEndList extends CommonAttributes {
		
	private List<RelationshipsJSON> relationshipsJSONList;

	private	MegaObject oStartingMetaClass;
	private MegaObject oInitialTargetMetaClass;
	private MegaObject oInitialMetaAssociationEnd;
	private boolean isValideMetaAssociationEnd;

	private HashMap<String,String> metaAssociationEndHashMap = new HashMap<String,String>();	

	private HashMap<String,String> globalUniqueName;
	

	public MetaAssociationEndList(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oStartingMetaClass, MegaObject pInitialMetaAssociationEnd, HashMap<String,String> overrideNameList,HashMap<String,String> globalUniqueName) {
		this.megaRoot = megaRoot;
		this.oMetamodel = oMetamodel;
		this.oStartingMetaClass = oStartingMetaClass;
		this.globalUniqueName = globalUniqueName;

		this.overrideNameList= overrideNameList;
		this.oInitialMetaAssociationEnd = megaRoot.getCollection(StaticFields.metaAssociationEnd).get(pInitialMetaAssociationEnd.megaField());

		this.relationshipsJSONList = new ArrayList<RelationshipsJSON>();
		
		// we filter only metaAssociation end in the diagram.
		MegaObject meaInDiagram = oMetamodel.getCollection(StaticFields.systemDescription).get(1).getCollection(StaticFields.diagramMetaAssociationEnd).get(oInitialMetaAssociationEnd);

		
		boolean isAvailableMEA = (boolean) oInitialMetaAssociationEnd.callFunction("IsAvailable");
		boolean isAvailableMetaClass = (boolean) oStartingMetaClass.callFunction("IsAvailable");
		
		if (isAvailableMEA && isAvailableMetaClass) {	
			if (meaInDiagram.exists()) {
				isValideMetaAssociationEnd = true;
				this.oInitialTargetMetaClass = oInitialMetaAssociationEnd.getCollection(StaticFields.metaAssociationEndMetaClass).get(1);
		
				boolean isFirst = true;
				subMetaClassConcrete(oInitialMetaAssociationEnd,oInitialTargetMetaClass,isFirst);      
	
			} else {
				isValideMetaAssociationEnd = false;
			}
		} else {
			isValideMetaAssociationEnd = false;
		}
		
	} // constructor
	
	public boolean isValideMetaAssociationEnd() {
		return this.isValideMetaAssociationEnd;
	}
	
	public List<RelationshipsJSON> getRelationshipsJSONList() {
		return relationshipsJSONList;
	}
	
	public Map<String,String> getMetaAssociationEndHashMap() {
		return this.metaAssociationEndHashMap;
	}
	
	// recursive function
	private void subMetaClassConcrete(MegaObject oMetaAssociationEnd, MegaObject oMetaClass, boolean isFirst) {
/*
		' 2 cases : either oMetaClassTarget is concrete or abstract
		' case concrete then we write the info
		' case abstract : we loop on all sub concrete metaclass
*/	
		
		MegaObject metaClassInDiagram = oMetamodel.getCollection(StaticFields.systemDescription).get(1).getCollection(StaticFields.diagramMetaClass).get(oMetaClass);
	
		if (metaClassInDiagram.exists()) {
			isValideMetaAssociationEnd = true;			
			String sMetaLayer = oMetaClass.getProp(StaticFields.metaLayer,"internal").toString();
	  	
			//concrete
			if (sMetaLayer.equals("30") || sMetaLayer.equals("20")) {
				MegaObject oMetaAssociation = oMetaAssociationEnd.getCollection(StaticFields.metaAssociation).get(1);

				MetaAssociationEnd metaAssociationEnd = new MetaAssociationEnd(megaRoot, oMetamodel, oStartingMetaClass, oInitialTargetMetaClass, oInitialMetaAssociationEnd, oMetaAssociationEnd , oMetaClass, oMetaAssociation,isFirst, overrideNameList,globalUniqueName);				
				
				if (metaAssociationEnd.isValideMetaAssociationEnd()) {
					String name = metaAssociationEnd.getName();
					String nameerror = metaAssociationEnd.getNameErrorDisplay();				
					boolean alreadyExist = metaAssociationEndHashMap.containsKey(name);
					// when we navigate down in the abstraction there can be several path that goes to the same physical metaClass.
					// to avoid duplicate in the JSON we take the first one we found
					if (!alreadyExist) {
						metaAssociationEndHashMap.put(name, nameerror);			
						List<RelationshipsJSON> localRelationshipsJSONList = metaAssociationEnd.getRelationshipsJSONList();	
						
						relationshipsJSONList.addAll(localRelationshipsJSONList);
					} else  {
						System.out.println("Error - MetaAssociationEnd name already exist : " + nameerror);
						
					}					
				} // isValid				
				
			} else {
				isFirst = false;
			// abstract	
				MegaObject pMetaClassLocal = megaRoot.getCollection(StaticFields.pMetaClass).get(oMetaClass);		
				MegaCollection pColSubMetaClass =pMetaClassLocal.getCollection(StaticFields.pDescription).get(1).getCollection(StaticFields.concretes);							

				//MegaCollection oColSubMetaClass = oMetaClass.getCollection(StaticFields.subMetaClass);
	   				
	      		while (pColSubMetaClass.hasNext()) {		
	      			MegaObject pSubMetaclass = pColSubMetaClass.next();	      	      			
	    			MegaObject oSubMetaclass = megaRoot.getCollection(StaticFields.metaClassMetaClass).get(pSubMetaclass.megaField());
	      			
	    			boolean isAvailable = (boolean) oSubMetaclass.callFunction("IsAvailable");
	    			if (isAvailable) {	
		    			subMetaClassConcrete(oInitialMetaAssociationEnd,oSubMetaclass,isFirst);    	    				
	    			}
	      		} //while
			} // if
			
			
		} // if		
		
		
	} // subMetaClassConcrete
	
	
	
	
}
