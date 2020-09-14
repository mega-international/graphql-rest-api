package com.mega.generator;

import java.util.ArrayList;
import java.util.List;


public class SchemaToGenerateList {

	private List<SchemaToGenerate> schemaToGenerate = new ArrayList<SchemaToGenerate>();

	public void setSchemaToGenerate(List<SchemaToGenerate> schemaToGenerate) {
		this.schemaToGenerate = schemaToGenerate;
	}
	
	public List<SchemaToGenerate> getSchemaToGenerate() {
		return this.schemaToGenerate;
	}
	
}
