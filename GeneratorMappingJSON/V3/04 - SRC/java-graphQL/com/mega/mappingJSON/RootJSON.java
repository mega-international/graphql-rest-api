package com.mega.mappingJSON;

import java.util.ArrayList;
import java.util.List;

public class RootJSON {

	private VersionJSON version = null;
	private List<MetaClassJSON> metaclass = new ArrayList<MetaClassJSON>();
	private List<InterfacesJSON> interfaces = new ArrayList<InterfacesJSON>();

	
	public RootJSON(String name, String version, String latestGeneration) {
		this.version = new VersionJSON(name,version,latestGeneration);
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
