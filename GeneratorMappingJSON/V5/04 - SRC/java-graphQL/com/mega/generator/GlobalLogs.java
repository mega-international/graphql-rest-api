package com.mega.generator;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.logging.ConsoleHandler;
import java.util.logging.FileHandler;
import java.util.logging.Level;
import java.util.logging.LogRecord;
import java.util.logging.Logger;
import java.util.logging.SimpleFormatter;

public class GlobalLogs {

	private static final Logger logger = Logger.getLogger(Logger.GLOBAL_LOGGER_NAME);	

    public static Logger getLogger() throws SecurityException, IOException {
  
    	String logFolder = Arguments.getLogfolder();
    	Level level =  Arguments.getLevel();
    	
    	logger.setLevel(level);
    	logger.setUseParentHandlers(false);  // to write in console

    	if (Arguments.getDebug()) {
            ConsoleHandler handler = new ConsoleHandler();
            handler.setLevel(level);
            handler.setFormatter(new SimpleFormatter() {
                private static final String format = "[%1$tF %1$tT] [%2$-7s] %3$s %n";

                @Override
                public synchronized String format(LogRecord lr) {
                    return String.format(format,
                            new Date(lr.getMillis()),
                            lr.getLevel().getLocalizedName(),
                            lr.getMessage()
                    );
                }
            });    	
            logger.addHandler(handler);      		
    	}	

        Date date = new Date() ;
        SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH-mm-ss") ;
         
    	FileHandler fileHandlerlogFile = new FileHandler(logFolder + dateFormat.format(date) + "-generator.log");
    	
    	fileHandlerlogFile.setLevel(level);
    	
    	fileHandlerlogFile.setFormatter(new SimpleFormatter() {
            private static final String format = "[%1$tF %1$tT] [%2$-7s] %3$s %n";

            @Override
            public synchronized String format(LogRecord lr) {
                return String.format(format,
                        new Date(lr.getMillis()),
                        lr.getLevel().getLocalizedName(),
                        lr.getMessage()
                );
            }
        });
   
        logger.addHandler(fileHandlerlogFile);	   	
    	
       	return GlobalLogs.logger;
   	
    }
	
}
