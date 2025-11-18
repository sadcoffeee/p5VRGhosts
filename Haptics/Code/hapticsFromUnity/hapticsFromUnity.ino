//Pins
int procedureMotor = 3;
int playMotor = 5;
int motorValue = 0;
int errorLed = 7;
int timingLed = 8;

//Motor timing
unsigned long lastPlayMotorMessageTime = 0;
const unsigned long playMotorTimeout = 100;
unsigned long lastProcedureMotorMessageTime = 0;
const unsigned long procedureMotorTimeout = 100;

//failed connection
bool connectionError = false;
unsigned long lastConnectionCheck = 0;
const unsigned long connectionCheckInterval = 500;
//failed connection blinking
unsigned long lastErrorBlink = 0;
const unsigned long errorBlinkInterval = 500;

//failed message
bool failedMessage = false;
unsigned long failedMessageTime = 0;
const unsigned long failedMessageBlinkOnTime = 1000;
//failed message blinking
unsigned long failedMessageLast = 0;
const unsigned long failedMessageBlinkInterval = 100;

void setup() {
  Serial.begin(115200);
  //LED
  pinMode(errorLed,OUTPUT);
  pinMode(timingLed,OUTPUT);

  //Motors
  pinMode(procedureMotor, OUTPUT);
  pinMode(playMotor,OUTPUT);

  //zero OUTPUTS
  analogWrite(procedureMotor, 0);
  analogWrite(playMotor, 0);
  digitalWrite(errorLed, LOW);
  digitalWrite(timingLed, LOW);
}

void loop() {
  if (Serial.available() > 0){
    String input = Serial.readStringUntil('\n');
    input.trim();

    if (input.length() > 0){
      lastConnectionCheck = millis(); // resets connection timer if a message is recived
      if (connectionError) {
        connectionError = false;
        digitalWrite(errorLed, LOW);
      }

      String code = input.substring(0,2); //checks first to characters
      int motorValue = input.substring(2).toInt(); //checks the rest

      motorValue = constrain(motorValue,0,255);

      if (input != "connectionCheck") {
        if (code == "PC") analogWrite(procedureMotor, motorValue);
        else if (code == "PL") analogWrite(playMotor, motorValue);
        else if (code == "TN") digitalWrite(timingLed, HIGH);
        else if (code == "TF") digitalWrite(timingLed, LOW);
        else {
          failedMessageTime = millis();
          failedMessage = true;
        }
        if (code == "PC") lastProcedureMotorMessageTime = millis();
        if (code == "PL") lastPlayMotorMessageTime = millis();
      }
    }
  }

  //checks if there has been a connection confirmation within 500 ms
  if (millis() - lastConnectionCheck > connectionCheckInterval) {
    connectionError = true;
  }

  //check time since last motor commands
  if (millis() - lastPlayMotorMessageTime > playMotorTimeout) {
    analogWrite(playMotor, 0);
  }
  if (millis() - lastProcedureMotorMessageTime > procedureMotorTimeout) {
    analogWrite(procedureMotor, 0);
  }

  //makes the LED blink if there is a connection error;
  if (connectionError){
    StopAll();
    if (millis() - lastErrorBlink >= errorBlinkInterval) {
      digitalWrite(errorLed, !digitalRead(errorLed));
      lastErrorBlink = millis();
    }
  }

  //makes error LED blink if it recives a failed message
  if (failedMessage) {
    if (millis() - failedMessageLast >= failedMessageBlinkInterval){
      digitalWrite(errorLed, !digitalRead(errorLed));
      failedMessageLast = millis();
    }
  }

  //resets failed message after 1 sec
  if (millis() - failedMessageTime >= failedMessageBlinkOnTime && failedMessage){
    digitalWrite(errorLed, LOW);
    failedMessage = false;
  }
}

void  StopAll(){
  analogWrite(playMotor, 0);
  analogWrite(procedureMotor, 0);
  digitalWrite(timingLed, LOW);
}

