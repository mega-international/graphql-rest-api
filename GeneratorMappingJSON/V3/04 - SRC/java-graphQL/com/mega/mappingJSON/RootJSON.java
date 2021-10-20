package com.mega.mappingJSON;

import java.util.ArrayList;
import java.util.List;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.annotation.JsonPropertyOrder;
import com.mega.generator.Arguments;

@JsonPropertyOrder({ "extends", "version", "metaclass", "interfaces" })
public class RootJSON {

	@JsonProperty(value = "version")
	private VersionJSON version = null;
	@JsonProperty(value = "extends")
	private String extendsOnly = null;
	@JsonProperty(value = "metaclass")
	private List<MetaClassJSON> metaclass = null;
	@JsonProperty(value = "interfaces")
	private List<InterfacesJSON> interfaces = null;

	
	public RootJSON(String name, String version, String latestGeneration, String schemaName) {
		
		if (!Arguments.getExtendOnly()) {
			// full
			this.version = new VersionJSON(name,version,latestGeneration);	
			this.metaclass = new ArrayList<MetaClassJSON>();
			this.interfaces = new ArrayList<InterfacesJSON>();

		} else {
			// extend only
			setExtends(schemaName);
			this.metaclass = new ArrayList<MetaClassJSON>();

		}
		
	}
	
	
	public void setExtends(String extendsOnly) {
		this.extendsOnly = extendsOnly;
	}
	public String getExtends() {
		return this.extendsOnly;
	}		
	
	
	public void setVersion(VersionJSON versionJSON) {
		this.version = versionJSON;
	}
	public VersionJSON getVersion() {
		return this.version;
	}	
	
	public void setMetaclass(List<MetaClassJSON> metaclass) {
		this.metaclass = metaclass;
	}

	public List<MetaClassJSON> getMetaclass() {
		return this.metaclass;
	}
	
	
	public void setInterfaces(List<InterfacesJSON> interfaces) {
		this.interfaces = interfaces;
	}

	public List<InterfacesJSON> getInterfaces() {
		return this.interfaces;
	}
	
	
	
}
