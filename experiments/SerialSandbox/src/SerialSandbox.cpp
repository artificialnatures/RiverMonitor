/******************************************************/
//       THIS IS A GENERATED FILE - DO NOT EDIT       //
/******************************************************/

#include "Particle.h"
#line 1 "/Users/aaron/GitHub/artificialnatures/RiverMonitor/experiments/SerialSandbox/src/SerialSandbox.ino"
void setup();
void loop();
void EchoLoop();
void ControllerLoop();
#line 1 "/Users/aaron/GitHub/artificialnatures/RiverMonitor/experiments/SerialSandbox/src/SerialSandbox.ino"
SerialLogHandler logHandler;

const String testCodes[10] = 
{
    "X0401",
    "X0402",
    "X0403",
    "X0404",
    "X0405",
    "X0406",
    "X0407",
    "X0408",
    "X0409",
    "X0410"
};
uint testCodeIndex = 0;

void setup() 
{
	Log.info("Started.");
	Serial1.begin(9600, SERIAL_8N1);
}

void loop() 
{
	EchoLoop();
}

void EchoLoop() {
	if (Serial1.available()) {
		String message = Serial1.readString();
		Log.info(message.c_str());
		String response = String::format("ECHO %s", message.c_str());
		Serial1.flush();
		Serial1.write(response.c_str());
		Serial1.flush();
	}
}

void ControllerLoop() {
	String testCode = testCodes[testCodeIndex];
	const char * serialMessage = testCode.c_str();
	Log.info("Sending code: %s", serialMessage);
	size_t bytesWritten = Serial1.write(serialMessage);
	Log.info("Wrote %i bytes.", bytesWritten);
	String response = Serial1.readString();
	Log.info("Raw Response: %s", response.c_str());
	if (testCodeIndex < 9)
	{
		testCodeIndex++;
	}
	else
	{
		testCodeIndex = 0;
	}
	delay(5000);
}