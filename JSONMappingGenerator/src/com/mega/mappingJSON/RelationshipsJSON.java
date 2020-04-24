package com.mega.mappingJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

public class RelationshipsJSON  extends CommonFields {

	private String name;
	private String globalUniqueName;
	private ConstraintsRelationShip constraints = new ConstraintsRelationShip();			
	
	private ArrayList<PathToTargetJSON> pathToTarget = new ArrayList<PathToTargetJSON>();
	private HashMap<String,String> globalUniqueNameHashMap;
	
	public 	RelationshipsJSON(HashMap<String,String> overrideNameList,HashMap<String,String> globalUniqueNameHashMap) {	
		this.overrideNameList = overrideNameList;
		this.globalUniqueNameHashMap = globalUniqueNameHashMap;
	}

	public 	RelationshipsJSON(RelationshipsJSON relationshipsJSON, HashMap<String,String> overrideNameList,HashMap<String,String> globalUniqueNameHashMap) {
		this.overrideNameList = overrideNameList;
		this.globalUniqueNameHashMap = globalUniqueNameHashMap;
		this.name = relationshipsJSON.getName() +"";
		this.id = relationshipsJSON.getId()+"";
		this.globalUniqueName = relationshipsJSON.getGlobalUniqueName()+"";
		List<PathToTargetJSON> initialPathToTargetList = relationshipsJSON.getPathToTarget();
		Iterator<PathToTargetJSON> iterator = initialPathToTargetList.iterator();
		while (iterator.hasNext()) {
			PathToTargetJSON initialPathToTarget = iterator.next();
			pathToTarget.add(new PathToTargetJSON(initialPathToTarget,overrideNameList));			
		}	
		ConstraintsRelationShip initialconstraints = relationshipsJSON.getConstraints();
		constraints.setReadOnly(initialconstraints.getReadOnly());
		
		
	}		
	
	public void setName(String name) {
		this.name = getRealName(name);
	}

	public String getName() {
		return this.name;				
	}
		
	public void setGlobalUniqueName(String globalUniqueName) {
		this.globalUniqueName = globalUniqueName;
	}

	public String getGlobalUniqueName() {
		return this.globalUniqueName;				
	}	
	
	
	public void setPathToTarget(ArrayList<PathToTargetJSON> pathToTarget) {
		this.pathToTarget = pathToTarget;
	}
	
	public List<PathToTargetJSON> getPathToTarget() {
		return this.pathToTarget;
	}

	public void addPathToTargetJSON(PathToTargetJSON pathToTargetJSON, String metaClassSourceName, boolean ignoreUniqueName, String   conditionMetaClassName  , String conditionMaeName, String conditionObjectFilter) {
		this.pathToTarget.add(pathToTargetJSON);
		updateName("",metaClassSourceName, ignoreUniqueName,  conditionMetaClassName  , conditionMaeName, conditionObjectFilter);
	}

	public void setConstraints(ConstraintsRelationShip constraints) {
		this.constraints = constraints;
	}

	public ConstraintsRelationShip getConstraints() {
		return this.constraints;
	}	
	
	
	private void updateName(String name, String metaClassSourceName, boolean ignoreUniqueName, String conditionMetaClassName, String conditionMaeName, String conditionObjectFilter) {
		String localName;
		String localId;
		String globalUniqueNameLocal;
		
		if (pathToTarget != null) {
			int sizePath = pathToTarget.size();			
			String keyID="";
			
			if ( sizePath == 1 ) {
				PathToTargetJSON pathToTargetJSON1 =pathToTarget.get(0);				
				localName = pathToTargetJSON1.getMetaClassName() + "_"+ pathToTargetJSON1.getMaeName();	
				
				if (pathToTargetJSON1.getMetaClassName().equals(pathToTargetJSON1.getMaeName())) {
					localName = pathToTargetJSON1.getMetaClassName();
				}
				
				keyID = pathToTarget.get(0).getMaeID() +"_"+ pathToTarget.get(0).getMetaClassID();
				if (overrideNameList.containsKey(keyID)) {
					localName = overrideNameList.get(keyID);
				} else {
					//overrideNameList.put(keyID, localName);
				}				

				if (!ignoreUniqueName) {

					if (conditionObjectFilter.equals("")) {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" + metaClassSourceName;								
						localId = pathToTargetJSON1.getMetaClassID()+"_"+ pathToTargetJSON1.getMaeID();
						this.id = localId;
					
					} else {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" + metaClassSourceName+ "_" + conditionMetaClassName  + "_" + conditionMaeName + "_" + conditionObjectFilter;													
					}
				
					if (this.globalUniqueNameHashMap.containsKey(globalUniqueNameLocal)) {
						System.out.println("Error 1 globalUniqueName already exist" + globalUniqueNameLocal);
						setGlobalUniqueName("Error 1 Not unique name" + globalUniqueNameLocal);
					} else {
						this.globalUniqueNameHashMap.put(globalUniqueNameLocal, globalUniqueNameLocal);					
						setGlobalUniqueName(globalUniqueNameLocal);
					}					
				}
				
				//overrideNameList.put(keyID, localName);
				
			} else if (sizePath == 2) {
				PathToTargetJSON pathToTargetJSON1 =pathToTarget.get(0);				
				PathToTargetJSON pathToTargetJSON2 =pathToTarget.get(1);	
				
				
				// we remove link attribute for now for complex path
				pathToTargetJSON1.setProperties(null);
				pathToTargetJSON2.setProperties(null);
				
				
				localName = pathToTargetJSON2.getMetaClassName() + "_"+ pathToTargetJSON2.getMaeName()+ "_"+ pathToTargetJSON1.getMetaClassName()+"_"+ pathToTargetJSON1.getMaeName();					
				
				localId = pathToTargetJSON2.getMetaClassID() + "_"+ pathToTargetJSON2.getMaeID()+ "_"+ pathToTargetJSON1.getMetaClassID()+"_"+ pathToTargetJSON1.getMaeID();
				
				keyID = pathToTarget.get(0).getMaeID() +"_"+ pathToTarget.get(0).getMetaClassID() +"_"+ pathToTarget.get(1).getMaeID() +"_"+ pathToTarget.get(1).getMetaClassID();
				if (overrideNameList.containsKey(keyID)) {
					localName = overrideNameList.get(keyID);
				} else {
					//overrideNameList.put(keyID, localName);
				}			
						
				this.id = localId;
								
				// 2 case
				// case with 2 path
				// case with 2 path and a condition
				if (!ignoreUniqueName) {					
					if (conditionObjectFilter.equals("")) {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON2.getMetaClassName() +  "_" + pathToTargetJSON2.getMaeName() + "_" + pathToTargetJSON2.getName() + "_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() +  "_" +  metaClassSourceName;								
					} else {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON2.getMetaClassName() +  "_" + pathToTargetJSON2.getMaeName() + "_" + pathToTargetJSON2.getName() + "_" +  pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" +  metaClassSourceName + "_" + conditionMetaClassName  + "_" + conditionMaeName + "_" + conditionObjectFilter;											
					}
					if (this.globalUniqueNameHashMap.containsKey(globalUniqueNameLocal)) {
						System.out.println("Error 2 globalUniqueName already exist" + globalUniqueNameLocal);
						setGlobalUniqueName("Error 2 Not unique name :" + globalUniqueNameLocal);
					} else {
						this.globalUniqueNameHashMap.put(globalUniqueNameLocal, globalUniqueNameLocal);					
						setGlobalUniqueName(globalUniqueNameLocal);
					}

				} 
				
				//overrideNameList.put(keyID, localName);
			
				
				
			} else if (sizePath >2 ) {
				localName = "Error";
				System.out.println("The path contains more than 2 steps " +name);
			}  else {
				localName = name;
			}

			if (overrideNameList.containsKey(this.id)) {
				localName = overrideNameList.get(keyID);
			} else {
				overrideNameList.put(keyID, localName);
			}

			
			setName(localName);
			
			//this.name =localName;
		} // if
	
	} // updateName
/*
	public void setImplementInterface (String implementInterface) {
		this.implementInterface = implementInterface;
	}
	
	public String getImplementInterface() {
		return this.implementInterface;
	}	
	*/	
	
}
