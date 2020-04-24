package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.mega.mappingJSON.InterfacesJSON;
import com.mega.mappingJSON.PropertiesJSON;
import com.mega.modeling.api.MegaCollection;
import com.mega.modeling.api.MegaObject;
import com.mega.modeling.api.MegaRoot;

public class InterfacesHOPEX extends CommonAttributes {

	List<InterfacesJSON> interfaces = new ArrayList<InterfacesJSON>();		
	
	public 	InterfacesHOPEX(MegaRoot megaRoot) {

		this.megaRoot = megaRoot;
	}
	

	public List<InterfacesJSON> generateJSON() {
		
		interfaces.add(JSONForgenericObject());
		interfaces.add(JSONForgenericObjectSystem());
		interfaces.add(JSONForRelationShip());
		
		
		return interfaces;
		
	}

	
	private InterfacesJSON JSONForgenericObject() {
		String name = "genericObject";
		String id = "p20000000E30";
		String description = "Common properties for all metaclass inheriting from generic object";

		HashMap<String,String> overrideNameList = new HashMap<String,String>();
		InterfacesJSON interfacesJSON = new InterfacesJSON(overrideNameList,id);
		interfacesJSON.setName(name);		
		interfacesJSON.setId(id);
		interfacesJSON.setDescription(description);		

		List<PropertiesJSON> PropertiesJSONList = new ArrayList<PropertiesJSON>();		
		interfacesJSON.setProperties(PropertiesJSONList);		
		
		MegaCollection oColMetaAttribute = addCollectionMetaAttributeGenericObject(megaRoot);
		
		
		while(oColMetaAttribute.hasNext()) {
			MegaObject oMetaAttribute = oColMetaAttribute.next();
			MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,oMetaAttribute, overrideNameList);
			if (metaAttribute.isValideMetaAttribute()) {				
				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				PropertiesJSONList.add(propertiesJSON);				
			} // if valid			

		} //while		
		
		return interfacesJSON;

	} // JSONForgenericObject
	

	private InterfacesJSON JSONForgenericObjectSystem() {
		String name = "genericObjectSystem";
		String id = "q20000000I30";
		String description = "Common properties for all metaclass inheriting from generic object system";

		HashMap<String,String> overrideNameList = new HashMap<String,String>();
		InterfacesJSON interfacesJSON = new InterfacesJSON(overrideNameList,id);
		interfacesJSON.setName(name);		
		interfacesJSON.setId(id);
		interfacesJSON.setDescription(description);		

		List<PropertiesJSON> PropertiesJSONList = new ArrayList<PropertiesJSON>();		
		interfacesJSON.setProperties(PropertiesJSONList);		
		
		MegaCollection oColMetaAttribute = addCollectionMetaAttributeGenericObjectSystem(megaRoot);
		
		
		while(oColMetaAttribute.hasNext()) {
			MegaObject oMetaAttribute = oColMetaAttribute.next();
			MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,oMetaAttribute, overrideNameList);
			if (metaAttribute.isValideMetaAttribute()) {				
				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				PropertiesJSONList.add(propertiesJSON);				
			} // if valid			

		} //while		
		
		return interfacesJSON;

	} // JSONForgenericObjectSystem	

	private InterfacesJSON JSONForRelationShip() {
		String name = "relationship";
		String id = "jHt39NXIPL2H";
		String description = "Common properties for all relationship";

		HashMap<String,String> overrideNameList = new HashMap<String,String>();
		InterfacesJSON interfacesJSON = new InterfacesJSON(overrideNameList,id);
		interfacesJSON.setName(name);		
		interfacesJSON.setId(id);
		interfacesJSON.setDescription(description);		

		List<PropertiesJSON> PropertiesJSONList = new ArrayList<PropertiesJSON>();		
		interfacesJSON.setProperties(PropertiesJSONList);		
		
		MegaCollection oColMetaAttribute = addCollectionMetaAttributeMetaAssociation(megaRoot);
		
		
		while(oColMetaAttribute.hasNext()) {
			MegaObject oMetaAttribute = oColMetaAttribute.next();
			MetaAttribute metaAttribute = new MetaAttribute(megaRoot,oMetamodel,oMetaAttribute, overrideNameList);
			if (metaAttribute.isValideMetaAttribute()) {				
				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				PropertiesJSONList.add(propertiesJSON);				
			} // if valid			

		} //while		
		
		return interfacesJSON;

	} // JSONForRelationShip	
	
	
	
	public static MegaCollection addCollectionMetaAttributeGenericObject(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");
		
		 oCol.insert(megaRoot.getObjectFromID("~M30000000P90[Is Writable]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~210000000900[Name]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~f10000000b20[Comment]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~CFmhlMxNT1iE[ External Identifier]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~310000000D00[Absolute Identifier]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~H20000000550[_HexaIdAbs]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~510000000L00[Creation Date]"));	
		 oCol.insert(megaRoot.getObjectFromID("~610000000P00[Modification Date]"));
		 oCol.insert(megaRoot.getObjectFromID("~)10000000z30[Creator Name]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~c10000000P20[Modifier Name]"));	 
		 oCol.insert(megaRoot.getObjectFromID("~(10000000v30[Creator]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~b10000000L20[Modifier]"));	   
		 //oCol.insert(megaRoot.getObjectFromID(""));	   

		return oCol;
	}
	
	public static MegaCollection addCollectionMetaAttributeGenericObjectSystem(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");		
		 oCol.insert(megaRoot.getObjectFromID("~M30000000P90[Is Writable]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~210000000900[Name]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~f10000000b20[Comment]"));   
		 oCol.insert(megaRoot.getObjectFromID("~310000000D00[Absolute Identifier]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~H20000000550[_HexaIdAbs]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~510000000L00[Creation Date]"));
		 oCol.insert(megaRoot.getObjectFromID("~610000000P00[Modification Date]"));		 
		 oCol.insert(megaRoot.getObjectFromID("~)10000000z30[Creator Name]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~c10000000P20[Modifier Name]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~(10000000v30[Creator]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~b10000000L20[Modifier]"));	   
		 //oCol.insert(megaRoot.getObjectFromID(""));	   

		return oCol;
	}
		
	public static MegaCollection addCollectionMetaAttributeMetaAssociation(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");
		 oCol.insert(megaRoot.getObjectFromID("~410000000H00[Order]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~C3cm9FyluS20[Link Comment]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~710000000T00[Link creation date]"));		 
		 oCol.insert(megaRoot.getObjectFromID("~810000000X00[Link modification date]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~720000000T40[Link Creator]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~820000000X40[Link Creator Name]"));	
		 oCol.insert(megaRoot.getObjectFromID("~920000000b40[Link Modifier]"));	   
		 oCol.insert(megaRoot.getObjectFromID("~A20000000f40[Link Modifier Name]"));
		 //oCol.insert(megaRoot.getObjectFromID(""));	   
		return oCol;
	}	
	
} // class
