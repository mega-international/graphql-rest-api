package com.mega.generator;

import java.io.StringWriter;
import java.text.SimpleDateFormat;
import java.util.*;
import java.util.logging.Logger;
import java.io.File;
import java.io.FileWriter;
import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.databind.*;
import com.mega.mappingJSON.*;
import com.mega.metamodelToJSON.MetaModel;
import com.mega.modeling.api.MegaRoot;

public class Generator {

	public static Logger logger;	
		
	public static void main(String[] args) throws Exception{
		
		// *************** Variables *************** 
		// Manage global variable throughout the whole program
		Arguments.setGlobalVariables(args);
		// ***************************************** 
		
		
		// *************** Logs *************** 
		// Manage the error logs throughout the whole program
		// ************************************ 
		
		logger = GlobalLogs.getLogger();	

		Generator.logger.config("debug                   = "+Arguments.getDebug());
		Generator.logger.config("verbose                 = "+Arguments.getVerbose());
		Generator.logger.config("Folder                  = "+Arguments.getFolder());
		Generator.logger.config("Log Folder              = "+Arguments.getLogfolder());
		Generator.logger.config("File Name Override      = "+Arguments.getFileNameOverride());
		Generator.logger.config("File Name Schema        = "+Arguments.getFileNameSchema());
		Generator.logger.config("Environment             = "+Arguments.getEnvironment());
		Generator.logger.config("Repository              = "+Arguments.getRepository());
		
		
		
	
		/*
		SEVERE (highest)
		WARNING
		INFO
		CONFIG
		FINE
		FINER
		FINEST
		*/
		
		// *************** Start *************** 
        // Start the program
		// ************************************ 
        
		String fileNameOverride=Arguments.getFileNameOverride();
		String filePath = Arguments.getFolder();	
		String latestGeneration = getToday();

		String filePathSchema = Arguments.getFolder() + Arguments.getFileNameSchema();
		
		SchemaToGenerateList schemaToGenerateList = loadSchemaToGenerate(filePathSchema);
				
		Generator.logger.finest("Latest Generation Date = " + latestGeneration);	
		Generator.logger.info("########### Starting ###########");
		
		Iterator<SchemaToGenerate> it = schemaToGenerateList.getSchemaToGenerate().iterator();
	
		while (it.hasNext()) {
			SchemaToGenerate schemaToGenerate = it.next();
			if (schemaToGenerate.getIncluded()) {
				generateMetaModel(schemaToGenerate.getSchemaName(),schemaToGenerate.getMetaModelAbsoluteIdentifier(),schemaToGenerate.getLogin(),schemaToGenerate.getPassword(),schemaToGenerate.getProfile(),    filePath, fileNameOverride, latestGeneration);				
			}
		}

		Generator.logger.info("########### All done ###########");


	}

	private static SchemaToGenerateList loadSchemaToGenerate(String filePath) {
		SchemaToGenerateList schemaToGenerateList = null;

		Generator.logger.info("Read schema name");

		try {
			ObjectMapper objectMapper = new ObjectMapper();

			File file = new File(filePath);			
			schemaToGenerateList = objectMapper.readValue(file, SchemaToGenerateList.class);
			
		} catch (Exception e) {
			e.printStackTrace();
		}	
		
		return schemaToGenerateList;
	}
	
	private static void generateMetaModel(String metaModelName, String metaModelidAbs, String login, String password, String profile, String filePath, String fileNameOverride, String latestGeneration) {

		Generator.logger.info("########## Starting : " + metaModelName);
		
		OpenHOPEX openHOPEX = new OpenHOPEX(login, password, "-Role:" + profile + " -TranType:Micro -OpenMode:R");
		MegaRoot megaRoot = openHOPEX.getMegaRoot();
	
		String fileName = metaModelName + ".JSON";
		HashMap<String,String> overRideNameList = new HashMap<String,String>();
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,false);
		generateJSON(megaRoot,metaModelidAbs,filePath+fileName,overRideNameList,openHOPEX.getName(),openHOPEX.getVersion(),latestGeneration);
		readwriteOverRideName(filePath + fileNameOverride,overRideNameList,true);
		openHOPEX.closeHOPEX();
	}
		
	private static void readwriteOverRideName(String filePath,  HashMap<String,String> overRideNameList, boolean write) {

		if (write) {
			Generator.logger.info("Write overRideName JSON");
		} else 
		{
			Generator.logger.info("Read overRideName JSON");
		}
		
		try {
			ObjectMapper objectMapper = new ObjectMapper();	
			objectMapper.configure(SerializationFeature.INDENT_OUTPUT, true);		
			objectMapper.setSerializationInclusion(Include.NON_NULL);		
			
			File file = new File(filePath);
			StringWriter stringEmp = new StringWriter();
			
			if (!file.exists()) {
				// create a template file in case it does not exist
		    	file.createNewFile();
				FileWriter fileWriter=new FileWriter(file);				
				stringEmp.append("{");
				//	stringEmp.append("\"metaClassID\" : \"MetaClass to rename\",");
				//	stringEmp.append("\"metaAssociationID_metaClassID\" : \"Way to build the key for a simple path\",");
				//	stringEmp.append("\"metaAttributeID\" : \"MetaAttribute to rename\",");
				//stringEmp.append("\"metaAssociationID1_metaClassID1_metaAssociationID2_metaClassID2\" : \"Way to build the key for a 2 step path\"");
				stringEmp.append("}");
				fileWriter.write(stringEmp.toString());
				fileWriter.close();	
			}
	

			if (write) {
				objectMapper.writeValue(stringEmp, overRideNameList);				
				FileWriter fileWriter=new FileWriter(file);	
				fileWriter.write(stringEmp.toString());
				fileWriter.close();			
				
			} else {
				@SuppressWarnings("unchecked")
				HashMap<String, String> localHashmap = objectMapper.readValue(file, HashMap.class);
							
				overRideNameList.putAll(localHashmap);
			}
			
			
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
	
	private static void writeJSONFile(RootJSON rootJSON, String filePath ) {

		Generator.logger.finer("Mapper ready");

		
		ObjectMapper objectMapper = new ObjectMapper();
		objectMapper.configure(SerializationFeature.INDENT_OUTPUT, true);		
		StringWriter stringEmp = new StringWriter();
		
		objectMapper.setSerializationInclusion(Include.NON_NULL);		
		
		try {
			objectMapper.writeValue(stringEmp, rootJSON);
		} catch (Exception e) {
			e.printStackTrace();
		}

		Generator.logger.info("Wrting file" + filePath);
		
	    try {
			File file = new File(filePath);
	    	file.createNewFile();
			FileWriter fileWriter=new FileWriter(file);		
			fileWriter.write(stringEmp.toString());
			fileWriter.close();			
			
		} catch (Exception e) {
			e.printStackTrace();
		}		
		
		
	}
	
	private static void generateJSON(MegaRoot megaRoot,String rootMetaModel,String filePath,HashMap<String,String> overRideNameList, String name, String version,String latestGeneration) {

		Generator.logger.info("Creating JSON");

		MetaModel metaModel = new MetaModel(megaRoot,rootMetaModel,overRideNameList);
		RootJSON rootJSON = metaModel.generateJSON(name,version,latestGeneration);
		
		writeJSONFile(rootJSON, filePath );
			
	}

	private static String getToday() {
		Calendar cal = Calendar.getInstance();
		SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
		String latestGeneration = format.format(cal.getTime());		
		return latestGeneration;
	}
	
	
} //class
