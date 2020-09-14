package com.mega.metamodelToJSON;

import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import com.mega.generator.Generator;
import com.mega.mappingJSON.FilterMaeJSON;
import com.mega.mappingJSON.MetaClassJSON;
import com.mega.mappingJSON.PathToTargetJSON;
import com.mega.mappingJSON.RelationshipsJSON;


public class ComputeReverseId {

	private static HashMap<String,List<RelationshipsJSON>> metaClassIdPositionInList = new HashMap<String,List<RelationshipsJSON>>();		
	
	
	public static void updateReverseId(List<MetaClassJSON> metaClassJSONListSource) {

		Generator.logger.info("Starting Reverse Id");
		
		
		// we create a hashmap to be able to find by ID the Metaclass
		for (MetaClassJSON i : metaClassJSONListSource) metaClassIdPositionInList.put(i.getId(),i.getRelationships());
				
		Iterator<MetaClassJSON> itSource = metaClassJSONListSource.iterator();		

		// we loop on each MetaClass
		while (itSource.hasNext()) {
			MetaClassJSON metaClassJSONSource = itSource.next();
			String metaClassIdSource = metaClassJSONSource.getId();
			
			Generator.logger.finer("metaClassJSON :" + metaClassJSONSource.getName() + " - " + metaClassIdSource);
						
			// we loop on every relationship of the metaclass
			Iterator<RelationshipsJSON> itRelationshipsTarget = metaClassJSONSource.getRelationships().iterator();
			while (itRelationshipsTarget.hasNext()) {
				RelationshipsJSON relationshipsJSONTarget = itRelationshipsTarget.next();
				String pathToTargetIdSource = relationshipsJSONTarget.getId();
				
				Generator.logger.finer(relationshipsJSONTarget.getName() + " - pathToTargetId :" + pathToTargetIdSource);
								
				List<PathToTargetJSON> pathToTargetListTarget = relationshipsJSONTarget.getPathToTarget();
				int pathSizeTarget = pathToTargetListTarget.size();
				
				Generator.logger.finest("pathSize :" + pathSizeTarget);
				
				// we treat case with 1 path or with 2 paths
				String reverseId = "error";				
				if (pathSizeTarget == 1) {
					reverseId = singlePath(metaClassIdSource, pathToTargetIdSource, pathToTargetListTarget);
				} else if (pathSizeTarget == 2) {
					reverseId = doublePath(metaClassIdSource, pathToTargetIdSource,  pathToTargetListTarget);
				} else {
					Generator.logger.severe("pathSize above 2 not allowed :" + pathSizeTarget + " - " + pathToTargetIdSource + " - " + relationshipsJSONTarget.getGlobalUniqueName());
					
				}
	
				if (reverseId !="") {
					relationshipsJSONTarget.setReverseId(reverseId);
				}
				
				
			} //while
			
			
			
			
			
		} //while
		
		
		
		
	} // updateReverseId
	
	
	private static String singlePath(String metaClassIdSource, String pathToTargetIdSource, List<PathToTargetJSON> pathToTargetListTarget) {
		String reverseId = "";
		
		PathToTargetJSON pathToTargetJSONTarget = pathToTargetListTarget.get(0);
		
		String metaAssociationIdTarget = pathToTargetJSONTarget.getId();
		//String metaAssociationEndIdTarget = pathToTargetJSONTarget.getMaeID();
		String metaClassIdTarget = pathToTargetJSONTarget.getMetaClassID();

		if (metaClassIdTarget != null && metaClassIdTarget != "") {
			// we loop on every relationship of the Metaclass source
			
			List<RelationshipsJSON> relationshipsJSONList = metaClassIdPositionInList.get(metaClassIdTarget);
			
			if (relationshipsJSONList != null) {
				Iterator<RelationshipsJSON> itRelationshipsSource = relationshipsJSONList.iterator();
		
				//itRelationships  may be null ? catch exception and log it
				
				while (itRelationshipsSource.hasNext()) {
					RelationshipsJSON relationshipsJSONSource = itRelationshipsSource.next();
					String relationshipsIdSource = relationshipsJSONSource.getId();
		
					List<PathToTargetJSON> pathToTargetListSource = relationshipsJSONSource.getPathToTarget();
					int pathSizeSource = pathToTargetListSource.size();
					
					Generator.logger.finest("singlePath pathSizeSource :" + pathSizeSource);			
					if (pathSizeSource == 1) {
						PathToTargetJSON pathToTargetJSONSource = pathToTargetListSource.get(0);
						String localmetaAssociationIdSource = pathToTargetJSONSource.getId();
						//String localmetaAssociationEndIdSource = pathToTargetJSONSource.getMaeID();
						String localmetaClassIdSource = pathToTargetJSONSource.getMetaClassID();
						if (       metaAssociationIdTarget.contentEquals(localmetaAssociationIdSource) 
								&& metaClassIdSource.contentEquals(localmetaClassIdSource)
								
							) {					
							reverseId = relationshipsIdSource;
							break;
						} //if
	
					}
				}
			} //while		
		
		}
		
		// if we did not find the path we don't write the reverseId
		return reverseId;
	}


	private static String doublePath(String metaClassIdSource, String pathToTargetIdSource, List<PathToTargetJSON> pathToTargetListTarget) {
		String reverseId = "";
		PathToTargetJSON pathToTargetJSONTarget1 = pathToTargetListTarget.get(0);
		PathToTargetJSON pathToTargetJSONTarget2 = pathToTargetListTarget.get(1);
		
		String metaAssociationIdTarget1 = pathToTargetJSONTarget1.getId();
		//String metaAssociationEndIdTarget1 = pathToTargetJSONTarget1.getMaeID();
		String metaClassIdTarget1 = pathToTargetJSONTarget1.getMetaClassID();
		
		String metaAssociationIdTarget2 = pathToTargetJSONTarget2.getId();
		//String metaAssociationEndIdTarget2 = pathToTargetJSONTarget2.getMaeID();
		String metaClassIdTarget2 = pathToTargetJSONTarget2.getMetaClassID();


		
		FilterMaeJSON metaAssociationEndFilterTarget = pathToTargetJSONTarget1.getCondition();
		//String metaAssociationEndIdFilterTarget = "";
		//String metaClassIdFilterTarget =  "";
		String objectIdFilterTarget =  "";			
		
		if (metaAssociationEndFilterTarget != null) {
			//metaAssociationEndIdFilterTarget = metaAssociationEndFilterTarget.getMaeID();
			//metaClassIdFilterTarget =  metaAssociationEndFilterTarget.getMetaClassID();
			objectIdFilterTarget =  metaAssociationEndFilterTarget.getObjectFilterID();			
		}
		
		// we loop on every relationship of the Metaclass source
		Iterator<RelationshipsJSON> itRelationshipsSource = metaClassIdPositionInList.get(metaClassIdTarget2).iterator();

		//itRelationships  may be null ? catch exception and log it
		
		while (itRelationshipsSource.hasNext()) {
			RelationshipsJSON relationshipsJSONSource = itRelationshipsSource.next();
			
			String relationshipsIdSource = relationshipsJSONSource.getId();
			
			List<PathToTargetJSON> pathToTargetListSource = relationshipsJSONSource.getPathToTarget();
			int pathSizeSource2 = pathToTargetListSource.size();
			
			Generator.logger.finest("doublePath pathSizeSource :" + pathSizeSource2);			
			if (pathSizeSource2 == 2) {

				PathToTargetJSON pathToTargetJSONSource1 = pathToTargetListSource.get(0);
				String localmetaAssociationIdSource1 = pathToTargetJSONSource1.getId();
				//String localmetaAssociationEndIdSource1 = pathToTargetJSONSource1.getMaeID();
				String localmetaClassIdSource1 = pathToTargetJSONSource1.getMetaClassID();
				
				PathToTargetJSON pathToTargetJSONSource2 = pathToTargetListSource.get(1);
				String localmetaAssociationIdSource2 = pathToTargetJSONSource2.getId();
				//String localmetaAssociationEndIdSource2 = pathToTargetJSONSource2.getMaeID();
				String localmetaClassIdSource2 = pathToTargetJSONSource2.getMetaClassID();
				
				
				FilterMaeJSON metaAssociationEndFilterSource = pathToTargetJSONSource1.getCondition();
				//String metaAssociationEndIdFilterSource = "";
				//String metaClassIdFilterSource =  "";
				String objectIdFilterSource =  "";			
				
				if (metaAssociationEndFilterSource != null) {
					//metaAssociationEndIdFilterSource = metaAssociationEndFilterSource.getMaeID();
					//metaClassIdFilterSource =  metaAssociationEndFilterSource.getMetaClassID();
					objectIdFilterSource =  metaAssociationEndFilterSource.getObjectFilterID();			
				}
					
				if (metaAssociationEndFilterTarget != null && metaAssociationEndFilterSource != null) {
					if (       metaAssociationIdTarget1.contentEquals(localmetaAssociationIdSource2) 
							&& metaAssociationIdTarget2.contentEquals(localmetaAssociationIdSource1)
							&& metaClassIdSource.contentEquals(localmetaClassIdSource2)
							&& metaClassIdTarget1.contentEquals(localmetaClassIdSource1)	
							&& objectIdFilterTarget.contentEquals(objectIdFilterSource)
						) {		

						reverseId = relationshipsIdSource; 
						break;
					} //if							
					
				} else {
					if (       metaAssociationIdTarget1.contentEquals(localmetaAssociationIdSource2) 
							&& metaAssociationIdTarget2.contentEquals(localmetaAssociationIdSource1)
							&& metaClassIdSource.contentEquals(localmetaClassIdSource2)
							&& metaClassIdTarget1.contentEquals(localmetaClassIdSource1)			
						) {		

						reverseId = relationshipsIdSource; 
						break;
					} //if					
				}

				
			} // if
		} //while		
				
		// if we did not find the path we don't write the reverseId
		return reverseId;
	}	
	
	
} //class
