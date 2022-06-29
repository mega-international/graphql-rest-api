package com.mega.mappingJSON;

import java.util.Calendar;

public class VersionJSON {

	private String jsonVersion;
	private String platformVersion;
	private String metamodelVersion;
	private String latestGeneration;
		
	public VersionJSON(String name, String version, String latestGeneration) {
		this.platformVersion = name;
		this.metamodelVersion = version;
		this.latestGeneration = latestGeneration;
		
		Calendar cal = Calendar.getInstance();
		String year = "" + cal.get(Calendar.YEAR);
		year = year.substring(2, 4);
		
		String month ="" + (cal.get(Calendar.MONTH)+1);		
		String day = ""+ cal.get(Calendar.DAY_OF_MONTH);
		
		if (day.length()<2) {
			day = "0"+ day;
		}
		
		jsonVersion = "V" + year + "."+ month + "."+ day;
	}
	
	public void setJsonVersion(String jsonVersion) {
		this.jsonVersion = jsonVersion;
	}
	
	public String getJsonVersion() {
		return this.jsonVersion;
	}

	public void setMetamodelVersion(String metamodelVersion) {
		this.metamodelVersion = metamodelVersion;
	}
	
	public String getMetamodelVersion() {
		return this.metamodelVersion;
	}	
	
	public void setLatestGeneration(String latestGeneration) {
		this.latestGeneration = latestGeneration;
	}

	public String getLatestGeneration() {
		return this.latestGeneration;
	}

	public void setPlatformVersion(String platformVersion) {
		this.platformVersion = platformVersion;
	}
	
	public String getPlatformVersion() {
		return this.platformVersion;
	}
	
	
	
}
