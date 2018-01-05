.Net hands-on exercise
The purpose of the current exercise is to develop an application in C# using the tools and technologies available for developers in the latest release of Microsoft .Net framework. The exercise tests not just coding abilities, but also architecture and software design – so sufficient thought must be given to design and test aspects of the solution.
You are a part of infrastructure team in a company developing a software application.
Among the different features in the application, there is a requirement for message logging for diagnostics and troubleshooting. There can be different threads and modules in the application that can produce log messages, a module responsible for logging should receive these log messages, organize them and write them to a file.
In context of the current exercise, you are required to implement this logging module. Each log message should include in addition to the actual message text both a timestamp and severity (log level).
You are required to implement the logging module in such a way, that severity will have higher priority than chronological order when deciding in what order to write log messages into a file. For example, if 100 warning messages have been received and are being saved, and 10 error messages comes in – these error messages will be saved first, before processing the rest of the warning messages. And if during processing of these 10 error messages a critical message comes in, processing it will take precedence placing it before some of the error messages in the resulting file. The immediate effect of this, is that contrary to regular logs – the information in the file will not be strictly in chronological order, which is desirable for our purposes.
Logging capabilities required are basic text logging with log level specification. Each log entry includes textual data only, no binary attachments are required.
Log levels are (from less severe to the more severe):
 Debug
 Info
 Warn
 Error
 Critical
Implement the core module that enables receiving messages from multithreaded environment, through a correct data structure (note the ordering requirements) capable of outputting them to a single file. Make sure to prove requirement coverage using tests or a sample program.