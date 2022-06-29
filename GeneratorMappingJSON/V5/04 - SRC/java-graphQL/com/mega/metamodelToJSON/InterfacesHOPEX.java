package com.mega.metamodelToJSON;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
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
			if (metaAttribute.getIsValid()) {				
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
			if (metaAttribute.getIsValid()) {				
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
			if (metaAttribute.getIsValid()) {				
				PropertiesJSON propertiesJSON = metaAttribute.getPropertiesJSON();
				PropertiesJSONList.add(propertiesJSON);				
			} // if valid			

		} //while		
		
		return interfacesJSON;

	} // JSONForRelationShip	
	
	
	
	public static MegaCollection addCollectionMetaAttributeGenericObject(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");
		
		
		
		List<String> listMetaClass = listCollectionMetaAttributeGenericObject();
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
	
	public static MegaCollection addCollectionMetaAttributeGenericObjectSystem(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");
		List<String> listMetaClass = listCollectionMetaAttributeGenericObjectSystem();
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
		
	public static MegaCollection addCollectionMetaAttributeMetaAssociation(MegaRoot megaRoot) {

		MegaCollection oCol = megaRoot.getSelection("");

		List<String> listMetaClass = listCollectionMetaAttributeMetaAssociation();
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
	
	
	
	
	
	public static List<String> listCollectionMetaAttributeGenericObject() {
		List<String> listMetaAttribute = new ArrayList<String>();
		listMetaAttribute.add("~M30000000P90[Is Writable]");
		listMetaAttribute.add("~210000000900[Name]");
		listMetaAttribute.add("~f10000000b20[Comment]");
		listMetaAttribute.add("~CFmhlMxNT1iE[ External Identifier]");
		listMetaAttribute.add("~310000000D00[Absolute Identifier]");
		listMetaAttribute.add("~H20000000550[_HexaIdAbs]");
		listMetaAttribute.add("~510000000L00[Creation Date]");
		listMetaAttribute.add("~610000000P00[Modification Date]");
		listMetaAttribute.add("~(10000000v30[Creator]");
		listMetaAttribute.add("~b10000000L20[Modifier]");	
		return listMetaAttribute;		
	}	
	
	public static List<String> listCollectionMetaAttributeGenericObjectSystem() {
		List<String> listMetaAttribute = new ArrayList<String>();
		listMetaAttribute.add("~M30000000P90[Is Writable]");
		listMetaAttribute.add("~210000000900[Name]");
		listMetaAttribute.add("~f10000000b20[Comment]");
		listMetaAttribute.add("~CFmhlMxNT1iE[ External Identifier]");		
		listMetaAttribute.add("~310000000D00[Absolute Identifier]");
		listMetaAttribute.add("~H20000000550[_HexaIdAbs]");
		listMetaAttribute.add("~510000000L00[Creation Date]");
		listMetaAttribute.add("~610000000P00[Modification Date]");
		listMetaAttribute.add("~(10000000v30[Creator]");
		listMetaAttribute.add("~b10000000L20[Modifier]");			
		
		return listMetaAttribute;		
	}	
	
	
	public static List<String> listCollectionMetaAttributeMetaAssociation() {
		List<String> listMetaAttribute = new ArrayList<String>();
		listMetaAttribute.add("~410000000H00[Order]");
		listMetaAttribute.add("~C3cm9FyluS20[Link Comment]");
		listMetaAttribute.add("~710000000T00[Link creation date]");
		listMetaAttribute.add("~810000000X00[Link modification date]");
		listMetaAttribute.add("~720000000T40[Link Creator]");
		listMetaAttribute.add("~920000000b40[Link Modifier]");
		return listMetaAttribute;		
	}		
	
	
	
	
} // class
