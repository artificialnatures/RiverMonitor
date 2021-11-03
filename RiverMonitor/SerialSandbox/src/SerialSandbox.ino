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
    "X040A"
};
const char testCharacterCodes[10][5] = 
{
    {'X', '0', '4', '0', '1'},
	{'X', '0', '4', '0', '2'},
	{'X', '0', '4', '0', '3'},
	{'X', '0', '4', '0', '4'},
	{'X', '0', '4', '0', '5'},
	{'X', '0', '4', '0', '6'},
	{'X', '0', '4', '0', '7'},
	{'X', '0', '4', '0', '8'},
	{'X', '0', '4', '0', '9'},
	{'X', '0', '4', '0', 'A'}
};
uint testCodeIndex = 0;

void setup() 
{
	Log.info("Started.");
	Serial1.begin(9600, SERIAL_8N1);
}

void loop() 
{
	ControllerLoop();
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
	/*
	String testCode = testCodes[testCodeIndex];
	const char * serialMessage = testCode.c_str();
	Log.info("Sending code: %s", serialMessage);
	size_t bytesWritten = Serial1.write(serialMessage);
	Log.info("Wrote %i bytes.", bytesWritten);
	*/
	for (int i = 0; i < 5; i++)
	{
		Serial1.write(testCharacterCodes[testCodeIndex][i]);
	}
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