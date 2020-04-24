package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.mega.mappingJSON.InterfacesJSON;
import com.mega.mappingJSON.MetaClassJSON;
import com.mega.mappingJSON.RootJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;
import com.mega.vocabulary.StaticFields;

public class MetaModel  extends CommonAttributes  {

	private String absoluteIdentifierMetamodel;

	private HashMap<String,String> overrideNameList;
	private HashMap<String,String> globalUniqueName = new HashMap<String,String>();
	
	public MetaModel(MegaRoot megaRoot, String rootMetamodel,HashMap<String,String> overrideNameList) {
		this.megaRoot = megaRoot;

		this.overrideNameList = overrideNameList;			
		oMetamodel = megaRoot.getObjectFromID(rootMetamodel);
		absoluteIdentifierMetamodel = oMetamodel.getProp(StaticFields.absoluteIdentifier);
	
	} // constructor
	
	public RootJSON generateJSON(String name, String version, String latestGeneration) {
		RootJSON rootJSON = new RootJSON(name,version,latestGeneration);
		

		setMetaClass(rootJSON);
		setInterfaces(rootJSON);
		
		return rootJSON;
	}
	
	private void setMetaClass(RootJSON rootJSON) {

		String sQuery = "Select [MetaClass] Where [MetaLayer] Not = \"5\" And [SystemDiagram].[Described Element]:[MetaModel].[Absolute Identifier] = \"" + absoluteIdentifierMetamodel + "\"";
		MegaCollection oColMetaclass = megaRoot.getSelection(sQuery);

		List<MetaClassJSON> metaClassJSONList = new ArrayList<MetaClassJSON>();		
		
		while(oColMetaclass.hasNext()) {
			MegaObject oMetaClass = oColMetaclass.next();
			MetaClass metaClass = new MetaClass(megaRoot,oMetamodel,oMetaClass,overrideNameList,globalUniqueName);			
			if (metaClass.getIsValidMetaClass()) {
				//System.out.println("Reading - oMetaClass - " + oMetaClass.getProp("Name"));

				 MetaClassJSON metaClassJSON = metaClass.getMetaClasJSON();
				metaClassJSONList.add(metaClassJSON);
			 }
		} // while
		
		rootJSON.setMetaclass(metaClassJSONList);		
		
	}
	
	private void setInterfaces(RootJSON rootJSON) {
		
		InterfacesHOPEX interfaceHopex = new InterfacesHOPEX(megaRoot);
		List<InterfacesJSON> interfaces = interfaceHopex.generateJSON();
		
		rootJSON.setInterfaces(interfaces);
		
	}
	
	
	
	
	
	
} // class MetaModel
