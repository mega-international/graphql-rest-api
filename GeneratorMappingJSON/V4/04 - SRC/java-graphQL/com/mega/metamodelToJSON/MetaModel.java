package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;


import com.mega.generator.Generator;
import com.mega.mappingJSON.CommonFields;
import com.mega.mappingJSON.InterfacesJSON;
import com.mega.mappingJSON.MetaClassJSON;
import com.mega.mappingJSON.RootJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaModel  extends CommonAttributes  {

	public MetaModel(MegaRoot megaRoot, String rootMetamodel,HashMap<String,String> overrideNameList) {
		this.megaRoot = megaRoot;

		this.overrideNameList = overrideNameList;			
		oMetamodel = megaRoot.getObjectFromID(rootMetamodel);
		absoluteIdentifier = oMetamodel.getProp(StaticFields.absoluteIdentifier);
	
	} // constructor
	
	public RootJSON generateJSON(String name, String version, String latestGeneration) {
		RootJSON rootJSON = new RootJSON(name,version,latestGeneration);
		

		setMetaClass(rootJSON);
		setInterfaces(rootJSON);
		
		return rootJSON;
	}
	
	private void setMetaClass(RootJSON rootJSON) {

		Generator.logger.info("Start Metaclass");
		
		String sQuery = "Select [MetaClass] Where [MetaLayer] Not = \"5\" And [SystemDiagram].[Described Element]:[MetaModel].[Absolute Identifier] = \"" + absoluteIdentifier + "\"";
		MegaCollection oColMetaclass = megaRoot.getSelection(sQuery);

		List<MetaClassJSON> metaClassJSONList = new ArrayList<MetaClassJSON>();		
		
		
		Generator.logger.info("Size = "+oColMetaclass.size());
		
		while(oColMetaclass.hasNext()) {
			MegaObject oMetaClass = oColMetaclass.next();
			MetaClass metaClass = new MetaClass(megaRoot,oMetamodel,oMetaClass,overrideNameList,globalUniqueName);			

			if (metaClass.getIsValidMetaClass()) {
				 MetaClassJSON metaClassJSON = metaClass.getMetaClasJSON();
				metaClassJSONList.add(metaClassJSON);								
			 }
		} // while
				
		MetaModel.reportDuplicateName("MetaClass", metaClassJSONList, ensureUniqueName);	
		ComputeReverseId.updateReverseId(metaClassJSONList);		
		
		rootJSON.setMetaclass(metaClassJSONList);	
		
		

	}
	

	
	private void setInterfaces(RootJSON rootJSON) {

		Generator.logger.info("Start Interfaces");
		
		
		InterfacesHOPEX interfaceHopex = new InterfacesHOPEX(megaRoot);
		List<InterfacesJSON> interfaces = interfaceHopex.generateJSON();

		MetaModel.reportDuplicateName("Interfaces", interfaces, ensureUniqueName);	
		
		rootJSON.setInterfaces(interfaces);
		
	}
	

	public static void reportDuplicateName(String location, List<?> JSONList, HashMap<String,String> ensureUniqueName) {
		//CommonFields
		// error to report duplicate in name
		@SuppressWarnings("unchecked")
		Iterator<CommonFields> it = (Iterator<CommonFields>) JSONList.iterator();
		while(it.hasNext()) {
			CommonFields metaClassJSON = it.next();
			//ensure unique name				
			if (ensureUniqueName.containsKey(metaClassJSON.getName())) {
				Generator.logger.severe(location + " duplicate name : " + metaClassJSON.getName() + " ID1 = " + metaClassJSON.getId() + " ID2 = "+ ensureUniqueName.get(metaClassJSON.getName()) );
			} else {
				ensureUniqueName.put(metaClassJSON.getName(), metaClassJSON.getId());
			} // if					
		} // while		
	
	}	
	
	
	
	
} // class MetaModel
