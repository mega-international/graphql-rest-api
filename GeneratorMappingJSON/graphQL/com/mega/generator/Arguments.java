package com.mega.generator;

import java.util.logging.Level;

import org.apache.commons.cli.CommandLine;
import org.apache.commons.cli.CommandLineParser;
import org.apache.commons.cli.DefaultParser;
import org.apache.commons.cli.HelpFormatter;
import org.apache.commons.cli.Option;
import org.apache.commons.cli.Options;
import org.apache.commons.cli.ParseException;

public class Arguments {

	private static boolean debug = false;
	private static boolean verbose = false;
//	private static String folder = "C:\\\\temp\\\\java\\\\";
	private static String folder = "C:\\temp\\java\\";

	private static String logfolder="C:\\temp\\java\\";

//	private static String logfolder="C:\\\\temp\\\\java\\\\";

	
	private static String environment = "C:\\Users\\Public\\Documents\\HOPEX V3\\PRESALESV3";
	private static String repository = "Demo";
	
	private static String fileNameOverride="00_OverrideName_Global.JSON";
	private static String fileNameSchema="00_SchemaToGenerate.JSON";

	
	private static Level level = Level.INFO;
	
	public static Level getLevel() {
		return Arguments.level;
	}
	
	public static boolean getDebug() {
		return debug;
	}
	
	public static void setDebug(boolean debug) {
		Arguments.debug = debug;
	}

	public static boolean getVerbose() {
		return verbose;
	}
	
	public static void setVerbose(boolean verbose) {
		Arguments.verbose = verbose;
	}	
	
	public static String getLogfolder() {
		return Arguments.logfolder;
	}
	
	public static void setLogfolder(String logfolder) {
		Arguments.logfolder = logfolder;
	}
	
	
	public static String getFolder() {
		return Arguments.folder;
	}
	
	public static void setFolder(String folder) {
		Arguments.folder = folder;
	}

	public static String getEnvironment() {
		return Arguments.environment;
	}
	
	public static void setEnvironment(String environment) {
		Arguments.environment = environment;
	}
		
	public static String getRepository() {
		return Arguments.repository;
	}
	
	public static void setRepository(String repository) {
		Arguments.repository = repository;
	}	
	
	public static String getFileNameOverride() {
		return Arguments.fileNameOverride;
	}

	public static void setFileNameOverride(String fileNameOverride) {
		Arguments.fileNameOverride = fileNameOverride;
	}	
	
	
	public static String getFileNameSchema() {
		return Arguments.fileNameSchema;
	}

	public static void setFileNameSchema(String fileNameSchema) {
		Arguments.fileNameSchema = fileNameSchema;
	}	
	
	public static void setGlobalVariables(String[] args) throws ParseException {

		Options options = new Options();

		options.addOption("h", "help", false, "print this message");
		options.addOption("d", "debug", false, "Enables aditionnal logs to be displayed for debug purposes");
		options.addOption("v", "verbose", false, "Be extra verbose in the logs");		
		
		Option folder   = Option.builder("f")
			.optionalArg(true)
			.longOpt("folder")
			.argName("folder" )
			.hasArg()
			.desc("Specify the folder hierarchy were to read/write the files" )
			.build();				
		options.addOption(folder);

		Option environment   = Option.builder("e")
				.optionalArg(true)
				.longOpt("environment")
				.argName("environment" )
				.hasArg()
				.desc("Specify the folder of the environment to use" )
				.build();				
			options.addOption(environment);		

			Option repository   = Option.builder("r")
					.optionalArg(true)
					.longOpt("repository")
					.argName("repository" )
					.hasArg()
					.desc("Specify the name of the repository" )
					.build();				
				options.addOption(repository);				
			
		Option logfile = Option.builder("l")
				.optionalArg(true)
				.longOpt("logfolder")
				.argName("logfolder" )
				.hasArg()
				.desc("Give the folder to use to store logs" )
				.build();				
		options.addOption(logfile);

		Option jsonFileForNaming = Option.builder("j")
				.optionalArg(true)
				.longOpt("jsonFileForNaming")
				.argName("file name with extension" )
				.hasArg()
				.desc("Give the name of the file that contains the names of MetaModel to override" )
				.build();				
		options.addOption(jsonFileForNaming);		


		Option fileNameSchema = Option.builder("s")
				.optionalArg(true)
				.longOpt("fileNameSchema")
				.argName("file name with extension" )
				.hasArg()
				.desc("Give the name of the file that contains the schema and metamodel to generate" )
				.build();				
		options.addOption(fileNameSchema);			
		
		CommandLineParser parser = new DefaultParser();
		CommandLine cmd = parser.parse( options, args);		

		
		if(cmd.hasOption("h")) {
			HelpFormatter formatter = new HelpFormatter();
			formatter.printHelp("Generator", options,true);		
		} 
		else {
			if(cmd.hasOption("d")) {
				Arguments.setDebug(true);	
				Arguments.level = Level.CONFIG;
			}
			if(cmd.hasOption("v")) {
				Arguments.setVerbose(true);
				Arguments.level = Level.FINEST;
			} 
			if(cmd.hasOption("f")) {
				Arguments.setFolder(cmd.getOptionValue( "f" ));
			} 
			if(cmd.hasOption("e")) {
				Arguments.setEnvironment(cmd.getOptionValue( "e" ));
			} 
			if(cmd.hasOption("r")) {
				Arguments.setRepository(cmd.getOptionValue( "r" ));
			}
			if(cmd.hasOption("l")) {
				Arguments.setLogfolder(cmd.getOptionValue( "l" ));
			}
			if(cmd.hasOption("j")) {
				Arguments.setFileNameOverride(cmd.getOptionValue( "j" ));
			}
			if(cmd.hasOption("s")) {
				Arguments.setFileNameSchema(cmd.getOptionValue( "s" ));
			}
		}

		
	}	
	
	
}
