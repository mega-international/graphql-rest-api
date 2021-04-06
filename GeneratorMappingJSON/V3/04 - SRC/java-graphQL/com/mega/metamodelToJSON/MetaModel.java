package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import com.mega.generator.Arguments;
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

	public MetaModel(MegaRoot megaRoot, String rootMetamodel,HashMap<String,String> overrideNameList) throws Exception {
		this.megaRoot = megaRoot;

		this.overrideNameList = overrideNameList;			
		oMetamodel = megaRoot.getObjectFromID(rootMetamodel);
		
		if (!oMetamodel.exists()) {
			Generator.logger.severe("The absolute identifier " + rootMetamodel + " for the metamodel is not valid. Check your JSON config file");
			throw new Exception();
		}
		
		
		absoluteIdentifier = oMetamodel.getProp(StaticFields.absoluteIdentifier);
	
	} // constructor
	
	public RootJSON generateJSON(String name, String version, String latestGeneration, String schemaName) {
		RootJSON rootJSON = new RootJSON(name,version,latestGeneration,schemaName);
		

		setMetaClass(rootJSON);
		
		
		if (!Arguments.getExtendOnly()) {
			setInterfaces(rootJSON);			
		}
		

		
		return rootJSON;
	}
	
	private void setMetaClass(RootJSON rootJSON) {

		Generator.logger.info("Start Metaclass");
		
		String sQuery = "Select [MetaClass] Where [MetaLayer] Not = \"5\" And [SystemDiagram].[Described Element]:[MetaModel].[Absolute Identifier] = \"" + absoluteIdentifier + "\"";
		MegaCollection oColMetaclass = megaRoot.getSelection(sQuery);

		List<MetaClassJSON> metaClassJSONList = new ArrayList<MetaClassJSON>();		
		
		
		Generator.logger.info("Size = "+oColMetaclass.size());
		
		int number = 0;
		
		while(oColMetaclass.hasNext()) {
			MegaObject oMetaClass = oColMetaclass.next();
			number = number+1;
			
			
			Generator.logger.info(number + " - MetaClass = " +oMetaClass.getProp(StaticFields.shortName) );
		
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
	
	
	
	public static boolean oMetaAssociationInMetaModelDiagram(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oMetaAssociationEnd) {
		boolean inDiagram = false;
		
		if (oMetaAssociationEnd.exists()) {
					
			String maeAbsoluteIdentifier = oMetaAssociationEnd.getProp(StaticFields.absoluteIdentifier);
			String oMetaModelAbsoluteIdentifier = oMetamodel.getProp(StaticFields.absoluteIdentifier);		
			String query = "Select ~R20000000k10{S}[MetaAssociationEnd] Where ~uhCGT34OxO30{P}[System Diagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs]=\""+ oMetaModelAbsoluteIdentifier +"\" And ~310000000D00{A}[_idAbs] = \""+maeAbsoluteIdentifier+"\"";
			
			MegaCollection oColMae= megaRoot.getSelection(query);
			
			int oColSize = oColMae.size();		
			
			if (oColSize==1) {
				inDiagram = true;
			}		
		}
		return inDiagram;	
	}
	
	
	public static boolean oMetaClassInMetaModelDiagram(MegaRoot megaRoot, MegaObject oMetamodel, MegaObject oMetaClass) {
		boolean inDiagram = false;
		
		

		if (oMetaClass.exists() && oMetamodel.exists()) {

			Generator.logger.finest("method oMetaClassInMetaModelDiagram :  exist= " + oMetaClass.exists()   );
			Generator.logger.finest("method oMetaClassInMetaModelDiagram :  exist= " + oMetaClass.exists() + " - Name " + oMetaClass.getProp("Name")  );
			
			
			String mcAbsoluteIdentifier = oMetaClass.getProp(StaticFields.absoluteIdentifier);
			String oMetaModelAbsoluteIdentifier = oMetamodel.getProp(StaticFields.absoluteIdentifier);		
		
			String query = "Select ~P20000000c10{S}[MetaClass] Where ~ahCGT34Ox820{P}[SystemDiagram].~iq6zWgoiC9F4{P}[Described Element]:~W20000000220{S}[_MetaModel].~310000000D00{A}[_idAbs] = \"" + oMetaModelAbsoluteIdentifier + "\"  And ~310000000D00{A}[_idAbs]=\"" + mcAbsoluteIdentifier +"\""; 
			
			MegaCollection oColMc= megaRoot.getSelection(query);
			
			int oColSize = oColMc.size();		
			
			if (oColSize==1) {
				inDiagram = true;
			}		
		
		}
		
		return inDiagram;			
	}
	
	
	
	
} // class MetaModel
