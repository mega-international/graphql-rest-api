package com.mega.mappingJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import com.mega.generator.Generator;


public class RelationshipsJSON  extends CommonFields {

//	private String name;
	

	private String reverseId;
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
/*	
	public void setName(String name) {
		this.name = getRealName(name);
	}

	public String getName() {
		return this.name;				
	}
*/	
		
	
	public void setGlobalUniqueName(String globalUniqueName) {
		this.globalUniqueName = globalUniqueName;
	}

	public String getGlobalUniqueName() {
		return this.globalUniqueName;				
	}	
	
	
	public void setReverseId(String reverseId) {
		this.reverseId = reverseId;
	}

	public String getReverseId() {
		return this.reverseId;				
	}		
	
	
	
	public void setPathToTarget(ArrayList<PathToTargetJSON> pathToTarget) {
		this.pathToTarget = pathToTarget;
	}
	
	public List<PathToTargetJSON> getPathToTarget() {
		return this.pathToTarget;
	}

	public void addPathToTargetJSON(PathToTargetJSON pathToTargetJSON, String metaClassSourceId, String metaClassSourceName, boolean ignoreUniqueName, String   conditionMetaClassName  , String conditionMaeName, String conditionObjectFilter) {
		this.pathToTarget.add(pathToTargetJSON);
		updateName(metaClassSourceId, metaClassSourceName, ignoreUniqueName,  conditionMetaClassName  , conditionMaeName, conditionObjectFilter);
	}

	public void setConstraints(ConstraintsRelationShip constraints) {
		this.constraints = constraints;
	}

	public ConstraintsRelationShip getConstraints() {
		return this.constraints;
	}	
	
	
	private void updateName(String metaClassSourceId, String metaClassSourceName, boolean ignoreUniqueName, String conditionMetaClassName, String conditionMaeName, String conditionObjectFilter) {
		String localName;
		String localId;
		String globalUniqueNameLocal = null;
		
		if (pathToTarget != null) {
			int sizePath = pathToTarget.size();			
			String keyID="";
			
			if ( sizePath == 1 ) {
				PathToTargetJSON pathToTargetJSON1 =pathToTarget.get(0);				
				localName = pathToTargetJSON1.getMetaClassName() + "_"+ pathToTargetJSON1.getMaeName();	

				localId = pathToTargetJSON1.getMetaClassID()+"_"+ pathToTargetJSON1.getMaeID() + "_" + metaClassSourceId;
				this.id = localId;				
				
				if (pathToTargetJSON1.getMetaClassName().equals(pathToTargetJSON1.getMaeName())) {
					localName = pathToTargetJSON1.getMetaClassName();
				}
				
				keyID = pathToTarget.get(0).getMaeID() +"_"+ pathToTarget.get(0).getMetaClassID();
				if (overrideNameList.containsKey(keyID)) {
					localName = overrideNameList.get(keyID);
				}			

				if (!ignoreUniqueName) {

					
					if (conditionObjectFilter.equals("")) {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" + metaClassSourceName;															
					} else {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" + metaClassSourceName+ "_" + conditionMetaClassName  + "_" + conditionMaeName + "_" + conditionObjectFilter;													
						this.id = this.id + "_" + pathToTargetJSON1.getCondition().getMetaClassID() + "_" + pathToTargetJSON1.getCondition().getMaeID() + "_" + pathToTargetJSON1.getCondition().getObjectFilterID();
					}
				
					if (this.globalUniqueNameHashMap.containsKey(globalUniqueNameLocal)) {
						Generator.logger.severe("Error 1 globalUniqueName already exist = " + globalUniqueNameLocal );
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

				localId = pathToTargetJSON2.getMetaClassID() + "_"+ pathToTargetJSON2.getMaeID()+ "_"+ pathToTargetJSON1.getMetaClassID()+"_"+ pathToTargetJSON1.getMaeID()+ "_" + metaClassSourceId;
				this.id = localId;
								
				//pathToTargetJSON1.setProperties(null);
				// we remove link attribute for now for complex path on second MetaAssociation
				pathToTargetJSON2.setProperties(null);
				
				
				localName = pathToTargetJSON2.getMetaClassName() + "_"+ pathToTargetJSON2.getMaeName()+ "_"+ pathToTargetJSON1.getMetaClassName()+"_"+ pathToTargetJSON1.getMaeName();					
				
				//specfic namming convention for MetaAssociation with Variation
				String oMAId = pathToTargetJSON1.getId();
				if (oMAId.contentEquals("qXAXOUeI6b90") || oMAId.contentEquals("KWAXSxeI6vE0")) {					
					localName = pathToTargetJSON2.getMetaClassName() + "_"+ pathToTargetJSON2.getMaeName();
				}
				
				
				keyID = pathToTarget.get(0).getMaeID() +"_"+ pathToTarget.get(0).getMetaClassID() +"_"+ pathToTarget.get(1).getMaeID() +"_"+ pathToTarget.get(1).getMetaClassID();
				if (overrideNameList.containsKey(keyID)) {
					localName = overrideNameList.get(keyID);
				}		
						
				// 2 case
				// case with 2 path
				// case with 2 path and a condition
				if (!ignoreUniqueName) {					
					if (conditionObjectFilter.equals("")) {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON2.getMetaClassName() +  "_" + pathToTargetJSON2.getMaeName() + "_" + pathToTargetJSON2.getName() + "_" + pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() +  "_" +  metaClassSourceName;								
					} else {
						globalUniqueNameLocal = "Relationship_" + pathToTargetJSON2.getMetaClassName() +  "_" + pathToTargetJSON2.getMaeName() + "_" + pathToTargetJSON2.getName() + "_" +  pathToTargetJSON1.getMetaClassName() +  "_" + pathToTargetJSON1.getMaeName() + "_" + pathToTargetJSON1.getName() + "_" +  metaClassSourceName + "_" + conditionMetaClassName  + "_" + conditionMaeName + "_" + conditionObjectFilter;	
						this.id = this.id + "_" + pathToTargetJSON1.getCondition().getMetaClassID() + "_" + pathToTargetJSON1.getCondition().getMaeID() + "_" + pathToTargetJSON1.getCondition().getObjectFilterID();
					}
					
					//manage duplicate
					if (this.globalUniqueNameHashMap.containsKey(globalUniqueNameLocal)) {
						Generator.logger.severe("Error 2 globalUniqueName already exist = " + globalUniqueNameLocal );

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
				localName = "Error";
			}

			if (overrideNameList.containsKey(this.id)) {
				localName = overrideNameList.get(keyID);
			} else {
				//overrideNameList.put(keyID, localName);
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
